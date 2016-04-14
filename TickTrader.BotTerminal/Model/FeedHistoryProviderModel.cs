using Machinarium.State;
using SoftFX.Extended;
using SoftFX.Extended.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class FeedHistoryProviderModel
    {
        private enum States { Starting, Online, Stopping, Offline }
        private enum Events { Start, Initialized, InitFailed, Stopped }

        private StateMachine<States> stateControl = new StateMachine<States>(States.Offline);
        private DataFeedStorage fdkStorage;
        private bool stopRequested;
        private DataFeed feedProxy;
        private BufferBlock<Task> requestQueue = new BufferBlock<Task>();
        private ActionBlock<Task> requestProcessor;
        private IDisposable pipeLink;

        public FeedHistoryProviderModel()
        {
            stateControl.AddTransition(States.Offline, Events.Start, States.Starting);
            stateControl.AddTransition(States.Starting, Events.Initialized, States.Online);
            stateControl.AddTransition(States.Starting, Events.InitFailed, States.Stopping);
            stateControl.AddTransition(States.Online, () => stopRequested, States.Stopping);
            stateControl.AddTransition(States.Stopping, Events.Stopped, States.Offline);

            stateControl.OnEnter(States.Starting, Init);
            stateControl.OnEnter(States.Stopping, Stop);
            stateControl.OnEnter(States.Offline, Reset);

            stateControl.StateChanged += (from, to) => System.Diagnostics.Debug.WriteLine("FeedHistoryProviderModel STATE " + from + " => " + to);

            requestProcessor = new ActionBlock<Task>(t => t.RunSynchronously(), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });
        }

        public void Start(DataFeed feedProxy)
        {
            this.feedProxy = feedProxy;
            stateControl.PushEvent(Events.Start);
        }

        private async void Init()
        {
            try
            {
                fdkStorage = await Task.Factory.StartNew(() => new DataFeedStorage(EnvService.Instance.FeedHistoryCacheFolder, StorageProvider.SQLite, feedProxy, false));

                pipeLink = requestQueue.LinkTo(requestProcessor); // start processing
                stateControl.PushEvent(Events.Initialized);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FeedHistoryProviderModel.Init() ERROR " + ex.ToString());
                stateControl.PushEvent(Events.InitFailed);
            }
        }

        private async void Stop()
        {
            try
            {
                pipeLink.Dispose(); // deattach buffer from the processor

                await Task.Factory.StartNew(() => fdkStorage.Dispose());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FeedHistoryProviderModel.Init() ERROR " + ex.ToString());
            }

            stateControl.PushEvent(Events.Stopped);
        }

        private void Reset()
        {
            stopRequested = false;
        }

        public Task<Quote[]> GetTicks(string symbol, DateTime startTime, DateTime endTime, int depth)
        {
            return Enqueue(() => fdkStorage.Online.GetQuotes(symbol, startTime, endTime, depth));
        }

        private Task<TResult> Enqueue<TResult>(Func<TResult> handler)
        {
            Task<TResult> task = new Task<TResult>(handler);
            requestQueue.Post(task);
            return task;
        }

        public Task Shutdown()
        {
            stateControl.ModifyConditions(() => stopRequested = true);
            return stateControl.AsyncWait(States.Offline);
        }
    }
}
