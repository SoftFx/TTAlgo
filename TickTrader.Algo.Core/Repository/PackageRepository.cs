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
        private bool _isWatcherFailed;
        private Task _scanTask;
        private Dictionary<string, PackageWatcher> _packages = new Dictionary<string, PackageWatcher>();
        private string _repPath;
        private RepositoryLocation _location;
        private IAlgoCoreLogger _logger;


        public event Action<AlgoRepositoryEventArgs> Added = delegate { };
        public event Action<AlgoRepositoryEventArgs> Removed = delegate { };
        public event Action<AlgoRepositoryEventArgs> Replaced = delegate { };


        public PackageRepository(string repPath, RepositoryLocation location, IAlgoCoreLogger logger = null)
        {
            _repPath = repPath;
            _location = location;
            _logger = logger;

            _stateControl.AddTransition(States.Created, Events.Start, States.Scanning);
            _stateControl.AddTransition(States.Scanning, Events.DoneScanning, States.Watching);
            _stateControl.AddTransition(States.Scanning, Events.WatcherFail, () => _isWatcherFailed = true);
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
                    _watcher.Error += WatcherOnError;
                }

                _isWatcherFailed = false;

                _watcher = new FileSystemWatcher(_repPath);
                _watcher.Path = _repPath;
                _watcher.IncludeSubdirectories = false;

                _watcher.Changed += WatcherOnChanged;
                _watcher.Created += WatcherOnChanged;
                _watcher.Deleted += WatcherOnDeleted;
                _watcher.Renamed += WatcherOnRenamed;

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
                            AddItem(file);
                        else
                            item.CheckForChanges();
                    }
                }

                _stateControl.PushEvent(Events.DoneScanning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _stateControl.PushEvent(Events.ScanFailed);
            }
        }

        private void AddItem(string file)
        {
            var item = new PackageWatcher(file, _location, _logger);
            item.Added += (a, m) => Added(new AlgoRepositoryEventArgs(this, m, a.FileName, a.FilePath));
            item.Removed += (a, m) => Removed(new AlgoRepositoryEventArgs(this, m, a.FileName, a.FilePath));
            item.Replaced += (a, m) => Replaced(new AlgoRepositoryEventArgs(this, m, a.FileName, a.FilePath));
            _packages.Add(file, item);
            item.Start();
        }

        private void WatcherOnError(object sender, ErrorEventArgs e)
        {
            _stateControl.PushEvent(Events.WatcherFail);
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            lock (_scanUpdateLockObj)
            {
                if (!PackageWatcher.IsFileSupported(e.FullPath))
                    return;

                PackageWatcher assembly;
                if (_packages.TryGetValue(e.FullPath, out assembly))
                    assembly.CheckForChanges();
                else
                {
                    assembly = new PackageWatcher(e.FullPath, _logger);
                    _packages.Add(e.FullPath, assembly);
                }
            }

            //lock (scanUpdateLockObj)
            //{
            //    FileWatcher assembly;
            //    if (assemblies.TryGetValue(e.OldFullPath, out assembly))
            //    {
            //        assemblies.Remove(e.OldFullPath);
            //        assemblies.Add(e.FullPath, assembly);
            //        assembly.Rename(e.FullPath);
            //    }
            //    else if (assemblies.TryGetValue(e.FullPath, out assembly))
            //    {
            //        // I dunno
            //    }
            //    else
            //    {
            //        assembly = new FileWatcher(e.FullPath, OnError);
            //        assemblies.Add(e.FullPath, assembly);
            //    }
            //}
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            lock (_scanUpdateLockObj)
            {
                if (!PackageWatcher.IsFileSupported(e.FullPath))
                    return;

                if (_packages.TryGetValue(e.FullPath, out var assembly))
                    assembly.CheckForChanges();
                else
                    AddItem(e.FullPath);
            }
        }
    }

    public class AlgoRepositoryEventArgs
    {
        public AlgoPackageRef PackageRef { get; }

        public AlgoPluginRef PluginRef { get; }


        public AlgoRepositoryEventArgs(AlgoPackageRef packageRef, AlgoPluginRef pluginRef)
        {
            PackageRef = packageRef;
            PluginRef = pluginRef;
        }
    }
}
