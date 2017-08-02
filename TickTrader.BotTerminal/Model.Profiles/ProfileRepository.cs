using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class ProfileRepositoryEventArgs
    {
        public string ProfileName { get; }

        public string FilePath { get; }


        public ProfileRepositoryEventArgs(string profileName, string filePath)
        {
            ProfileName = profileName;
            FilePath = filePath;
        }
    }


    public class ProfileRepository : IDisposable
    {
        public enum States { Created, Scanning, Waiting, Watching, Closing, Closed }


        public enum Events { Start, DoneScanning, ScanFailed, NextAttempt, CloseRequested, DoneClosing }


        private Logger _logger = LogManager.GetCurrentClassLogger();
        private StateMachine<States> _stateControl = new StateMachine<States>();
        private object _scanUpdateLockObj = new object();
        private FileSystemWatcher _watcher;
        private bool _isWatcherFailed;
        private Task _scanTask;
        private string _repPath;
        private Dictionary<string, string> _profiles = new Dictionary<string, string>();


        public event Action<ProfileRepositoryEventArgs> Added = delegate { };
        public event Action<ProfileRepositoryEventArgs> Removed = delegate { };


        public ProfileRepository(string repPath)
        {
            this._repPath = repPath;

            _stateControl.AddTransition(States.Created, Events.Start, States.Scanning);
            _stateControl.AddTransition(States.Scanning, Events.DoneScanning, States.Watching);
            _stateControl.AddTransition(States.Scanning, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Scanning, Events.ScanFailed, States.Waiting);
            _stateControl.AddTransition(States.Waiting, Events.NextAttempt, States.Scanning);
            _stateControl.AddTransition(States.Waiting, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Watching, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Watching, () => _isWatcherFailed, States.Scanning);
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

        public void Dispose()
        {
            StopWatching();
        }


        private void AddProfile(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            _profiles.Add(path, name);
            Added(new ProfileRepositoryEventArgs(name, path));
        }

        private void RemoveProfile(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            _profiles.Remove(path);
            Removed(new ProfileRepositoryEventArgs(name, path));
        }

        private bool IsFileSupported(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".profile";
        }

        private void StartWatching()
        {
            if (_watcher != null)
            {
                _watcher.Changed += Watcher_Changed;
                _watcher.Created += Watcher_Changed;
                _watcher.Deleted += Watcher_Deleted;
                _watcher.Renamed += Watcher_Renamed;
            }
        }

        private void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.Changed -= Watcher_Changed;
                _watcher.Created -= Watcher_Changed;
                _watcher.Deleted -= Watcher_Deleted;
                _watcher.Renamed -= Watcher_Renamed;
            }
        }

        private void Scan()
        {
            try
            {
                if (_watcher != null)
                {
                    StopWatching();
                    _watcher.Dispose();
                    _watcher.Error += Watcher_Error;
                }

                _isWatcherFailed = false;

                _watcher = new FileSystemWatcher(_repPath);
                _watcher.Path = _repPath;
                _watcher.IncludeSubdirectories = false;

                StartWatching();

                lock (_scanUpdateLockObj)
                {
                    _watcher.EnableRaisingEvents = true;

                    string[] fileList = Directory.GetFiles(_repPath);
                    foreach (string file in fileList)
                    {
                        if (_stateControl.Current == States.Closing)
                            break;

                        if (!IsFileSupported(file))
                            continue;

                        if (!_profiles.ContainsKey(file))
                            AddProfile(file);
                    }
                }

                _stateControl.PushEvent(Events.DoneScanning);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to scan profile repository: {ex.Message}");
                _stateControl.PushEvent(Events.ScanFailed);
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            _stateControl.ModifyConditions(() => _isWatcherFailed = true);
        }

        void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            lock (_scanUpdateLockObj)
            {
                if (_profiles.ContainsKey(e.OldFullPath))
                    RemoveProfile(e.OldFullPath);

                if (!IsFileSupported(e.FullPath))
                    return;

                if (!_profiles.ContainsKey(e.FullPath))
                    AddProfile(e.FullPath);
            }
        }

        void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            lock (_scanUpdateLockObj)
            {
                if (_profiles.ContainsKey(e.FullPath))
                    RemoveProfile(e.FullPath);
            }
        }

        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (_scanUpdateLockObj)
            {
                if (!IsFileSupported(e.FullPath))
                    return;

                if (!_profiles.ContainsKey(e.FullPath))
                    AddProfile(e.FullPath);
            }
        }
    }
}
