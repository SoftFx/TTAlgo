﻿using Machinarium.State;
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
        public enum Events { Start, DoneScanning, ScanFailed, NextAttempt, CloseRequested, DoneClosing, WatcherFail }

        private StateMachine<States> stateControl = new StateMachine<States>();
        private object scanUpdateLockObj = new object();
        private object globalLockObj = new object();
        private FileSystemWatcher watcher;
        private IAlgoCoreLogger logger;
        //private bool isWatcherFailed;
        private Task scanTask;
        private string repPath;
        private bool _isolation;
        private Dictionary<string, FileWatcher> assemblies = new Dictionary<string, FileWatcher>();

        public AlgoRepository(string repPath, bool isolate = true, IAlgoCoreLogger logger = null)
        {
            this.repPath = repPath;
            this.logger = logger;
            _isolation = isolate;

            stateControl.AddTransition(States.Created, Events.Start, States.Scanning);
            stateControl.AddTransition(States.Scanning, Events.DoneScanning, States.Watching);
            //stateControl.AddTransition(States.Scanning, Events.WatcherFail, () => isWatcherFailed = true);
            stateControl.AddTransition(States.Scanning, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Scanning, Events.ScanFailed, States.Waiting);
            stateControl.AddTransition(States.Waiting, Events.NextAttempt, States.Scanning);
            stateControl.AddTransition(States.Waiting, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Watching, Events.CloseRequested, States.Closing);
            stateControl.AddTransition(States.Watching, Events.WatcherFail, States.Scanning);

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

        public async Task WaitInit()
        {
            await stateControl.AsyncWait(States.Watching);
            await Task.WhenAll(assemblies.Values.Select(a => a.WaitReady()));
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

                //isWatcherFailed = false;

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
            var item = new FileWatcher(file, logger, _isolation);
            item.Added += (a, m) => Added(new AlgoRepositoryEventArgs(this, m, a.FileName, a.FilePath));
            item.Removed += (a, m) => Removed(new AlgoRepositoryEventArgs(this, m, a.FileName, a.FilePath));
            item.Replaced += (a, m) => Replaced(new AlgoRepositoryEventArgs(this, m, a.FileName, a.FilePath));
            assemblies.Add(file, item);
            item.Start();
        }

        void watcher_Error(object sender, ErrorEventArgs e)
        {
            stateControl.PushEvent(Events.WatcherFail);
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
                    assembly = new FileWatcher(e.FullPath, logger, _isolation);
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
        public AlgoRepositoryEventArgs(AlgoRepository rep, AlgoPluginRef pRef, string fileName, string filePath)
        {
            Repository = rep;
            FileName = fileName;
            FilePath = filePath;
            PluginRef = pRef;
        }

        public AlgoPluginRef PluginRef { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public AlgoRepository Repository { get; private set; }
    }
}
