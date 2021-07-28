using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    internal sealed class PackageState
    {
        public string Id { get; set; }

        public PackageIdentity Identity { get; set; }

        public PkgUpdateAction? NextAction { get; set; }

        public bool IsLoading { get; set; }
    }


    public sealed class PackageRepository : Actor
    {
        private readonly string _path, _locationId;
        private readonly ChannelWriter<PackageFileUpdate> _updateSink;
        private readonly Dictionary<string, PackageState> _pkgStateCache = new Dictionary<string, PackageState>();

        private IAlgoLogger _logger;
        private bool _enabled;
        private FileWatcherAdapter _watcher;
        private TaskCompletionSource<object> _whenIdleSrc;


        private PackageRepository(string path, string locationId, ChannelWriter<PackageFileUpdate> updateSink)
        {
            _path = path;
            _locationId = locationId;
            _updateSink = updateSink;

            Receive<StartCmd>(Start);
            Receive<StopCmd>(Stop);
            Receive<WhenIdleRequest, Task>(WhenIdle);
            Receive<PackageLoadResult>(OnPackageLoaded);
        }


        public static IActorRef Create(string path, string locationId, ChannelWriter<PackageFileUpdate> updateSink)
        {
            return ActorSystem.SpawnLocal(() => new PackageRepository(path, locationId, updateSink), $"{nameof(PackageRepository)} ({locationId})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
            _watcher = new FileWatcherAdapter(Self, _logger);
        }


        private void Start(StartCmd cmd)
        {
            _enabled = true;
            Scan();
        }

        private void Stop(StopCmd cmd)
        {
            _enabled = false;
            _whenIdleSrc?.TrySetResult(null);
            _watcher.Deinit();
        }

        private Task WhenIdle(WhenIdleRequest request)
        {
            if (!_enabled)
                throw new InvalidOperationException($"{Name} not started");

            if (IsIdle())
                return Task.CompletedTask;

            if (_whenIdleSrc == null)
                _whenIdleSrc = new TaskCompletionSource<object>();

            return _whenIdleSrc.Task;
        }

        private void OnPackageLoaded(PackageLoadResult result)
        {
            (var path, var fileUpdate) = result;

            if (!_pkgStateCache.TryGetValue(path, out var pkgState))
            {
                _logger?.Error($"Missing package state for '{path}'");
                return;
            }

            pkgState.IsLoading = false;
            if (fileUpdate != null)
            {
                _logger?.Debug($"Loaded package '{path}'");
                pkgState.Identity = fileUpdate.PkgInfo.Identity;
                _updateSink.TryWrite(fileUpdate);
            }

            if (pkgState.NextAction != null)
            {
                switch (pkgState.NextAction.Value)
                {
                    case PkgUpdateAction.Upsert:
                        UpsertPackage(path);
                        break;
                    case PkgUpdateAction.Remove:
                        RemovePackage(path);
                        break;
                }
            }

            if (_whenIdleSrc != null && IsIdle())
            {
                _whenIdleSrc.TrySetResult(null);
                _whenIdleSrc = null;
            }
        }


        private void Scan()
        {
            if (!_enabled)
                return;

            _logger.Debug($"Starting scan at '{_path}'");

            try
            {
                var cnt = 0;
                var fileList = Directory.GetFiles(_path);

                _watcher.Deinit();
                _watcher.Init(_path);

                foreach (var file in fileList)
                    if (PackageHelper.IsFileSupported(file))
                    {
                        UpsertPackage(file);
                        cnt++;
                    }

                _logger.Debug($"Scan finished found {cnt} packages");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to scan '{_path}'");
            }
        }

        private void UpsertPackage(string path)
        {
            if (!_enabled)
                return;

            _logger.Debug($"Upsert package '{path}'");

            if (!_pkgStateCache.TryGetValue(path, out var pkgState))
            {
                pkgState = new PackageState { Id = PackageId.FromPath(_locationId, path) };
                _pkgStateCache[path] = pkgState;
            }

            if (pkgState.IsLoading)
            {
                pkgState.NextAction = PkgUpdateAction.Upsert;
            }
            else
            {
                pkgState.IsLoading = true;
                pkgState.NextAction = null;

                Task.Factory.StartNew(() => LoadPackage(new PackageLoadRequest(pkgState.Id, path, pkgState.Identity), _logger),
                    CancellationToken.None, TaskCreationOptions.None, ParallelTaskScheduler.ShortRunning)
                    .ContinueWith(t => Self.Tell(t.IsCompleted ? t.Result : new PackageLoadResult(path)));
            }
        }

        private void RemovePackage(string path)
        {
            _logger.Debug($"Remove package '{path}'");

            _pkgStateCache.TryGetValue(path, out var pkgState);

            if (pkgState?.IsLoading ?? false)
            {
                pkgState.NextAction = PkgUpdateAction.Remove;
            }
            else
            {
                var pkgId = PackageId.FromPath(_locationId, path);

                _pkgStateCache.Remove(path);
                _updateSink.TryWrite(new PackageFileUpdate(pkgId, PkgUpdateAction.Remove));
            }
        }

        private bool IsIdle() => _pkgStateCache.Values.All(p => !p.IsLoading);

        private static PackageLoadResult LoadPackage(PackageLoadRequest request, IAlgoLogger logger)
        {
            (var pkgId, var path, var pkgIdentity) = request;
            var res = new PackageLoadResult(path);
            byte[] fileBytes = null;

            logger?.Debug($"Loading package '{path}'");

            try // loading file in memory
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    var fileInfo = new FileInfo(path);
                    var pkgSize = fileInfo.Length;

                    if (pkgSize > PackageLoader.MaxZipPkgSize)
                    {
                        logger?.Error($"Algo package size({pkgSize}) exceeds limit '{path}'");
                        return res;
                    }

                    var skipFileScan = pkgIdentity != null
                        && pkgSize == pkgIdentity.Size
                        && fileInfo.CreationTimeUtc == pkgIdentity.CreatedUtc.ToDateTime()
                        && fileInfo.LastWriteTimeUtc == pkgIdentity.LastModifiedUtc.ToDateTime();

                    // FileSystemWatcher produces many events
                    // Package scans might take a while
                    // Therefore we can have many events asking to load this file without real need
                    if (skipFileScan)
                    {
                        logger?.Debug($"Algo package scan skipped at '{path}'");
                        return res;
                    }

                    fileBytes = new byte[pkgSize];

                    using (var memStream = new MemoryStream(fileBytes))
                    {
                        stream.CopyTo(memStream);
                    }

                    pkgIdentity = PackageIdentity.Create(fileInfo, string.Empty);
                }
            }
            catch (IOException ioEx)
            {
                if (ioEx.IsLockException())
                    logger?.Debug($"Algo package is locked at '{path}'");
                else
                    logger?.Info($"Can't open Algo package at '{path}': {ioEx.Message}"); // other errors
            }
            catch (Exception ex)
            {
                logger?.Error(ex, $"Failed to load Algo package at '{path}'");
            }

            if (fileBytes == null)
                return res; // load skipped or failed

            var pkgInfo = new PackageInfo
            {
                PackageId = pkgId,
                Identity = pkgIdentity,
                IsValid = false,
            };

            try // scan package
            {
                using (var memStream = new MemoryStream(fileBytes))
                {
                    pkgIdentity.Hash = FileHelper.CalculateSha256Hash(memStream);
                }

                pkgInfo = PackageLoadContext.ReflectionOnlyLoad(pkgId, path);
                pkgInfo.Identity = pkgIdentity;
                pkgInfo.IsValid = true;
            }
            catch (Exception ex)
            {
                logger?.Error(ex, $"Failed to scan Algo package at '{path}'");
            }

            res.FileUpdate = new PackageFileUpdate(pkgId, PkgUpdateAction.Upsert, pkgInfo, fileBytes);
            return res;
        }


        public class StartCmd : Singleton<StartCmd> { }

        public class StopCmd : Singleton<StopCmd> { }

        public class WhenIdleRequest : Singleton<WhenIdleRequest> { }

        internal sealed class PackageLoadRequest
        {
            public string Id { get; }

            public string Path { get; }

            public PackageIdentity CachedIdentity { get; }


            public PackageLoadRequest(string id, string path, PackageIdentity cachedIdentity)
            {
                Id = id;
                Path = path;
                CachedIdentity = cachedIdentity;
            }

            public void Deconstruct(out string id, out string path, out PackageIdentity identity)
            {
                id = Id;
                path = Path;
                identity = CachedIdentity;
            }
        }

        internal sealed class PackageLoadResult
        {
            public string Path { get; }

            public PackageFileUpdate FileUpdate { get; set; }


            public PackageLoadResult(string path)
            {
                Path = path;
            }

            public void Deconstruct(out string path, out PackageFileUpdate update)
            {
                path = Path;
                update = FileUpdate;
            }
        }
    }
}
