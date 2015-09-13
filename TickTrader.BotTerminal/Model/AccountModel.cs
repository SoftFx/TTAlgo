using SoftFX.Extended;
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
    internal class AccountModel
    {
        public enum States { Offline, WaitingData, Online, Deinitializing}
        public enum Events { Connected, CacheInitialized, Diconnected, DoneInit, DoneDeinit }

        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private ObservableDictionary<string, PositionModel> positions = new ObservableDictionary<string, PositionModel>();
        private ObservableDictionary<string, OrderModel> orders = new ObservableDictionary<string, OrderModel>();
        private ConnectionModel connection;
        private ActionBlock<Action> uiUpdater;
        //private RepeatableActivity initActivity;

        public AccountModel(ConnectionModel connection)
        {
            this.connection = connection;
            this.Positions = positions.AsReadonly();
            this.Orders = orders.AsReadonly();
            //this.initActivity = new RepeatableActivity(Init);

            stateControl.AddTransition(States.Offline, Events.Connected, States.WaitingData);
            stateControl.AddTransition(States.WaitingData, Events.CacheInitialized, States.Online);
            stateControl.AddTransition(States.WaitingData, Events.Diconnected, States.Offline);
            stateControl.AddTransition(States.Online, Events.Diconnected, States.Deinitializing);
            stateControl.AddTransition(States.Deinitializing, Events.DoneDeinit, States.Offline);

            stateControl.OnEnter(States.WaitingData, Init);
            stateControl.OnEnter(States.Online, UpdateSnapshots);
            stateControl.OnEnter(States.Deinitializing, Deinit);

            connection.Initialized += () =>
                {
                    connection.TradeProxy.CacheInitialized += TradeProxy_CacheInitialized;
                    connection.TradeProxy.AccountInfo += TradeProxy_AccountInfo;
                    connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                    connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                };

            connection.Deinitialized += () =>
            {
                connection.TradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
                connection.TradeProxy.AccountInfo -= TradeProxy_AccountInfo;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
            };

            connection.Connected += () => stateControl.PushEvent(Events.Connected);
            connection.Disconnected += s => stateControl.PushEventAndAsyncWait(Events.Diconnected, States.Offline);

            stateControl.StateChanged += (from, to) => System.Diagnostics.Debug.WriteLine("AccountModel STATE " + from + " => " + to);
            stateControl.EventFired += e => System.Diagnostics.Debug.WriteLine("AccountModel EVENT " + e);
        }

        public ReadonlyDictionaryObserver<string, PositionModel> Positions { get; private set; }
        public ReadonlyDictionaryObserver<string, OrderModel> Orders { get; private set; }
        public IStateProvider<States> State { get { return stateControl; } }

        public void Init()
        {
            this.uiUpdater = DataflowHelper.CreateUiActionBlock<Action>(a => a(), 100, 100, CancellationToken.None);
            positions.Clear();
            orders.Clear();
        }

        public void UpdateSnapshots()
        {
            var fdkPositionsArray = connection.TradeProxy.Cache.Positions;
            foreach (var fdkPosition in fdkPositionsArray)
                positions.Add(fdkPosition.Symbol, new PositionModel());

            var fdkOrdersArray = connection.TradeProxy.Cache.TradeRecords;
            foreach (var fdkOrder in fdkOrdersArray)
                orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder));
            
            stateControl.PushEvent(Events.DoneInit);
        }

        public async void Deinit()
        {
            await Task.Factory.StartNew(() =>
                {
                    uiUpdater.Complete();
                    uiUpdater.Completion.Wait();
                });
            stateControl.PushEvent(Events.DoneDeinit);
        }

        private void UpsertOrder(ExecutionReport report)
        {
            OrderModel order;
            if (orders.TryGetValue(report.OrderId, out order))
                order.Update(report);
            else
                orders.Add(report.OrderId, new OrderModel(report));
        }

        void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            //uiUpdater.SendAsync(()=>e.
        }

        void TradeProxy_ExecutionReport(object sender, SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
            var execType = e.Report.ExecutionType;

            if (execType == ExecutionType.Canceled
                || execType == ExecutionType.Expired
                || execType == ExecutionType.Replace
                || execType == ExecutionType.Rejected
                || e.Report.LeavesVolume == 0)
            {
                uiUpdater.SendAsync(() => orders.Remove(e.Report.OrderId));
            }
            else if (execType == ExecutionType.Calculated
                || execType == ExecutionType.Replace
                || execType == ExecutionType.Trade
                || execType == ExecutionType.PendingCancel
                || execType == ExecutionType.PendingReplace)
            {
                uiUpdater.SendAsync(() => UpsertOrder(e.Report));
            }
        }

        void TradeProxy_AccountInfo(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        {
        }

        void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            stateControl.PushEvent(Events.CacheInitialized);
        }
    }
}
