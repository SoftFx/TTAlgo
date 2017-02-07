using Machinarium.State;
using Machinarium.Qnil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class AlgoRepository : IDisposable
    {
        public enum States { Created, Scanning, Waiting, Watching, Closing, Closed }
        public enum Events { Start, DoneScanning, ScanFailed, NextAttempt, CloseRequested, DoneClosing }

        private StateMachine<States> stateControl = new StateMachine<States>();
        private object scanUpdateLockObj = new object();
        private object globalLockObj = new object();
        private FileSystemWatcher watcher;
        private IAlgoCoreLogger logger;
        private bool isWatcherFailed;
        private Task scanTask;
        private string repPath;
        private Dictionary<string, FileWatcher> assemblies = new Dictionary<string, FileWatcher>();

        public AlgoRepository(string repPath, IAlgoCoreLogger logger = null)
        {
            this.repPath = repPath;
            this.logger = logger;

            stateControl.AddTransition(States.Created, Events.Start, States.Scanning);
            stateControl.AddTransition(States.Scanning, Events.DoneScanning, States.Watching);
            stateControl.AddTransition(States.Scanning, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Scanning, Events.ScanFailed, States.Waiting);
            stateControl.AddTransition(States.Waiting, Events.NextAttempt, States.Scanning);
            stateControl.AddTransition(States.Waiting, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Watching, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Watching, () => isWatcherFailed, States.Scanning);
            stateControl.AddTransition(States.Closing, Events.DoneClosing, States.Closed);

            stateControl.AddScheduledEvent(States.Waiting, Events.NextAttempt, 1000);

            stateControl.OnEnter(States.Scanning, () => scanTask = Task.Factory.StartNew(Scan));
        }

        public event Action<AlgoRepositoryEventArgs> Added = delegate { };
        public event Action<AlgoRepositoryEventArgs> Removed = delegate { };
        public event Action<AlgoRepositoryEventArgs> Replaced = delegate { };

        public void Start()
        {
            stateControl.PushEvent(Events.Start);
        }

        public Task Stop()
        {
            return stateControl.PushEventAndWait(Events.CloseRequested, States.Closed);
        }

        private void Scan()
        {
            try
            {
                if (watcher != null)
                {
                    watcher.Dispose();
                    watcher.Changed -= watcher_Changed;
                    watcher.Created -= watcher_Changed;
                    watcher.Deleted -= watcher_Deleted;
                    watcher.Renamed -= watcher_Renamed;
                    watcher.Error += watcher_Error;
                }

                isWatcherFailed = false;

                watcher = new FileSystemWatcher(repPath);
                watcher.Path = repPath;
                watcher.IncludeSubdirectories = false;

                watcher.Changed += watcher_Changed;
                watcher.Created += watcher_Changed;
                watcher.Deleted += watcher_Deleted;
                watcher.Renamed += watcher_Renamed;

                lock (scanUpdateLockObj)
                {
                    watcher.EnableRaisingEvents = true;

                    string[] fileList = Directory.GetFiles(repPath);
                    foreach (string file in fileList)
                    {
                        if (stateControl.Current == States.Closing)
                            break;

                        if (!FileWatcher.IsFileSupported(file))
                            continue;

                        FileWatcher item;
                        if (!assemblies.TryGetValue(file, out item))
                            AddItem(file);
                        else
                            item.CheckForChanges();
                    }
                }

                stateControl.PushEvent(Events.DoneScanning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                stateControl.PushEvent(Events.ScanFailed);
            }
        }

        private void AddItem(string file)
        {
            var item = new FileWatcher(file, logger);
            item.Added += (a, m) => Added(new AlgoRepositoryEventArgs(this, m, a.FileName));
            item.Removed += (a, m) => Removed(new AlgoRepositoryEventArgs(this, m, a.FileName));
            item.Replaced += (a, m) => Replaced(new AlgoRepositoryEventArgs(this, m, a.FileName));
            assemblies.Add(file, item);
            item.Start();
        }

        void watcher_Error(object sender, ErrorEventArgs e)
        {
            stateControl.ModifyConditions(() => isWatcherFailed = true);
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            lock (scanUpdateLockObj)
            {
                if (!FileWatcher.IsFileSupported(e.FullPath))
                    return;

                FileWatcher assembly;
                if (assemblies.TryGetValue(e.FullPath, out assembly))
                    assembly.CheckForChanges();
                else
                {
                    assembly = new FileWatcher(e.FullPath, logger);
                    assemblies.Add(e.FullPath, assembly);
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

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (scanUpdateLockObj)
            {
                if (!FileWatcher.IsFileSupported(e.FullPath))
                    return;

                FileWatcher assembly;
                if (assemblies.TryGetValue(e.FullPath, out assembly))
                    assembly.CheckForChanges();
                else
                    AddItem(e.FullPath);
            }
        }

        public void Dispose()
        {
        }
    }

    public class AlgoRepositoryEventArgs
    {
        public AlgoRepositoryEventArgs(AlgoRepository rep, AlgoPluginRef pRef, string fileName)
        {
            Repository = rep;
            FileName = fileName;
            PluginRef = pRef;
        }

        public AlgoPluginRef PluginRef { get; private set; }
        public string FileName { get; private set; }
        public AlgoRepository Repository { get; private set; }
    }
}
