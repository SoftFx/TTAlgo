using ActorSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Package
{
    internal enum UpdateAction { Upsert, Remove }

    internal struct PackageFileUpdate
    {
        public string PkgId { get; }

        public UpdateAction Action { get; }

        public PackageInfo PkgInfo { get; }

        public byte[] PkgBytes { get; }


        public PackageFileUpdate(string pkgId, UpdateAction action, PackageInfo pkgInfo, byte[] pkgBytes)
        {
            PkgId = pkgId;
            Action = action;
            PkgInfo = pkgInfo;
            PkgBytes = pkgBytes;
        }
    }


    internal class PackageRepository
    {
        public const int MaxPkgSize = 256 * 1024 * 1024;

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<PackageRepository>();

        private readonly Ref<Impl> _impl;


        internal PackageRepository(string repPath, string locationId, IAsyncChannel<PackageFileUpdate> updateChannel)
        {
            _impl = Actor.SpawnLocal<Impl>(null, $"PackageRepository {locationId}");

            _impl.Send(a => a.Init(repPath, locationId, updateChannel));
        }


        public Task Start()
        {
            return _impl.Call(a => a.Start());
        }

        public Task Stop()
        {
            return _impl.Call(a => a.Stop());
        }

        public Task WaitLoaded()
        {
            var taskSrc = new TaskCompletionSource<object>();
            _impl.Send(a => taskSrc.TrySetResult(null));
            return taskSrc.Task;
        }


        private class Impl : Actor
        {
            private readonly Dictionary<string, PackageIdentity> _pkgIdentityCache = new Dictionary<string, PackageIdentity>();

            private string _repPath;
            private string _location;
            private IAsyncChannel<PackageFileUpdate> _updateChannel;
            private bool enabled;
            private FileSystemWatcher _watcher;


            public void Init(string repPath, string location, IAsyncChannel<PackageFileUpdate> updateChannel)
            {
                _repPath = repPath;
                _location = location;
                _updateChannel = updateChannel;

                _logger?.Debug($"Location = {_location}: Initialized");
            }

            public void Start()
            {
                enabled = true;
                Scan();
            }

            public void Stop()
            {
                enabled = false;
                DeinitWatcher();
            }


            private void Scan()
            {
                if (!enabled)
                    return;

                _logger?.Debug($"Location = {_location}: Starting scan at '{_repPath}'");

                try
                {
                    DeinitWatcher();

                    InitWatcher();

                    _watcher.EnableRaisingEvents = true;

                    var cnt = 0;
                    var fileList = Directory.GetFiles(_repPath);
                    foreach (var file in fileList)
                    {
                        if (!PackageHelper.IsFileSupported(file))
                            continue;

                        SchedulePackageUpsert(file);
                        cnt++;
                    }

                    _logger?.Debug($"Location = {_location}: Scan finished found {cnt} packages");
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"Failed to scan '{_repPath}'");
                }
            }

            private void SchedulePackageUpsert(string path)
            {
                ContextSend(() => UpsertPackage(path));
            }

            private void SchedulePackageRemove(string path)
            {
                ContextSend(() => RemovePackage(path));
            }

            private void UpsertPackage(string path)
            {
                if (!enabled)
                    return;

                _logger?.Debug($"Location = {_location}: Upsert package '{path}'");

                var pkgId = PackageId.FromPath(_location, path);
                _pkgIdentityCache.TryGetValue(pkgId, out var pkgIdentity);
                byte[] fileBytes = null;
                try // loading file in memory
                {
                    using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                    {
                        var fileInfo = new FileInfo(path);
                        var pkgSize = fileInfo.Length;

                        if (pkgSize > MaxPkgSize)
                        {
                            _logger?.Error($"Location = {_location}: Algo package size({pkgSize}) exceeds limit '{path}'");
                            return;
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
                            _logger?.Debug($"Location = {_location}: Algo package scan skipped at '{path}'");
                            return;
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
                    if (ioEx.IsLockExcpetion())
                    {
                        _logger?.Debug($"Location = {_location}: Algo package is locked at '{path}'");
                    }
                    else
                    {
                        _logger?.Info($"Location = {_location}: Can't open Algo package at '{path}': {ioEx.Message}"); // other errors
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"Location = {_location}: Failed to load Algo package at '{path}'");
                }

                if (fileBytes == null)
                    return; // load skipped or failed

                var pkgInfo = new PackageInfo { PackageId = pkgId, Identity = pkgIdentity, IsValid = false, };
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
                    _logger?.Error(ex, $"Location = {_location}: Failed to scan Algo package at '{path}'");
                }

                _pkgIdentityCache[pkgId] = pkgIdentity;
                _updateChannel.AddAsync(new PackageFileUpdate(pkgId, UpdateAction.Upsert, pkgInfo, pkgInfo.IsValid ? fileBytes : null));
            }

            private void RemovePackage(string path)
            {
                _logger?.Debug($"Location = {_location}: Remove package '{path}'");

                var pkgId = PackageId.FromPath(_location, path);
                _pkgIdentityCache.Remove(pkgId);
                _updateChannel.AddAsync(new PackageFileUpdate(pkgId, UpdateAction.Remove, null, null));
            }

            private void InitWatcher()
            {
                _watcher = new FileSystemWatcher()
                {
                    Path = _repPath,
                    IncludeSubdirectories = false
                };

                _watcher.Changed += WatcherOnChanged;
                _watcher.Created += WatcherOnChanged;
                _watcher.Deleted += WatcherOnDeleted;
                _watcher.Renamed += WatcherOnRenamed;
                _watcher.Error += WatcherOnError;
            }

            private void DeinitWatcher()
            {
                if (_watcher == null)
                    return;

                _watcher.Changed -= WatcherOnChanged;
                _watcher.Created -= WatcherOnChanged;
                _watcher.Deleted -= WatcherOnDeleted;
                _watcher.Renamed -= WatcherOnRenamed;
                _watcher.Error -= WatcherOnError;

                _watcher.Dispose();

                _watcher = null;
            }

            private void WatcherOnError(object sender, ErrorEventArgs e)
            {
                _logger?.Error(e.GetException(), $"Location = {_location}: Watcher error");

                ContextSend(Scan); // restart watcher
            }

            private void WatcherOnRenamed(object sender, RenamedEventArgs e)
            {
                _logger?.Debug($"Location = {_location}: Watcher renamed '{e.OldFullPath}' into '{e.FullPath}' ({e.ChangeType})");

                if (PackageHelper.IsFileSupported(e.OldFullPath))
                    SchedulePackageRemove(e.OldFullPath);

                if (PackageHelper.IsFileSupported(e.FullPath))
                    SchedulePackageUpsert(e.FullPath);

            }

            private void WatcherOnChanged(object sender, FileSystemEventArgs e)
            {
                _logger?.Debug($"Location = {_location}: Watcher changed '{e.FullPath}' ({e.ChangeType})");

                if (PackageHelper.IsFileSupported(e.FullPath))
                    SchedulePackageUpsert(e.FullPath);
            }

            private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
            {
                _logger?.Debug($"Location = {_location}: Watcher deleted '{e.FullPath}' ({e.ChangeType})");

                if (PackageHelper.IsFileSupported(e.FullPath))
                    SchedulePackageRemove(e.FullPath);
            }
        }
    }
}
