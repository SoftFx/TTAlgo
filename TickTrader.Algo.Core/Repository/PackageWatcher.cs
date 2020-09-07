using Machinarium.State;
using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    public class PackageWatcher : IDisposable
    {
        public enum States { Created, Loading, WatingForRetry, Ready, Closing, Closed }

        public enum Events { Start, Changed, DoneLoad, DoneLoadRetry, NextRetry, CloseRequested, DoneClosing, Rescan }


        private StateMachine<States> _stateControl;
        private FileInfo _currentFileInfo;
        private bool _isRescanRequested;
        private Task _scanTask;
        private IAlgoCoreLogger _logger;
        private bool _isolation;


        public string FilePath { get; }

        public string FileName { get; }

        public RepositoryLocation Location { get; }

        public AlgoPackageRef PackageRef { get; private set; }


        public event Action<AlgoPackageRef> Updated;


        public PackageWatcher(string filePath, RepositoryLocation location, IAlgoCoreLogger logger, bool isolation)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Location = location;
            _logger = logger;
            _isolation = isolation;

            PackageRef = new AlgoPackageRef(FileName, Location, PackageIdentity.CreateInvalid(FileName, FilePath), null);

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

        public static string GetPackageExtensions => "Packages|*.ttalgo";

        public static string GetPackageAndAllExtensions => "Packages|*.ttalgo|All Files|*.*";

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
            if (!_isolation)
            {
                _logger.Info($"Isolation is disabled. Skipping check for changes in package {FileName} at {Location}");
                return;
            }

            _isRescanRequested = true;
            _stateControl.PushEvent(Events.Rescan);
        }


        private void Load(string filePath)
        {
            var retry = false;

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
                        PackageRef?.SetObsolete(); // mark old package obsolete, so it is disposed after all running plugins are gracefully stopped
                        var container = LoadContainer(filePath, out retry);
                        var identity = CreateIdentity(info, out retry);
                        PackageRef = new IsolatedAlgoPackageRef(FileName, Location, identity, container);
                        _currentFileInfo = info;
                        _logger.Info("Loaded package " + FileName);
                    }
                }
            }
            catch (IOException ioEx)
            {
                if (ioEx.IsLockExcpetion())
                {
                    _logger?.Debug($"Package {FileName} at {Location} is locked");
                    retry = true; // File is in use. We should retry loading.
                }
                else
                {
                    _logger?.Info($"Cannot open package {FileName} at {Location}: {ioEx.Message}"); // other errors
                }
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to update package {FileName} at {Location}", ex);
                PackageRef = new AlgoPackageRef(FileName, Location, PackageIdentity.CreateInvalid(FileName, FilePath), null);
            }

            if (!retry && !_isRescanRequested)
                OnPackageUpdated();
            _stateControl.PushEvent(retry || _isRescanRequested ? Events.DoneLoadRetry : Events.DoneLoad);
        }

        private PluginContainer LoadContainer(string filePath, out bool retry)
        {
            retry = false;
            try
            {
                return PluginContainer.Load(filePath, _isolation, _logger);
            }
            catch (IOException ioEx)
            {
                if (ioEx.IsLockExcpetion())
                {
                    _logger?.Debug("File is locked: " + FileName);
                    retry = true; // File is in use. We should retry loading.
                }
                else
                {
                    _logger?.Info("Cannot open file: " + FileName + " " + ioEx.Message); // other errors
                }
            }
            catch (Exception ex)
            {
                _logger?.Info("Cannot open file: " + FileName + " " + ex.Message);
            }
            return null;
        }

        private void OnPackageUpdated()
        {
            try
            {
                Updated?.Invoke(PackageRef);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Failed to send update events for package {FileName} at {Location}", ex);
            }
        }

        private PackageIdentity CreateIdentity(FileInfo info, out bool retry)
        {
            retry = false;
            try
            {
                return PackageIdentity.Create(info);
            }
            catch (IOException ioEx)
            {
                if (ioEx.IsLockExcpetion())
                {
                    _logger?.Debug("File is locked: " + FileName);
                    retry = true; // File is in use. We should retry loading.
                }
                else
                {
                    _logger?.Info("Cannot calculate file hash: " + FileName + " " + ioEx.Message); // other errors
                }
            }
            catch (Exception ex)
            {
                _logger?.Info("Cannot calculate file hash: " + FileName + " " + ex.Message);
            }
            return PackageIdentity.CreateInvalid(info);
        }
    }
}
