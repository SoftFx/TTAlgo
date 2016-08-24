using Machinarium.Qnil;
using Machinarium.State;
using NLog;
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
    internal class SymbolCollectionModel : IDynamicDictionarySource<string, SymbolModel>
    {
        private Logger logger;
        public enum States { Offline, WatingData, Canceled, Online, UpdatingSubscription, Stopping }
        public enum Events { Disconnected, OnConnecting, SybmolsArrived, ConnectCanceled, DoneUpdating, DoneStopping }

        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private DynamicDictionary<string, SymbolModel> symbols = new DynamicDictionary<string, SymbolModel>();
        private ConnectionModel connection;
        private IEnumerable<SymbolInfo> snapshot;
        private bool isSymbolsArrived;
        private bool pendingSubscribe;
        private TriggeredActivity updateSubscriptionActivity;
        private ActionBlock<Quote> rateUpdater;
        private List<Algo.Core.SymbolEntity> algoSymbolCache = new List<Algo.Core.SymbolEntity>();

        public event DictionaryUpdateHandler<string, SymbolModel> Updated { add { symbols.Updated += value; } remove { symbols.Updated -= value; } }

        public SymbolCollectionModel(ConnectionModel connection)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.connection = connection;

            updateSubscriptionActivity = new TriggeredActivity(UpdateSubscription);

            stateControl.AddTransition(States.Offline, Events.OnConnecting, States.WatingData);
            stateControl.AddTransition(States.WatingData, () => isSymbolsArrived, States.UpdatingSubscription);
            stateControl.AddTransition(States.WatingData, Events.ConnectCanceled, States.Canceled);
            stateControl.AddTransition(States.UpdatingSubscription, Events.Disconnected, States.Stopping);
            stateControl.AddTransition(States.UpdatingSubscription, Events.DoneUpdating, States.Online);
            stateControl.AddTransition(States.Online, () => pendingSubscribe, States.UpdatingSubscription);
            stateControl.AddTransition(States.Online, Events.Disconnected, States.Stopping);
            stateControl.AddTransition(States.Stopping, Events.DoneStopping, States.Offline);

            stateControl.OnEnter(States.UpdatingSubscription, () => updateSubscriptionActivity.Trigger());
            stateControl.OnEnter(States.Stopping, Stop);
            stateControl.OnEnter(States.Offline, ResetSubscription);
            stateControl.OnExit(States.WatingData, () => Merge(snapshot));

            connection.Connecting += () =>
            {
                connection.FeedProxy.SymbolInfo += FeedProxy_SymbolInfo;
                connection.FeedProxy.Tick += FeedProxy_Tick;
                isSymbolsArrived = false;
                stateControl.PushEvent(Events.OnConnecting);
            };

            connection.Disconnecting += () =>
            {
                connection.FeedProxy.SymbolInfo -= FeedProxy_SymbolInfo;
                connection.FeedProxy.Tick -= FeedProxy_Tick;
            };

            //connection.Initalizing += (s, c) =>
            //{
            //    isSymbolsArrived = false;
            //    c.Register(() => stateControl.PushEvent(Events.ConnectCanceled));
            //    return stateControl.PushEventAndWait(Events.OnConnecting, state => state == States.Online || state == States.Canceled);
            //};

            connection.Deinitalizing += (s, c) => stateControl.PushEventAndWait(Events.Disconnected, States.Offline);

            stateControl.StateChanged += (from, to) => logger.Debug("STATE " + from + " => " + to);
            stateControl.EventFired += e => logger.Debug("EVENT " + e);

            rateUpdater = DataflowHelper.CreateUiActionBlock<Quote>(UpdateRate, 100, 100, CancellationToken.None);
        }

        public States State { get { return stateControl.Current; } }
        public IEnumerable<Algo.Core.SymbolEntity> AlgoSymbolCache { get { return algoSymbolCache; } }

        public IReadOnlyDictionary<string, SymbolModel> Snapshot { get { return symbols.Snapshot; } }

        private void UpdateRate(Quote tick)
        {
            SymbolModel symbol;
            if (symbols.TryGetValue(tick.Symbol, out symbol))
                symbol.CastNewTick(tick);
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
                    if (!SubscriptionInfo.IsEquals(symbol.RequestedSubscription, symbol.CurrentSubscription))
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
                logger.Debug("SubscribeToQuotes(" + toSubscribe.Count + ")");
                await Task.Factory.StartNew(() => connection.FeedProxy.Server.SubscribeToQuotes(toSubscribe, 1));
            }
            catch (Exception ex) { logger.Error("UpdateSubscription ERROR: " + ex.Message); }
            stateControl.PushEvent(Events.DoneUpdating);
        }

        private async void Stop()
        {
            await updateSubscriptionActivity.Stop();
            stateControl.PushEvent(Events.DoneStopping);
        }

        void FeedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        {
            logger.Debug("EVENT SymbolsArrived");

            snapshot = e.Information;
            //algoSymbolCache.Clear();
            algoSymbolCache = snapshot.Select(FdkToAlgo.Convert).ToList();
            stateControl.ModifyConditions(() => isSymbolsArrived = true);
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
                }
            }

            // delete
            List<SymbolModel> toRemove = new List<SymbolModel>();
            foreach (var symbolModel in symbols.Values)
            {
                if (!freshSnapshotDic.ContainsKey(symbolModel.Name))
                    toRemove.Add(symbolModel);
            }

            foreach (var model in toRemove)
                symbols.Remove(model.Name);
        }

        private void ResetSubscription()
        {
            foreach (var smb in symbols.Values) smb.CurrentSubscription = new SubscriptionInfo();
        }

        public void Dispose()
        {
        }

        //        public event Action<SymbolModel> Added = delegate { };
        //      public event Action<SymbolModel> Removed = delegate { };

        public SymbolModel GetOrDefault(string key)
        {
            SymbolModel result;
            this.symbols.TryGetValue(key, out result);
            return result;
        }

        public SymbolModel this[string key]
        {
            get
            {
                SymbolModel result;
                if (!this.symbols.TryGetValue(key, out result))
                    throw new ArgumentException("Symbol Not Found: " + key);
                return result;
            }
        }
    }
}
