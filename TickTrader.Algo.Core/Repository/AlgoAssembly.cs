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
    internal class AlgoAssembly : IDisposable
    {
        public enum States { Created, Loading, WatingForRetry, Ready, Closing, Closed }
        public enum Events { Start, Changed, DoneLoad, DoneLoadRetry, NextRetry, CloseRequested, DoneClosing }

        //public enum ScanStatuses { NotScanned, Scanned, BadReferences, UnknownFile }

        private Isolated<ChildDomainProxy> currentSubdomain;
        private StateMachine<States> stateControl = new StateMachine<States>();
        private Dictionary<string, AlgoPluginRef> items = new Dictionary<string, AlgoPluginRef>();
        private bool isChanged;
        private Task scanTask;
        private Action<Exception> onError;

        public AlgoAssembly(string filePath, Action<Exception> onError)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this.onError = onError;

            stateControl.AddTransition(States.Created, Events.Start, States.Loading);
            stateControl.AddTransition(States.Loading, Events.DoneLoad, States.Ready);
            stateControl.AddTransition(States.Loading, Events.DoneLoadRetry, States.WatingForRetry);
            stateControl.AddTransition(States.Loading, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.WatingForRetry, Events.NextRetry, States.Loading);
            stateControl.AddTransition(States.WatingForRetry, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Ready, ()=> isChanged, States.Loading);
            stateControl.AddTransition(States.Ready, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Closing, Events.DoneClosing, States.Closed);

            stateControl.OnEnter(States.Loading, () => scanTask = Task.Factory.StartNew(() => Load(FilePath)));
            stateControl.AddScheduledEvent(States.WatingForRetry, Events.NextRetry, 100);            
        }

        public string FilePath { get; private set; }
        //public ScanStatuses ScanStatus { get; private set; }
        public FileInfo FileInfo { get; private set; }
        public string FileName { get; private set; }

        public event Action<AlgoAssembly, AlgoPluginRef> Added;
        public event Action<AlgoAssembly, AlgoPluginRef> Removed;
        public event Action<AlgoAssembly, AlgoPluginRef> Replaced;

        public void Start()
        {
            stateControl.PushEvent(Events.Start);
        }

        public void Dispose()
        {
            if (currentSubdomain != null)
                currentSubdomain.Dispose();
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
            Isolated<ChildDomainProxy> subDomain = null;

            try
            {
                isChanged = false;

                FileInfo info = new FileInfo(filePath);

                //if (this.ScanStatus != ScanStatuses.NotScanned
                //    || this.FileInfo == null
                //    || this.FileInfo.Length != info.Length
                //    || this.FileInfo.LastWriteTime != info.LastWriteTime)
                //{
                    subDomain = new Isolated<ChildDomainProxy>();
                    var sandbox = subDomain.Value.CreateSanbox(filePath);

                    Merge(sandbox.AlgoMetadata, sandbox);

                    this.FileInfo = info;

                    if (currentSubdomain != null)
                        currentSubdomain.Dispose();

                    currentSubdomain = subDomain;
                    //ScanStatus = ScanStatuses.Scanned;
                //}
                stateControl.PushEvent(Events.DoneLoad);
            }
            //catch (BadImageFormatException)
            //{
            //    if (subDomain != null)
            //        subDomain.Dispose();
            //    //ScanStatus = ScanStatuses.UnknownFile;
            //    stateControl.PushEvent(Events.DoneLoad);
            //}
            //catch (FileLoadException)
            //{
            //    if (subDomain != null)
            //        subDomain.Dispose();
            //    //ScanStatus = ScanStatuses.NotScanned;
            //    stateControl.PushEvent(Events.DoneLoad);
            //}
            //catch (FileNotFoundException fnfex)
            //{
            //    if (subDomain != null)
            //        subDomain.Dispose();
            //    //ScanStatus = ScanStatuses.NotScanned;
            //    OnError(fnfex);
            //    stateControl.PushEvent(Events.DoneLoad);
            //}
            catch (Exception ex)
            {
                if (subDomain != null)
                    subDomain.Dispose();
                //ScanStatus = ScanStatuses.NotScanned;
                stateControl.PushEvent(Events.DoneLoad);
                OnError(ex);
            }
        }

        private void Merge(IEnumerable<AlgoPluginDescriptor> newMetadata, AlgoSandbox newSandbox)
        {
            // upsert

            foreach (AlgoPluginDescriptor newInfo in newMetadata)
            {
                var newItem = new IsolatedPluginRef(newInfo, newSandbox);

                if (!items.ContainsKey(newInfo.Id))
                {
                    items.Add(newInfo.Id, newItem);
                    Added(this, newItem);
                }
                else
                {
                    items[newInfo.Id] = newItem;
                    Replaced(this, newItem);
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

        private void OnError(Exception ex)
        {
            onError?.Invoke(ex);
        }

        internal class ChildDomainProxy : CrossDomainObject
        {
            public AlgoSandbox CreateSanbox(string dllPath)
            {
                return new AlgoSandbox(dllPath);
            }
        }
    }
}
