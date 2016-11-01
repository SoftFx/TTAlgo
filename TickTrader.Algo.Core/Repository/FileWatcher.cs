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
        public enum Events { Start, Changed, DoneLoad, DoneLoadRetry, NextRetry, CloseRequested, DoneClosing }

        const long ERROR_SHARING_VIOLATION = 0x20;
        const long ERROR_LOCK_VIOLATION = 0x21;

        private StateMachine<States> stateControl = new StateMachine<States>();
        private Dictionary<string, AlgoPluginRef> items = new Dictionary<string, AlgoPluginRef>();
        private FileInfo currentFileInfo;
        private bool isRescanRequested;
        private Task scanTask;
        private Action<Exception> onError;
        private AlgoRepositoryItem item;

        public FileWatcher(string filePath, Action<Exception> onError)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this.onError = onError;

            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".ttalgo")
                item = new AlgoPackageItem();
            else if (ext == ".dll")
                item = new NetAssemblyItem();
            else
                throw new ArgumentException("Unrecognized file type: " + ext);

            stateControl.AddTransition(States.Created, Events.Start, States.Loading);
            stateControl.AddTransition(States.Loading, Events.DoneLoad, States.Ready);
            stateControl.AddTransition(States.Loading, Events.DoneLoadRetry, States.WatingForRetry);
            stateControl.AddTransition(States.Loading, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.WatingForRetry, Events.NextRetry, States.Loading);
            stateControl.AddTransition(States.WatingForRetry, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Ready, () => isRescanRequested, States.Loading);
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
            DisposeSafe();
        }

        private void DisposeSafe()
        {
            try
            {
                item.Dispose();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        internal void CheckForChanges()
        {
            stateControl.ModifyConditions(() => isRescanRequested = true);
        }

        internal void Rename(string newPath)
        {
            stateControl.SyncContext.Synchronized(() => FilePath = newPath);
        }

        private void Load(string filePath)
        {
            try
            {
                FileInfo info;

                using (var stream = File.OpenRead(filePath))
                {
                    info = new FileInfo(filePath);

                    bool skipFileScan = currentFileInfo != null
                        && currentFileInfo.Length == info.Length
                        && currentFileInfo.CreationTime == info.CreationTimeUtc
                        && currentFileInfo.LastWriteTime == info.LastWriteTimeUtc;

                    if (!skipFileScan)
                    {
                        DisposeSafe();
                        item.Load(stream, filePath);
                    }
                }

                currentFileInfo = info;
                Merge(item.Metadata);
                stateControl.PushEvent(Events.DoneLoad);
            }
            catch (IOException ioEx)
            {
                long win32ErrorCode = ioEx.HResult & 0xFFFF;
                if (win32ErrorCode == ERROR_SHARING_VIOLATION || win32ErrorCode == ERROR_LOCK_VIOLATION)
                {
                    // file in use.
                    stateControl.PushEvent(Events.DoneLoadRetry);
                }
                else
                {
                    stateControl.PushEvent(Events.DoneLoad);
                }
            }
            catch (Exception ex)
            {
                stateControl.PushEvent(Events.DoneLoad);
                OnError(ex);
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

        private void OnError(Exception ex)
        {
            onError?.Invoke(ex);
        }
    }

    internal abstract class AlgoRepositoryItem : CrossDomainObject
    {
        public IEnumerable<AlgoPluginRef> Metadata { get; protected set; }
        public abstract void Load(FileStream stream, string filePath);

        internal class ChildDomainProxy : CrossDomainObject
        {
            public AlgoSandbox CreateDotNetSanbox(IDotNetPluginPackage netPackage)
            {
                return new AlgoSandbox(netPackage);
            }
        }
    }

    internal class NetAssemblyItem : AlgoRepositoryItem, IDotNetPluginPackage
    {
        private Isolated<ChildDomainProxy> subDomain;
        private string dllFolderPath;

        public string MainAssemblyName { get; private set; }

        public override void Load(FileStream stream, string filePath)
        {
            dllFolderPath = Path.GetDirectoryName(filePath);
            MainAssemblyName = Path.GetFileName(filePath);
            subDomain = new Isolated<ChildDomainProxy>();
            var sandbox = subDomain.Value.CreateDotNetSanbox(this);
            Metadata = sandbox.AlgoMetadata.Select(d => new IsolatedPluginRef(d, sandbox));
        }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            string fullPath = Path.Combine(dllFolderPath, packageLocalPath);

            try
            {
                return File.ReadAllBytes(fullPath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (subDomain != null)
                subDomain.Dispose();

            base.Dispose(disposing);
        }
    }

    internal class AlgoPackageItem : AlgoRepositoryItem, IDotNetPluginPackage
    {
        private Isolated<ChildDomainProxy> subDomain;
        private Package algoPackage;

        public override void Load(FileStream stream, string filePath)
        {
            algoPackage = Package.Load(stream);
            MainAssemblyName = algoPackage.Metadata.MainBinaryFile;
            subDomain = new Isolated<ChildDomainProxy>();
            var sandbox = subDomain.Value.CreateDotNetSanbox(this);
            Metadata = sandbox.AlgoMetadata.Select(d => new IsolatedPluginRef(d, sandbox));
        }

        public string MainAssemblyName { get; private set; }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            return algoPackage.GetFile(packageLocalPath);
        }

        protected override void Dispose(bool disposing)
        {
            if (subDomain != null)
                subDomain.Dispose();

            base.Dispose(disposing);
        }
    }
}
