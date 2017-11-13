using Machinarium.State;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    internal class FileWatcher : IDisposable
    {
        public enum States { Created, Loading, WatingForRetry, Ready, Closing, Closed }
        public enum Events { Start, Changed, DoneLoad, DoneLoadRetry, NextRetry, CloseRequested, DoneClosing, Rescan }

        private StateMachine<States> stateControl = new StateMachine<States>();
        private Dictionary<string, AlgoPluginRef> items = new Dictionary<string, AlgoPluginRef>();
        private FileInfo currentFileInfo;
        private bool isRescanRequested;
        private Task scanTask;
        private IAlgoCoreLogger logger;
        private PluginContainer item;

        public FileWatcher(string filePath, IAlgoCoreLogger logger)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this.logger = logger;

            stateControl.AddTransition(States.Created, Events.Start, States.Loading);
            stateControl.AddTransition(States.Loading, Events.DoneLoad, States.Ready);
            stateControl.AddTransition(States.Loading, Events.DoneLoadRetry, States.WatingForRetry);
            stateControl.AddTransition(States.Loading, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.WatingForRetry, Events.NextRetry, States.Loading);
            stateControl.AddTransition(States.WatingForRetry, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Ready, Events.Rescan, States.Loading);
            stateControl.AddTransition(States.Ready, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Closing, Events.DoneClosing, States.Closed);

            stateControl.OnEnter(States.Loading, () =>
            {
                isRescanRequested = false;
                scanTask = Task.Factory.StartNew(() => Load(FilePath));
            });

            stateControl.AddScheduledEvent(States.WatingForRetry, Events.NextRetry, 100);            
        }

        public static bool IsFileSupported(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".ttalgo" || ext == ".dll";
        }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        public event Action<FileWatcher, AlgoPluginRef> Added;
        public event Action<FileWatcher, AlgoPluginRef> Removed;
        public event Action<FileWatcher, AlgoPluginRef> Replaced;

        public void Start()
        {
            stateControl.PushEvent(Events.Start);
        }

        public void Dispose()
        {
            item?.Dispose();
        }

        internal void CheckForChanges()
        {
            stateControl.PushEvent(Events.Rescan);
        }

        internal void Rename(string newPath)
        {
            stateControl.SyncContext.Synchronized(() => FilePath = newPath);
        }

        private void Load(string filePath)
        {
            PluginContainer newItem = null;

            try
            {
                FileInfo info;

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    info = new FileInfo(filePath);

                    bool skipFileScan = currentFileInfo != null
                        && currentFileInfo.Length == info.Length
                        && currentFileInfo.CreationTime == info.CreationTimeUtc
                        && currentFileInfo.LastWriteTime == info.LastWriteTimeUtc;

                    if (!skipFileScan)
                    {
                        newItem = PluginContainer.Load(filePath, logger);
                        currentFileInfo = info;
                        logger.Info("Loaded package " + FileName);
                        Merge(newItem.Plugins); // this will fire events
                        item?.Dispose(); // dispose old item after event firing, so all running plugins can be gracefuly stopped
                        item = newItem;
                    }
                }

                stateControl.PushEvent(Events.DoneLoad);
            }
            catch (IOException ioEx)
            {
                newItem?.Dispose();

                if (ioEx.IsLockExcpetion())
                {
                    logger?.Debug("File is locked: " + FileName);
                    stateControl.PushEvent(Events.DoneLoadRetry); // File is in use. We should retry loading.
                }
                else
                {
                    logger?.Info("Cannot open file: " + FileName + " " + ioEx.Message);
                    stateControl.PushEvent(Events.DoneLoad); // other errors
                }
            }
            catch (Exception ex)
            {
                newItem?.Dispose();
                logger?.Info("Cannot open file: " + FileName + " " + ex.Message);
                stateControl.PushEvent(Events.DoneLoad);
            }
        }

        private void Merge(IEnumerable<AlgoPluginRef> newMetadata)
        {
            // upsert

            foreach (var pluginRef in newMetadata)
            {
                if (!items.ContainsKey(pluginRef.Id))
                {
                    items.Add(pluginRef.Id, pluginRef);
                    Added(this, pluginRef);
                }
                else
                {
                    items[pluginRef.Id] = pluginRef;
                    Replaced(this, pluginRef);
                }
            }

            // delete

            var newMetadataLookup = newMetadata.ToDictionary(a => a.Id);
            foreach (AlgoPluginRef item in items.Values)
            {
                if (!newMetadataLookup.ContainsKey(item.Descriptor.Id))
                {
                    items.Remove(item.Descriptor.Id);
                    Removed(this, item);
                }
            }
        }
    }
}
