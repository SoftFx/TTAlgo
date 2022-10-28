using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.PkgStorage
{
    public class PackageVersionUpdate
    {
        public string PkgId { get; }

        public string LatestPkgRefId { get; }

        public PackageVersionUpdate(string pkgId, string pkgRefId)
        {
            PkgId = pkgId;
            LatestPkgRefId = pkgRefId;
        }
    }


    public class PackageStorage
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PackageStorage>();

        private readonly IActorRef _eventBus;
        private readonly Dictionary<string, IActorRef> _repositories = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, int> _packageVersions = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _pkgIdToRefMap = new Dictionary<string, string>();
        private readonly Dictionary<string, PackageRef> _pkgRefMap = new Dictionary<string, PackageRef>();

        private string _uploadLocationId, _uploadDir;
        private Channel<PackageFileUpdate> _updateChannel;
        private Task _updateConsumerTask;
        private Action<PackageVersionUpdate> _pkgRefUpdateCallback;


        public PackageStorage(IActorRef eventBus)
        {
            _eventBus = eventBus;
        }


        public async Task Start(PkgStorageSettings settings, Action<PackageVersionUpdate> pkgRefUpdateCallback)
        {
            _pkgRefUpdateCallback = pkgRefUpdateCallback;

            _updateChannel = DefaultChannelFactory.CreateForManyToOne<PackageFileUpdate>();
            _updateConsumerTask = _updateChannel.Consume(HandlePackageUpdate);

            _uploadLocationId = settings.UploadLocationId;
            _uploadDir = settings.Locations.FirstOrDefault(l => l.LocationId == _uploadLocationId)?.Path;
            if (string.IsNullOrEmpty(_uploadDir))
                throw Errors.MissingPathForUploadLocation();

            foreach (var assembly in settings.Assemblies)
                RegisterAssembly(assembly);
            foreach (var loc in settings.Locations)
                RegisterRepository(loc.LocationId, loc.Path);

            await Task.WhenAll(_repositories.Values.Select(r => r.Ask(PackageRepository.StartCmd.Instance)));
        }

        public async Task Stop()
        {
            _logger.Debug("Stopping...");

            if (_updateChannel != null)
            {
                _updateChannel.Writer.TryComplete();
                _logger.Debug("Marked update stream complete");
            }
            if (_updateConsumerTask != null)
            {
                await _updateConsumerTask;
                _logger.Debug("PackageFileUpdate consumer stopped");
            }
            await Task.WhenAll(_repositories.Select(r => StopRepository(r.Key, r.Value)));
            _logger.Debug("Package repositories stopped");

            _logger.Debug("Stopped");
        }

        public async Task WhenLoaded()
        {
            _logger.Debug("Waiting for all packages to load...");

            await Task.WhenAll(_repositories.Values.Select(r => r.Ask<Task>(PackageRepository.WhenIdleRequest.Instance).Unwrap()));

            _logger.Debug("All packages loaded");
        }

        public bool LockPkgRef(string pkgRefId)
        {
            if (_pkgRefMap.TryGetValue(pkgRefId, out var pkgRef))
            {
                var gotLocked = pkgRef.IncrementRef();
                if (gotLocked && !pkgRef.IsObsolete)
                {
                    _eventBus.Tell(new PackageStateUpdate(pkgRef.PkgId, pkgRef.IsLocked));
                }
                return true;
            }

            return false;
        }

        public bool ReleasePkgRef(string pkgRefId)
        {
            if (_pkgRefMap.TryGetValue(pkgRefId, out var pkgRef))
            {
                var gotUnlocked = pkgRef.DecrementRef();
                if (gotUnlocked && !pkgRef.IsObsolete)
                {
                    _eventBus.Tell(new PackageStateUpdate(pkgRef.PkgId, pkgRef.IsLocked));
                }
                return true;
            }

            return false;
        }

        public byte[] GetPackageBinary(string pkgId)
        {
            if (!TryGetPkgRefByPkgId(pkgId, out var pkgRef))
                throw Errors.PackageNotFound(pkgId);

            return pkgRef.PkgBytes;
        }

        public bool PackageFileExists(string pkgFileName)
        {
            var pkgId = PackageId.Pack(_uploadLocationId, pkgFileName);
            return _pkgIdToRefMap.TryGetValue(pkgId, out _);
        }

        public string UploadPackage(UploadPackageRequest request, string pkgFilePath)
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
            if (TryGetPkgRefByPkgId(pkgId, out var pkgRef))
            {
                if (pkgRef.IsLocked)
                    throw Errors.PackageLocked(pkgId);

                // update existing file
                pkgPath = pkgRef.FilePath;
            }
            else
            {
                if (string.IsNullOrEmpty(filename)) // update-only mode request.PackageId was specified
                    throw Errors.PackageNotFound(pkgId);

                // add new file
                pkgPath = Path.Combine(_uploadDir, filename);
            }

            try
            {
                using (var dstFile = File.Open(pkgPath, FileMode.Create, FileAccess.Write))
                using (var srcFile = File.Open(pkgFilePath, FileMode.Open, FileAccess.Read))
                {
                    srcFile.CopyTo(dstFile);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error saving Algo package '{pkgId}' at '{pkgPath}'");
            }

            return pkgId;
        }

        public void RemovePackage(RemovePackageRequest request)
        {
            var pkgId = request.PackageId;
            if (!TryGetPkgRefByPkgId(pkgId, out var pkgRef))
                throw Errors.PackageNotFound(pkgId);
            if (pkgRef.IsLocked)
                throw Errors.PackageLocked(pkgId);

            var pkgPath = pkgRef.FilePath;
            try
            {
                File.Delete(pkgPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting Algo package '{pkgId}' at '{pkgPath}'");
            }
        }

        public PackageRef GetPkgRef(string pkgId)
        {
            TryGetPkgRefByPkgId(pkgId, out var pkgRef);
            return pkgRef;
        }


        private void RegisterRepository(string locationId, string path)
        {
            if (_repositories.ContainsKey(locationId))
                throw Errors.DuplicateLocationId(locationId);

            PathHelper.EnsureDirectoryCreated(path);

            var repo = PackageRepository.Create(path, locationId, _updateChannel.Writer);
            _repositories[locationId] = repo;
        }

        private async Task StopRepository(string locationId, IActorRef repo)
        {
            await repo.Ask(PackageRepository.StopCmd.Instance)
                .OnException(ex => _logger.Error(ex, $"Failed to stop repository '{locationId}'"));

            await ActorSystem.StopActor(repo)
                .OnException(ex => _logger.Error(ex, $"Failed to stop {repo.Name}"));
        }

        private void RegisterAssembly(Assembly assembly)
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

            HandlePackageUpdate(new PackageFileUpdate(pkgId, PkgUpdateAction.Upsert, pkgInfo));
        }

        private void HandlePackageUpdate(PackageFileUpdate update)
        {
            var pkgId = update.PkgId;
            if (TryGetPkgRefByPkgId(pkgId, out var pkgRef))
                pkgRef?.SetObsolete();

            switch (update.Action)
            {
                case PkgUpdateAction.Upsert:
                    var refId = GeneratePkgRefId(pkgId);
                    _pkgIdToRefMap[pkgId] = refId;
                    _pkgRefMap[refId] = new PackageRef(refId, update.PkgInfo, update.PkgBytes);
                    var pkgInfo = update.PkgInfo;
                    _eventBus.Tell(pkgRef != null ? PackageUpdate.Updated(pkgId, pkgInfo) : PackageUpdate.Added(pkgId, pkgInfo));
                    _pkgRefUpdateCallback?.Invoke(new PackageVersionUpdate(pkgId, refId));
                    break;
                case PkgUpdateAction.Remove:
                    _pkgIdToRefMap.Remove(pkgId);
                    _pkgRefMap.Remove(pkgRef?.Id);
                    _eventBus.Tell(PackageUpdate.Removed(pkgId));
                    _pkgRefUpdateCallback?.Invoke(new PackageVersionUpdate(pkgId, null));
                    break;
            }
        }

        private string GeneratePkgRefId(string pkgId)
        {
            if (!_packageVersions.TryGetValue(pkgId, out var currentVersion))
                currentVersion = -1;

            currentVersion++;
            _packageVersions[pkgId] = currentVersion;
            var pkgRefId = $"{pkgId}/{currentVersion}";
            return pkgRefId;
        }

        private bool TryGetPkgRefByPkgId(string pkgId, out PackageRef pkgRef)
        {
            pkgRef = default;
            return _pkgIdToRefMap.TryGetValue(pkgId, out var pkgRefId) && _pkgRefMap.TryGetValue(pkgRefId, out pkgRef);
        }
    }
}
