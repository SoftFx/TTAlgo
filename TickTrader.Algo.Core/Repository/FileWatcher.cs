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

        private StateMachine<States> stateControl = new StateMachine<States>();
        private Dictionary<string, AlgoPluginRef> items = new Dictionary<string, AlgoPluginRef>();
        private FileInfo currentFileInfo;
        private bool isRescanRequested;
        private Task scanTask;
        private IAlgoCoreLogger logger;
        private AlgoRepositoryItem item;

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
            DisposeSafe(item);       
        }

        private void DisposeSafe(AlgoRepositoryItem itemToDispose)
        {
            try
            {
                if (item != null)
                    itemToDispose.Dispose();
            }
            catch (Exception ex)
            {
                logger?.Debug("Failed to unload child domain: " + ex.Message);
            }
        }

        private static AlgoRepositoryItem ItemFactory(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".ttalgo")
                return new AlgoPackageItem();
            else if (ext == ".dll")
                return new NetAssemblyItem();
            else
                throw new ArgumentException("Unrecognized file type: " + ext);
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
            AlgoRepositoryItem newItem = null;

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
                        newItem = ItemFactory(filePath);
                        newItem.Logger = logger;
                        newItem.Load(stream, filePath);
                        currentFileInfo = info;
                        logger.Info("Loaded package " + FileName);
                        Merge(newItem.Metadata); // this will fire events
                        DisposeSafe(item); // dispose old item after event firing, so all running plugins can be gracefuly stopped
                        item = newItem;
                    }
                }

                stateControl.PushEvent(Events.DoneLoad);
            }
            catch (IOException ioEx)
            {
                if (newItem != null)
                    DisposeSafe(newItem);

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
                if (newItem != null)
                    DisposeSafe(newItem);

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

    internal abstract class AlgoRepositoryItem : CrossDomainObject
    {
        public IAlgoCoreLogger Logger { get; set; }
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
            try
            {
                subDomain = new Isolated<ChildDomainProxy>();
                var sandbox = subDomain.Value.CreateDotNetSanbox(this);
                Metadata = sandbox.AlgoMetadata.Select(d => new IsolatedPluginRef(d, sandbox));
            }
            catch (Exception)
            {
                Dispose(true);
                throw;
            }
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
            try
            {
                if (subDomain != null)
                    subDomain.Dispose();
            }
            catch (Exception ex)
            {
                Logger?.Debug("Failed to unload child domain: " + ex.Message);
            }

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
            try
            {
                subDomain = new Isolated<ChildDomainProxy>();
                var sandbox = subDomain.Value.CreateDotNetSanbox(this);
                Metadata = sandbox.AlgoMetadata.Select(d => new IsolatedPluginRef(d, sandbox));
            }
            catch (Exception)
            {
                Dispose(true);
                throw;
            }
        }

        public string MainAssemblyName { get; private set; }

        public byte[] GetFileBytes(string packageLocalPath)
        {
            return algoPackage.GetFile(packageLocalPath);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (subDomain != null)
                    subDomain.Dispose();
            }
            catch (Exception ex)
            {
                Logger?.Debug("Failed to unload child domain: " + ex.Message);
            }

            base.Dispose(disposing);
        }
    }
}
