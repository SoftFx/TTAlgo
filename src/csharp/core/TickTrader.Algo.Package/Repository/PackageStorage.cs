using ActorSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Package
{
    public class PackageVersionUpdate
    {
        public string PackageId { get; }

        public string LatestPkgRefId { get; }


        public PackageVersionUpdate(string packageId, string latestPkgRefId)
        {
            PackageId = packageId;
            LatestPkgRefId = latestPkgRefId;
        }
    }


    public class PackageStorage
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PackageStorage>();

        private readonly Ref<Impl> _impl;
        private readonly ChannelEventSource<PackageUpdate> _pkgUpdateEventSrc = new ChannelEventSource<PackageUpdate>();
        private readonly ChannelEventSource<PackageVersionUpdate> _pkgVersionUpdateEventSrc = new ChannelEventSource<PackageVersionUpdate>();

        public IEventSource<PackageUpdate> PackageUpdated => _pkgUpdateEventSrc;

        public IEventSource<PackageVersionUpdate> PackageVersionUpdated => _pkgVersionUpdateEventSrc;


        public PackageStorage()
        {
            _impl = Actor.SpawnLocal<Impl>(null, nameof(PackageStorage));

            _impl.Send(a => a.Init(_pkgUpdateEventSrc.Writer, _pkgVersionUpdateEventSrc.Writer));
        }

        public Task RegisterRepositoryLocation(string locationId, string path, bool isUploadLocation)
        {
            return _impl.Call(a => a.RegisterRepository(locationId, path, isUploadLocation));
        }

        public Task RegisterAssemblyAsPackage(Assembly assembly)
        {
            return _impl.Call(a => a.RegisterAssembly(assembly));
        }

        public Task<AlgoPackageRef> GetPackageRef(string pkgId)
        {
            return _impl.Call(a => a.GetPackageRef(pkgId));
        }

        public Task<bool> LockPackageRef(string pkgRefId) => _impl.Call(a => a.LockPackageRef(pkgRefId));

        public void ReleasePackageRef(string pkgRefId) => _impl.Call(a => a.ReleasePackageRef(pkgRefId));

        public Task<string> GetPackageRefPath(string pkgRefId) => _impl.Call(a => a.GetPackageRefPath(pkgRefId));

        public void ReleasePackageRef(AlgoPackageRef pkgRef)
        {
            _impl.Send(a => a.ReleasePackageRef(pkgRef));
        }

        public Task Stop()
        {
            _pkgUpdateEventSrc.Dispose();
            return _impl.Call(a => a.Stop());
        }

        public Task WaitLoaded()
        {
            var taskSrc = new TaskCompletionSource<object>();
            _impl.Send(a => a.WhenLoaded(() => taskSrc.TrySetResult(null)));
            return taskSrc.Task;
        }

        public Task<byte[]> GetPackageBinary(string pkgId)
        {
            return _impl.Call(a => a.GetPackageBinary(pkgId));
        }

        public Task UploadPackage(UploadPackageRequest request, string pkgFilePath)
        {
            return _impl.Call(a => a.UploadPackage(request, pkgFilePath));
        }

        public Task RemovePackage(RemovePackageRequest request)
        {
            return _impl.Call(a => a.RemovePackage(request));
        }

        public Task<bool> PackageWithNameExists(string pkgName)
        {
            return _impl.Call(a => a.PackageWithNameExists(pkgName));
        }

        public Task<List<PackageInfo>> GetPackageSnapshot()
        {
            return _impl.Call(a => a.GetPackageSnapshot());
        }


        private class Impl : Actor
        {
            private readonly Dictionary<string, PackageRepository> _repositories = new Dictionary<string, PackageRepository>();
            private readonly AsyncChannelProcessor<PackageFileUpdate> _fileUpdateProcessor = AsyncChannelProcessor<PackageFileUpdate>.CreateUnbounded($"{nameof(PackageStorage)} package loop", true);
            private readonly Dictionary<string, int> _packageVersions = new Dictionary<string, int>();
            private readonly Dictionary<string, string> _pkgIdToRefMap = new Dictionary<string, string>();
            private readonly Dictionary<string, AlgoPackageRef> _pkgRefMap = new Dictionary<string, AlgoPackageRef>();

            private ChannelWriter<PackageUpdate> _pkgUpdateSink;
            private ChannelWriter<PackageVersionUpdate> _pkgVersionUpdateSink;
            private string _uploadLocationId, _uploadDir;


            public void Init(ChannelWriter<PackageUpdate> pkgUpdateSink, ChannelWriter<PackageVersionUpdate> pkgVersionUpdateSink)
            {
                _pkgUpdateSink = pkgUpdateSink;
                _pkgVersionUpdateSink = pkgVersionUpdateSink;

                _fileUpdateProcessor.Start(HandlePackageUpdate);
            }

            public async Task Stop()
            {
                _logger.Debug("Stopping...");

                await _fileUpdateProcessor.Stop(false);
                _logger.Debug("File update processor stopped");
                _pkgUpdateSink.TryComplete();
                _logger.Debug("Marked update stream complete");
                await Task.WhenAll(_repositories.Values.Select(r => r.Stop()));
                _logger.Debug("Package repositories stopped");

                _logger.Debug("Stopped");
            }

            public void WhenLoaded(Action continuation)
            {
                Task.WhenAll(_repositories.Values.Select(r => r.WaitLoaded()))
                    .ContinueWith(t => continuation());
            }

            public Task RegisterRepository(string locationId, string path, bool isUploadLocation)
            {
                if (_repositories.ContainsKey(locationId))
                    throw new ArgumentException($"Cannot register multiple paths for location '{locationId}'");

                if (isUploadLocation)
                {
                    if (!string.IsNullOrEmpty(_uploadLocationId))
                        throw new ArgumentException($"Upload location already set to '{_uploadLocationId}' ({_uploadDir})");

                    _uploadLocationId = locationId;
                    _uploadDir = path;
                }

                PathHelper.EnsureDirectoryCreated(path);

                var repo = new PackageRepository(path, locationId, _fileUpdateProcessor);
                _repositories[locationId] = repo;
                return repo.Start(); // wait for initial directory scan
            }

            public void RegisterAssembly(Assembly assembly)
            {
                var assemblyPath = assembly.Location;
                var pkgId = PackageId.FromPath(SharedConstants.EmbeddedRepositoryId, assembly.Location);

                var pkgInfo = PackageExplorer.ScanAssembly(pkgId, assembly);

                pkgInfo.Identity = PackageIdentity.CreateInvalid(assemblyPath);
                try
                {
                    var fileInfo = new FileInfo(assemblyPath);
                    pkgInfo.Identity = PackageIdentity.Create(fileInfo, assembly.FullName);
                    pkgInfo.IsValid = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to get assembly file info '{assemblyPath}'");
                }

                HandlePackageUpdate(new PackageFileUpdate(pkgId, UpdateAction.Upsert, pkgInfo));
            }

            public AlgoPackageRef GetPackageRef(string pkgId)
            {
                if (TryGetPackageRefByPkgId(pkgId, out var pkgRef))
                {
                    pkgRef.IncrementRef();
                    return pkgRef;
                }

                return null;
            }

            public void ReleasePackageRef(AlgoPackageRef pkgRef)
            {
                pkgRef?.DecrementRef();
            }

            public bool LockPackageRef(string pkgRefId)
            {
                if (_pkgRefMap.TryGetValue(pkgRefId, out var pkgRef))
                {
                    pkgRef.IncrementRef();
                    return true;
                }

                return false;
            }

            public void ReleasePackageRef(string pkgRefId)
            {
                if (_pkgRefMap.TryGetValue(pkgRefId, out var pkgRef))
                    pkgRef.DecrementRef();

            }

            public string GetPackageRefPath(string pkgRefId)
            {
                if (_pkgRefMap.TryGetValue(pkgRefId, out var pkgRef))
                    return pkgRef.Identity.FilePath;

                throw new AlgoException($"Package ref '{pkgRefId}' doesn't exist");
            }

            public byte[] GetPackageBinary(string pkgId)
            {
                if (TryGetPackageRefByPkgId(pkgId, out var pkgRef))
                    return pkgRef.PackageBytes;

                throw new AlgoException($"Package '{pkgId}' doesn't exist");
            }

            public void UploadPackage(UploadPackageRequest request, string pkgFilePath)
            {
                (var pkgId, var filename) = request;

                if (!string.IsNullOrEmpty(pkgId) && !string.IsNullOrEmpty(filename))
                    throw new ArgumentException($"Both {nameof(request.PackageId)} and {nameof(request.Filename)} can't be specified");
                if (string.IsNullOrEmpty(pkgId) && string.IsNullOrEmpty(filename))
                    throw new ArgumentException($"{nameof(request.PackageId)} or {nameof(request.Filename)} should be specified");

                PathHelper.EnsureDirectoryCreated(_uploadDir);

                if (string.IsNullOrEmpty(pkgId))
                    pkgId = PackageId.Pack(_uploadLocationId, filename);

                var pkgPath = string.Empty;
                if (TryGetPackageRefByPkgId(pkgId, out var pkgRef))
                {
                    if (pkgRef.IsLocked)
                        throw new AlgoException($"Can't update Algo package '{pkgId}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                    // update existing file
                    pkgPath = pkgRef.Identity.FilePath;
                }
                else
                {
                    if (string.IsNullOrEmpty(filename)) // update-only mode request.PackageId was specified
                        throw new AlgoException($"Package '{pkgId}' doesn't exist");

                    // add new file
                    pkgPath = Path.Combine(_uploadDir, filename);
                }

                try
                {
                    if (File.Exists(pkgPath))
                        File.Delete(pkgPath);
                    File.Move(pkgFilePath, pkgPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error saving Algo package '{pkgId}' at '{pkgPath}'");
                }
            }

            public void RemovePackage(RemovePackageRequest request)
            {
                var pkgId = request.PackageId;
                if (!TryGetPackageRefByPkgId(pkgId, out var pkgRef))
                    throw new AlgoException($"Package '{pkgId}' doesn't exist");
                if (pkgRef.IsLocked)
                    throw new AlgoException($"Can't remove Algo package '{pkgId}': one or more trade bots from this package is being executed! Please stop all bots and try again!");

                var pkgPath = pkgRef.Identity.FilePath;
                try
                {
                    File.Delete(pkgPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error deleting Algo package '{pkgId}' at '{pkgPath}'");
                }
            }

            public bool PackageWithNameExists(string pkgName)
            {
                var pkgId = PackageId.Pack(_uploadLocationId, pkgName);
                return _pkgIdToRefMap.TryGetValue(pkgId, out _);
            }

            public List<PackageInfo> GetPackageSnapshot()
            {
                var res = new List<PackageInfo>(_pkgIdToRefMap.Count);
                foreach(var pkgRefId in _pkgIdToRefMap.Values)
                {
                    if (_pkgRefMap.TryGetValue(pkgRefId, out var pkgRef))
                        res.Add(pkgRef.PackageInfo);
                }
                return res;
            }


            private void HandlePackageUpdate(PackageFileUpdate update)
            {
                var pkgId = update.PkgId;
                if (TryGetPackageRefByPkgId(pkgId, out var pkgRef))
                    pkgRef?.SetObsolete();

                switch (update.Action)
                {
                    case UpdateAction.Upsert:
                        var refId = GeneratePackageRefId(pkgId);
                        _pkgIdToRefMap[pkgId] = refId;
                        _pkgRefMap[refId] = new AlgoPackageRef(refId, update.PkgInfo, update.PkgBytes);
                        _pkgUpdateSink.TryWrite(new PackageUpdate { Action = Domain.Package.Types.UpdateAction.Upsert, Id = pkgId, Package = update.PkgInfo, });
                        break;
                    case UpdateAction.Remove:
                        _pkgIdToRefMap.Remove(pkgId);
                        _pkgRefMap.Remove(pkgRef?.Id);
                        _pkgUpdateSink.TryWrite(new PackageUpdate { Action = Domain.Package.Types.UpdateAction.Removed, Id = pkgId, });
                        break;
                }
            }

            private string GeneratePackageRefId(string pkgId)
            {
                if (!_packageVersions.TryGetValue(pkgId, out var currentVersion))
                    currentVersion = -1;

                currentVersion++;
                _packageVersions[pkgId] = currentVersion;
                var pkgRefId = $"{pkgId}/{currentVersion}";
                _pkgVersionUpdateSink.TryWrite(new PackageVersionUpdate(pkgId, pkgRefId));
                return pkgRefId;
            }

            private bool TryGetPackageRefByPkgId(string pkgId, out AlgoPackageRef pkgRef)
            {
                pkgRef = default;
                return _pkgIdToRefMap.TryGetValue(pkgId, out var pkgRefId) && _pkgRefMap.TryGetValue(pkgRefId, out pkgRef);
            }
        }
    }
}
