using StateMachinarium;
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

namespace TickTrader.Algo.Core.Repository
{
    public class AlgoRepository : IDisposable
    {
        private const string AlgoFilesPattern = "*.ttalgo";

        public enum States { Created, Scanning, Waiting, Watching, Closing, Closed }
        public enum Events { Start, DoneScanning, ScanFailed, NextAttempt, CloseRequested, DoneClosing }

        private StateMachine<States> stateControl = new StateMachine<States>();
        private FileSystemWatcher watcher;
        private bool isWatcherFailed;
        private Task scanTask;
        private string repPath;
        private Dictionary<string, AlgoAssembly> assemblies = new Dictionary<string, AlgoAssembly>();

        public AlgoRepository(string repPath)
        {
            this.repPath = repPath;

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

            stateControl.PushEvent(Events.Start);

            //watcher = new FileSystemWatcher(repPath, AlgoFilesPattern);
        }

        public AlgoRepositoryItem Items { get; set; }

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
                }

                watcher = new FileSystemWatcher(repPath);
                watcher.Path = repPath;
                watcher.IncludeSubdirectories = false;
                watcher.Filter = AlgoFilesPattern;

                watcher.Changed += watcher_Changed;
                watcher.Created += watcher_Changed;
                watcher.Deleted += watcher_Deleted;
                watcher.Renamed += watcher_Renamed;

                watcher.EnableRaisingEvents = true;

                string[] fileList = Directory.GetFiles(repPath, AlgoFilesPattern, SearchOption.AllDirectories);
                foreach (string file in fileList)
                {
                    if (stateControl.Current == States.Closing)
                        break;

                    AlgoAssembly assemblyMetadata;
                    if (!assemblies.TryGetValue(file, out assemblyMetadata))
                    {
                        assemblyMetadata = new AlgoAssembly(file);
                        assemblies.Add(file, assemblyMetadata);
                    }
                    else
                        assemblyMetadata.CheckForChanges();
                }

                stateControl.PushEvent(Events.DoneScanning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                stateControl.PushEvent(Events.ScanFailed);
            }
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            AlgoAssembly assembly;
            if (assemblies.TryGetValue(e.OldFullPath, out assembly))
            {
                assemblies.Remove(e.OldFullPath);
                assemblies.Add(e.FullPath, assembly);
                assembly.Rename(e.FullPath);
            }
            else if (assemblies.TryGetValue(e.FullPath, out assembly))
            {
                // I dunno
            }
            else
            {
                assembly = new AlgoAssembly(e.FullPath);
                assemblies.Add(e.FullPath, assembly);
            }
        }

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            AlgoAssembly assembly;
            if (assemblies.TryGetValue(e.FullPath, out assembly))
                assembly.CheckForChanges();
            else
            {
                assembly = new AlgoAssembly(e.FullPath);
                assemblies.Add(e.FullPath, assembly);
            }
        }

        public void Dispose()
        {
        }
    }
}
