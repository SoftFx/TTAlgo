using StateMachinarium;
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
    internal class AlgoAssembly : IDisposable
    {
        public enum States { Created, Loading, WatingForRetry, Ready, Closing, Closed }
        public enum Events { Start, Changed, Loaded, LoadFailed, NextRetry, CloseRequested, DoneClosing }

        public enum ScanStatuses { NotScanned, Scanned, UnknownFile }

        private Isolated<AlgoSandbox> isolatedSandbox;
        private StateMachine<States> stateControl = new StateMachine<States>();
        private Dictionary<string, AlgoRepositoryItem> items = new Dictionary<string, AlgoRepositoryItem>();
        private bool isChanged;
        private Task scanTask;

        public AlgoAssembly(string filePath)
        {
            this.FilePath = filePath;

            stateControl.AddTransition(States.Created, Events.Start, States.Loading);
            stateControl.AddTransition(States.Loading, Events.Loaded, States.Ready);
            stateControl.AddTransition(States.Loading, Events.LoadFailed, States.WatingForRetry);
            stateControl.AddTransition(States.Loading, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.WatingForRetry, Events.NextRetry, States.Loading);
            stateControl.AddTransition(States.WatingForRetry, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Ready, ()=> isChanged, States.Loading);
            stateControl.AddTransition(States.Ready, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Closing, Events.DoneClosing, States.Closed);

            stateControl.OnEnter(States.Loading, () => scanTask = Task.Factory.StartNew(() => Load(FilePath)));
            stateControl.AddScheduledEvent(States.WatingForRetry, Events.NextRetry, 100);

            stateControl.PushEvent(Events.Start);
        }

        public string FilePath { get; private set; }
        public ScanStatuses ScanStatus { get; private set; }
        public FileInfo FileInfo { get; private set; }

        public event Action<AlgoRepositoryItem> Added;
        public event Action<AlgoRepositoryItem> Removed;
        public event Action<AlgoRepositoryItem> Replaced;

        public void Dispose()
        {
            if (isolatedSandbox != null)
                isolatedSandbox.Dispose();
        }

        internal void CheckForChanges()
        {
            stateControl.ModifyConditions(() => isChanged = true);
        }

        internal void Rename(string newPath)
        {
            stateControl.SyncContext.Synchronized(() => FilePath = newPath);
        }

        private void Load(string filePath)
        {
            Isolated<AlgoSandbox> newSandbox = null;

            try
            {
                isChanged = false;

                FileInfo info = new FileInfo(filePath);

                if (this.ScanStatus != ScanStatuses.NotScanned
                    || this.FileInfo == null
                    || this.FileInfo.Length != info.Length
                    || this.FileInfo.LastWriteTime != info.LastWriteTime)
                {
                    newSandbox = new Isolated<AlgoSandbox>();
                    var metadata = newSandbox.Value.LoadAndInspect(filePath);

                    Merge(metadata, newSandbox.Value);

                    this.FileInfo = info;
                    if (isolatedSandbox != null)
                    {
                        // TO DO : fire event
                        isolatedSandbox.Dispose();
                    }

                    isolatedSandbox = newSandbox;
                    ScanStatus = ScanStatuses.Scanned;
                }
                stateControl.PushEvent(Events.Loaded);
            }
            catch (BadImageFormatException)
            {
                ScanStatus = ScanStatuses.UnknownFile;
                stateControl.PushEvent(Events.Loaded);
            }
            catch (FileLoadException)
            {
                ScanStatus = ScanStatuses.NotScanned;
                stateControl.PushEvent(Events.LoadFailed);
            }
            catch (FileNotFoundException fnfex)
            {
                Debug.WriteLine("ERROR: File not found: " + fnfex.FileName);
                ScanStatus = ScanStatuses.NotScanned;
                stateControl.PushEvent(Events.LoadFailed);
            }
            catch (Exception ex)
            {
                ScanStatus = ScanStatuses.NotScanned;
                stateControl.PushEvent(Events.LoadFailed);
                Debug.WriteLine(ex);
            }

            if (newSandbox != null)
                Dispose();
        }

        private void Merge(IEnumerable<AlgoInfo> newMetadata, AlgoSandbox newSandbox)
        {
            // upsert

            foreach (AlgoInfo newInfo in newMetadata)
            {
                var newItem = new AlgoRepositoryItem(newSandbox, newInfo);

                if (!items.ContainsKey(newInfo.Id))
                {
                    items.Add(newInfo.Id, newItem);
                    Added(newItem);
                }
                else
                {
                    items[newInfo.Id] = newItem;
                    Replaced(newItem);
                }
            }

            // delete

            var newMetadataLookup = newMetadata.ToDictionary(a => a.Id);
            foreach (AlgoRepositoryItem item in items.Values)
            {
                if (!newMetadataLookup.ContainsKey(item.Descriptor.Id))
                {
                    items.Remove(item.Descriptor.Id);
                    Removed(item);
                }
            }
        }
    }
}
