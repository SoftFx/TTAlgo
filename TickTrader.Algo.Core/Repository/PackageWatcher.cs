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


        private StateMachine<States> _stateControl = new StateMachine<States>();
        private Dictionary<string, AlgoPluginRef> _plugins = new Dictionary<string, AlgoPluginRef>();
        private FileInfo _currentFileInfo;
        private bool _isRescanRequested;
        private Task _scanTask;
        private IAlgoCoreLogger _logger;


        public string FilePath { get; }

        public string FileName { get; }

        public RepositoryLocation Location { get; }

        public AlgoPackageRef Package { get; private set; }


        public event Action<PackageWatcher, AlgoPluginRef> Added;
        public event Action<PackageWatcher, AlgoPluginRef> Removed;
        public event Action<PackageWatcher, AlgoPluginRef> Replaced;


        public PackageWatcher(string filePath, RepositoryLocation location, IAlgoCoreLogger logger)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Location = location;
            _logger = logger;

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
            foreach (var plugin in _plugins.Values)
            {
                Removed?.Invoke(this, plugin);
            }
            Package?.SetObsolete();
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
                        var oldPackage = Package;
                        var container = PluginContainer.Load(filePath, _logger);
                        newPackage = new AlgoPackageRef(Location, FileName, info.LastWriteTimeUtc, container);
                        Package = newPackage;
                        _currentFileInfo = info;
                        _logger.Info("Loaded package " + FileName);
                        Merge(newPackage.GetPluginRefs()); // this will fire events
                        oldPackage?.SetObsolete(); // mark old package obsolete, so it is disposed after all running plugins are gracefuly stopped
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

        private void Merge(IEnumerable<AlgoPluginRef> newMetadata)
        {
            // upsert

            foreach (var pluginRef in newMetadata)
            {
                if (!_plugins.ContainsKey(pluginRef.Id))
                {
                    _plugins.Add(pluginRef.Id, pluginRef);
                    Added(this, pluginRef);
                }
                else
                {
                    _plugins[pluginRef.Id] = pluginRef;
                    Replaced(this, pluginRef);
                }
            }

            // delete

            var newMetadataLookup = newMetadata.ToDictionary(a => a.Id);
            foreach (var item in _plugins.Values)
            {
                if (!newMetadataLookup.ContainsKey(item.Metadata.Id))
                {
                    _plugins.Remove(item.Metadata.Id);
                    Removed(this, item);
                }
            }
        }
    }
}
