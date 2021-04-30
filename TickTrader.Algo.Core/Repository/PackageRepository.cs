using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Core.Repository
{
    internal enum UpdateAction { Upsert, Remove }

    internal struct PackageFileUpdate
    {
        internal string PackageId { get; }

        internal UpdateAction Action { get; }

        internal string FilePath { get; }


        public PackageFileUpdate(string packageId, UpdateAction action, string filePath)
        {
            PackageId = packageId;
            Action = action;
            FilePath = filePath;
        }
    }


    public class PackageRepository : IDisposable
    {
        private object _scanUpdateLockObj = new object();
        private FileSystemWatcher _watcher;
        private Task _scanTask;
        private string _repPath;
        private string _location;
        private IAlgoLogger _logger;
        private bool _isolated;
        private readonly IAsyncChannel<PackageFileUpdate> _updateChannel;
        private bool _enabled;


        internal PackageRepository(string repPath, string locationId, IAsyncChannel<PackageFileUpdate> updateChannel, IAlgoLogger logger = null, bool isolated = true)
        {
            _repPath = repPath;
            _location = locationId;
            _updateChannel = updateChannel;
            _logger = logger;
            _isolated = isolated;
        }


        public void Start()
        {
            _enabled = true;
            _scanTask = Task.Factory.StartNew(Scan);
        }

        public async Task Stop()
        {
            _enabled = false;
            try
            {
                await _scanTask;
            }
            catch (Exception) { }
            DeinitWatcher();

        }

        public void Dispose()
        {
            DeinitWatcher();
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

            _watcher.Dispose();
            _watcher.Changed -= WatcherOnChanged;
            _watcher.Created -= WatcherOnChanged;
            _watcher.Deleted -= WatcherOnDeleted;
            _watcher.Renamed -= WatcherOnRenamed;
            _watcher.Error -= WatcherOnError;

            _watcher = null;
        }

        private void Scan()
        {
            lock (_scanUpdateLockObj)
            {
                if (!_enabled)
                    return;

                try
                {
                    DeinitWatcher();

                    InitWatcher();

                    _watcher.EnableRaisingEvents = true;

                    var fileList = Directory.GetFiles(_repPath);
                    foreach (var file in fileList)
                    {
                        if (!PackageHelper.IsFileSupported(file))
                            continue;

                        UpsertPackage(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"Failed to scan {_repPath}");
                }
            }
        }

        private void WatcherOnError(object sender, ErrorEventArgs e)
        {
            _logger.Error(e.GetException(), $"Location = {_location}: Watcher error");

            Task.Factory.StartNew(Scan);
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            _logger.Debug($"Location = {_location}: Watcher renamed '{e.OldFullPath}' into '{e.FullPath}' ({e.ChangeType})");

            if (PackageHelper.IsFileSupported(e.OldFullPath))
                RemovePackage(e.OldFullPath);

            if (PackageHelper.IsFileSupported(e.FullPath))
                UpsertPackage(e.FullPath);

        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            _logger.Debug($"Location = {_location}: Watcher changed '{e.FullPath}' ({e.ChangeType})");

            if (PackageHelper.IsFileSupported(e.FullPath))
                UpsertPackage(e.FullPath);
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            _logger.Debug($"Location = {_location}: Watcher deleted '{e.FullPath}' ({e.ChangeType})");

            if (PackageHelper.IsFileSupported(e.FullPath))
                RemovePackage(e.FullPath);
        }

        private void UpsertPackage(string path)
        {

            _logger.Debug($"Location = {_location}: Upsert package '{path}'");

            var id = PackageHelper.GetPackageIdFromPath(_location, path);
            _updateChannel.AddAsync(new PackageFileUpdate(id, UpdateAction.Upsert, path));
        }

        private void RemovePackage(string path)
        {
            _logger.Debug($"Location = {_location}: Remove package '{path}'");

            var id = PackageHelper.GetPackageIdFromPath(_location, path);
            _updateChannel.AddAsync(new PackageFileUpdate(id, UpdateAction.Remove, path));
        }
    }
}
