using Machinarium.State;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class PackageRepository : IDisposable
    {
        public enum States { Created, Scanning, Waiting, Watching, Closing, Closed }

        public enum Events { Start, DoneScanning, ScanFailed, NextAttempt, CloseRequested, DoneClosing, WatcherFail }


        private StateMachine<States> _stateControl = new StateMachine<States>();
        private object _scanUpdateLockObj = new object();
        private object _globalLockObj = new object();
        private FileSystemWatcher _watcher;
        private Task _scanTask;
        private Dictionary<string, PackageWatcher> _packages = new Dictionary<string, PackageWatcher>();
        private string _repPath;
        private RepositoryLocation _location;
        private IAlgoCoreLogger _logger;
        private bool _isolation;


        public event Action<AlgoPackageRef> Added;
        public event Action<AlgoPackageRef> Updated;
        public event Action<AlgoPackageRef> Removed;


        public IReadOnlyDictionary<string, PackageWatcher> Packages => _packages;


        public PackageRepository(string repPath, RepositoryLocation location, IAlgoCoreLogger logger = null, bool isolation = true)
        {
            _repPath = repPath;
            _location = location;
            _logger = logger;
            _isolation = isolation;

            _stateControl.AddTransition(States.Created, Events.Start, States.Scanning);
            _stateControl.AddTransition(States.Scanning, Events.DoneScanning, States.Watching);
            _stateControl.AddTransition(States.Scanning, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Scanning, Events.ScanFailed, States.Waiting);
            _stateControl.AddTransition(States.Waiting, Events.NextAttempt, States.Scanning);
            _stateControl.AddTransition(States.Waiting, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Watching, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Watching, Events.WatcherFail, States.Scanning);

            _stateControl.AddTransition(States.Closing, Events.DoneClosing, States.Closed);

            _stateControl.AddScheduledEvent(States.Waiting, Events.NextAttempt, 1000);

            _stateControl.OnEnter(States.Scanning, () => _scanTask = Task.Factory.StartNew(Scan));
        }

        public void Start()
        {
            _stateControl.PushEvent(Events.Start);
        }

        public Task Stop()
        {
            return _stateControl.PushEventAndWait(Events.CloseRequested, States.Closed);
        }

        public async Task WaitInit()
        {
            await _stateControl.AsyncWait(States.Watching);
            await Task.WhenAll(_packages.Values.Select(a => a.WaitReady()));
        }

        public void Dispose()
        {
            lock (_scanUpdateLockObj)
            {
                foreach (var package in _packages.Values)
                {
                    package.Dispose();
                }
            }
        }


        private void Scan()
        {
            try
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                    _watcher.Changed -= WatcherOnChanged;
                    _watcher.Created -= WatcherOnChanged;
                    _watcher.Deleted -= WatcherOnDeleted;
                    _watcher.Renamed -= WatcherOnRenamed;
                    _watcher.Error -= WatcherOnError;
                }

                _watcher = new FileSystemWatcher(_repPath);
                _watcher.Path = _repPath;
                _watcher.IncludeSubdirectories = false;

                _watcher.Changed += WatcherOnChanged;
                _watcher.Created += WatcherOnChanged;
                _watcher.Deleted += WatcherOnDeleted;
                _watcher.Renamed += WatcherOnRenamed;
                _watcher.Error += WatcherOnError;

                lock (_scanUpdateLockObj)
                {
                    _watcher.EnableRaisingEvents = true;

                    var fileList = Directory.GetFiles(_repPath);
                    foreach (var file in fileList)
                    {
                        if (_stateControl.Current == States.Closing)
                            break;

                        if (!PackageWatcher.IsFileSupported(file))
                            continue;

                        if (!_packages.TryGetValue(file, out var item))
                            UpsertPackage(file);
                        else
                            item.CheckForChanges();
                    }
                }

                _stateControl.PushEvent(Events.DoneScanning);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to scan {_repPath}", ex);
                _stateControl.PushEvent(Events.ScanFailed);
            }
        }

        private void WatcherOnError(object sender, ErrorEventArgs e)
        {
            _stateControl.PushEvent(Events.WatcherFail);
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            var oldFileSupported = PackageWatcher.IsFileSupported(e.OldFullPath);
            var newFileSupported = PackageWatcher.IsFileSupported(e.FullPath);

            if (!oldFileSupported && !newFileSupported)
                return;

            lock (_scanUpdateLockObj)
            {
                if (oldFileSupported)
                    RemovePackage(e.OldFullPath);

                if (newFileSupported)
                    UpsertPackage(e.FullPath);
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (!PackageWatcher.IsFileSupported(e.FullPath))
                return;

            lock (_scanUpdateLockObj)
            {
                UpsertPackage(e.FullPath);
            }
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (!PackageWatcher.IsFileSupported(e.FullPath))
                return;

            lock (_scanUpdateLockObj)
            {
                RemovePackage(e.FullPath);
            }
        }

        private void UpsertPackage(string path)
        {
            if (_packages.TryGetValue(path, out var package))
            {
                package.CheckForChanges();
            }
            else
            {
                package = new PackageWatcher(path, _location, _logger, _isolation);
                _packages.Add(path, package);
                package.Updated += PackageWatcherOnUpdated;
                Added?.Invoke(package.PackageRef);
                package.Start();
            }
        }

        private void RemovePackage(string path)
        {
            if (_packages.TryGetValue(path, out var package))
            {
                _packages.Remove(path);
                package.Updated -= PackageWatcherOnUpdated;
                Removed?.Invoke(package.PackageRef);
                package.Dispose();
            }
        }

        private void PackageWatcherOnUpdated(AlgoPackageRef packageRef)
        {
            Updated?.Invoke(packageRef);
        }
    }
}
