using SoftFX.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SymbolCollectionModel : IEnumerable<SymbolModel>
    {
        public enum States { Offline, Online,  UpdatingSubscription, Stopping }
        public enum Events { Disconnected, SybmolsArrived, DoneUpdating, DoneStopping }

        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private Dictionary<string, SymbolModel> symbols = new Dictionary<string, SymbolModel>();
        private ConnectionModel connection;
        private IEnumerable<SymbolInfo> snapshot;
        private bool pendingSubscribe;
        private TriggeredActivity updateSubscriptionActivity;
        private ActionBlock<Quote> rateUpdater;

        public SymbolCollectionModel(ConnectionModel connection)
        {
            this.connection = connection;

            updateSubscriptionActivity = new TriggeredActivity(UpdateSubscription);

            stateControl.AddTransition(States.Offline, Events.SybmolsArrived, States.UpdatingSubscription);
            stateControl.AddTransition(States.UpdatingSubscription, Events.Disconnected, States.Stopping);
            stateControl.AddTransition(States.UpdatingSubscription, Events.DoneUpdating, States.Online);
            stateControl.AddTransition(States.Online, () => pendingSubscribe, States.UpdatingSubscription);
            stateControl.AddTransition(States.Online, Events.Disconnected, States.Stopping);
            stateControl.AddTransition(States.Stopping, Events.DoneStopping, States.Offline);

            stateControl.OnEnter(States.UpdatingSubscription, () => updateSubscriptionActivity.Trigger());
            stateControl.OnEnter(States.Stopping, Stop);
            stateControl.OnEnter(States.Offline, ResetSubscription);
            stateControl.OnExit(States.Offline, () => Merge(snapshot));

            connection.Initialized += connection_Initialized;
            connection.Deinitialized += connection_Deinitialized;
            connection.Disconnected += s => stateControl.PushEventAndAsyncWait(Events.Disconnected, States.Offline);

            stateControl.StateChanged += (from, to) => System.Diagnostics.Debug.WriteLine("SymbolListModel STATE " + from + " => " + to);

            rateUpdater = DataflowHelper.CreateUiActionBlock<Quote>(UpdateRate, 100, 100, CancellationToken.None);
        }

        private void UpdateRate(Quote tick)
        {
            SymbolModel symbol;
            if (symbols.TryGetValue(tick.Symbol, out symbol))
                symbol.CastNewTick(tick);
        }

        private void connection_Deinitialized()
        {
            connection.FeedProxy.SymbolInfo += FeedProxy_SymbolInfo;
            connection.FeedProxy.Tick += FeedProxy_Tick;
        }

        private void connection_Initialized()
        {
            connection.FeedProxy.SymbolInfo += FeedProxy_SymbolInfo;
            connection.FeedProxy.Tick += FeedProxy_Tick;
        }

        private async Task UpdateSubscription(CancellationToken cToken)
        {
            try
            {
                List<string> toSubscribe = new List<string>();
                int depth = -1;
                pendingSubscribe = false;
                foreach (SymbolModel symbol in symbols.Values)
                {
                    if (symbol.RequestedSubscription != symbol.CurrentSubscription)
                    {
                        if (depth == -1 || depth == symbol.RequestedSubscription.Depth)
                        {
                            toSubscribe.Add(symbol.Name);
                            symbol.CurrentSubscription = symbol.RequestedSubscription;
                        }
                        else
                            pendingSubscribe = true;
                    }
                }

                //await Task.Delay(1000);
                await Task.Factory.StartNew(() => connection.FeedProxy.Server.SubscribeToQuotes(toSubscribe, 1));
            }
            catch (Exception ex) { Debug.WriteLine("SymbolListModel.UpdateSubscription() ERROR: " + ex.Message); }
            stateControl.PushEvent(Events.DoneUpdating);
        }

        private async void Stop()
        {
            await updateSubscriptionActivity.Abort();            
            stateControl.PushEvent(Events.DoneStopping);
        }

        void FeedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        {
            snapshot = e.Information;
            stateControl.PushEvent(Events.SybmolsArrived);
        }

        void FeedProxy_Tick(object sender, SoftFX.Extended.Events.TickEventArgs e)
        {
            rateUpdater.SendAsync(e.Tick).Wait();
        }

        private void Merge(IEnumerable<SymbolInfo> freshSnashot)
        {
            var freshSnapshotDic = freshSnashot.ToDictionary(i => i.Name);

            // upsert
            foreach (var info in freshSnashot)
            {
                SymbolModel model;
                if (symbols.TryGetValue(info.Name, out model))
                    model.Update(info);
                else
                {
                    model = new SymbolModel(this, info);
                    symbols.Add(info.Name, model);
                    Added(model);
                }
            }

            // delete
            List<SymbolModel> toRemove = new List<SymbolModel>();
            foreach (var symbolModel in symbols.Values)
            {
                if (!freshSnapshotDic.ContainsKey(symbolModel.Name))
                    toRemove.Add(symbolModel);
            }

            foreach(var model in toRemove)
            {
                if(symbols.Remove(model.Name))
                    Removed(model);
            }
        }

        private void ResetSubscription()
        {
            foreach (var smb in symbols.Values) smb.CurrentSubscription = new SubscriptionInfo();
        }

        public event Action<SymbolModel> Added = delegate { };
        public event Action<SymbolModel> Removed = delegate { };

        public IEnumerator<SymbolModel> GetEnumerator()
        {
            return symbols.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return symbols.Values.GetEnumerator();
        }
    }
}
