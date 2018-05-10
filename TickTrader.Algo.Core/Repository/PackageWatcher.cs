using Machinarium.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    internal class PackageWatcher : IDisposable
    {
        public enum States { Created, Loading, WatingForRetry, Ready, Closing, Closed }

        public enum Events { Start, Changed, DoneLoad, DoneLoadRetry, NextRetry, CloseRequested, DoneClosing, Rescan }


        private StateMachine<States> _stateControl;
        private FileInfo _currentFileInfo;
        private bool _isRescanRequested;
        private Task _scanTask;
        private IAlgoCoreLogger _logger;


        public string FilePath { get; }

        public string FileName { get; }

        public RepositoryLocation Location { get; }

        public AlgoPackageRef PackageRef { get; private set; }


        public event Action<AlgoPackageRef> Updated;


        public PackageWatcher(string filePath, RepositoryLocation location, IAlgoCoreLogger logger)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath).ToLowerInvariant();
            Location = location;
            _logger = logger;

            _stateControl = new StateMachine<States>();

            _stateControl.AddTransition(States.Created, Events.Start, States.Loading);
            _stateControl.AddTransition(States.Loading, Events.DoneLoad, States.Ready);
            _stateControl.AddTransition(States.Loading, Events.DoneLoadRetry, States.WatingForRetry);
            _stateControl.AddTransition(States.Loading, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.WatingForRetry, Events.NextRetry, States.Loading);
            _stateControl.AddTransition(States.WatingForRetry, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Ready, Events.Rescan, States.Loading);
            _stateControl.AddTransition(States.Ready, Events.CloseRequested, States.Closing);
            _stateControl.AddTransition(States.Closing, Events.DoneClosing, States.Closed);

            _stateControl.OnEnter(States.Loading, () =>
            {
                _isRescanRequested = false;
                _scanTask = Task.Factory.StartNew(() => Load(FilePath));
            });

            _stateControl.AddScheduledEvent(States.WatingForRetry, Events.NextRetry, 100);
        }


        public static bool IsFileSupported(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".ttalgo" || ext == ".dll";
        }


        public void Start()
        {
            _stateControl.PushEvent(Events.Start);
        }

        public void Dispose()
        {
            PackageRef?.SetObsolete();
        }

        public Task WaitReady()
        {
            return _stateControl.AsyncWait(States.Ready);
        }


        internal void CheckForChanges()
        {
            _stateControl.PushEvent(Events.Rescan);
        }


        private void Load(string filePath)
        {
            AlgoPackageRef newPackage = null;

            try
            {
                FileInfo info;

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    info = new FileInfo(filePath);

                    var skipFileScan = _currentFileInfo != null
                        && _currentFileInfo.Length == info.Length
                        && _currentFileInfo.CreationTimeUtc == info.CreationTimeUtc
                        && _currentFileInfo.LastWriteTimeUtc == info.LastWriteTimeUtc;

                    if (!skipFileScan)
                    {
                        var oldPackage = PackageRef;
                        var container = PluginContainer.Load(filePath, _logger);
                        newPackage = new IsolatedAlgoPackageRef(FileName, Location, info.LastWriteTimeUtc, container);
                        _currentFileInfo = info;
                        _logger.Info("Loaded package " + FileName);
                        PackageRef = newPackage;
                        if (oldPackage != null)
                        {
                            Updated?.Invoke(newPackage);
                            oldPackage.SetObsolete(); // mark old package obsolete, so it is disposed after all running plugins are gracefully stopped
                        }
                    }
                }

                _stateControl.PushEvent(Events.DoneLoad);
            }
            catch (IOException ioEx)
            {
                newPackage?.Dispose();

                if (ioEx.IsLockExcpetion())
                {
                    _logger?.Debug("File is locked: " + FileName);
                    _stateControl.PushEvent(Events.DoneLoadRetry); // File is in use. We should retry loading.
                }
                else
                {
                    _logger?.Info("Cannot open file: " + FileName + " " + ioEx.Message);
                    _stateControl.PushEvent(Events.DoneLoad); // other errors
                }
            }
            catch (Exception ex)
            {
                newPackage?.Dispose();
                _logger?.Info("Cannot open file: " + FileName + " " + ex.Message);
                _stateControl.PushEvent(Events.DoneLoad);
            }
        }
    }
}
