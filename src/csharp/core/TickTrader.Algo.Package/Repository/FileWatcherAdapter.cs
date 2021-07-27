using System.IO;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Package
{
    internal class FileWatcherAdapter
    {
        private readonly IActorRef _parent;
        private readonly IAlgoLogger _logger;

        private FileSystemWatcher _watcher;


        public FileWatcherAdapter(IActorRef parent, IAlgoLogger logger)
        {
            _parent = parent;
            _logger = logger;
        }


        public void Init(string path)
        {
            _watcher = new FileSystemWatcher()
            {
                Path = path,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };

            _watcher.Changed += WatcherOnChanged;
            _watcher.Created += WatcherOnChanged;
            _watcher.Deleted += WatcherOnDeleted;
            _watcher.Renamed += WatcherOnRenamed;
            _watcher.Error += WatcherOnError;
        }

        public void Deinit()
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


        private void SendPackageUpsert(string path)
        {
            if (PackageHelper.IsFileSupported(path))
                _parent.Tell(new FileUpsertMsg(path));
        }

        private void SendPackageRemoved(string path)
        {
            if (PackageHelper.IsFileSupported(path))
                _parent.Tell(new FileRemovedMsg(path));
        }

        private void WatcherOnError(object sender, ErrorEventArgs e)
        {
            _logger?.Error(e.GetException(), $"Watcher error");

            _parent.Tell(RestartRequiredMsg.Instance); // restart watcher
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            _logger?.Debug($"Watcher renamed '{e.OldFullPath}' into '{e.FullPath}' ({e.ChangeType})");

            SendPackageRemoved(e.OldFullPath);
            SendPackageUpsert(e.FullPath);
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            _logger?.Debug($"Watcher changed '{e.FullPath}' ({e.ChangeType})");

            SendPackageUpsert(e.FullPath);
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            _logger?.Debug($"Watcher deleted '{e.FullPath}' ({e.ChangeType})");

            SendPackageRemoved(e.FullPath);
        }


        internal class RestartRequiredMsg : Singleton<RestartRequiredMsg> { }

        internal class FileUpsertMsg // file created or changed
        {
            public string Path { get; }

            public FileUpsertMsg(string path)
            {
                Path = path;
            }
        }

        internal class FileRemovedMsg
        {
            public string Path { get; }

            public FileRemovedMsg(string path)
            {
                Path = path;
            }
        }
    }
}
