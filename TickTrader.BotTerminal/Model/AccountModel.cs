using Machinarium.State;
using NLog;
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
        private Logger logger;
        public enum States { Offline, WaitingData, Canceled, Online, Deinitializing }
        public enum Events { Connected, ConnectionCanceled, CacheInitialized, Diconnected, DoneInit, DoneDeinit }

        private StateMachine<States> stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
        private ObservableDictionary<string, PositionModel> positions = new ObservableDictionary<string, PositionModel>();
        private ObservableDictionary<string, AssetModel> assets = new ObservableDictionary<string, AssetModel>();
        private ObservableDictionary<string, OrderModel> orders = new ObservableDictionary<string, OrderModel>();
        private ConnectionModel connection;
        private ActionBlock<Action> uiUpdater;
        private AccountType? accType;
        //private RepeatableActivity initActivity;

        public AccountModel(ConnectionModel connection)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.connection = connection;
            this.Positions = positions.AsReadonly();
            this.Orders = orders.AsReadonly();
            this.Assets = assets.AsReadonly();
            //this.initActivity = new RepeatableActivity(Init);

            stateControl.AddTransition(States.Offline, Events.Connected, States.WaitingData);
            stateControl.AddTransition(States.WaitingData, Events.CacheInitialized, States.Online);
            stateControl.AddTransition(States.WaitingData, Events.Diconnected, States.Offline);
            stateControl.AddTransition(States.Online, Events.Diconnected, States.Deinitializing);
            stateControl.AddTransition(States.Deinitializing, Events.DoneDeinit, States.Offline);

            stateControl.OnEnter(States.WaitingData, Init);
            stateControl.OnEnter(States.Online, UpdateSnapshots);
            stateControl.OnEnter(States.Deinitializing, Deinit);

            connection.Connecting += () =>
                {
                    connection.TradeProxy.CacheInitialized += TradeProxy_CacheInitialized;
                    connection.TradeProxy.AccountInfo += TradeProxy_AccountInfo;
                    connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                    connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                    connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                };

            connection.Disconnecting += () =>
            {
                connection.TradeProxy.CacheInitialized -= TradeProxy_CacheInitialized;
                connection.TradeProxy.AccountInfo -= TradeProxy_AccountInfo;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;
            };

            connection.Connected += () => stateControl.PushEvent(Events.Connected);

            //connection.Initalizing += (s, c) =>
            //{
            //    c.Register(() => stateControl.PushEvent(Events.ConnectionCanceled));
            //    return stateControl.PushEventAndWait(Events.Connected, state => state == States.Online || state == States.Canceled);
            //};

            connection.Deinitalizing += (s, c) => stateControl.PushEventAndWait(Events.Diconnected, States.Offline);

            stateControl.StateChanged += (from, to) => logger.Debug("STATE " + from + " => " + to);
            stateControl.EventFired += e => logger.Debug("EVENT " + e);
        }

        private void TradeProxy_TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            var a = e.Report;
        }

        public event Action AccountTypeChanged = delegate { };
        public ReadonlyDictionaryObserver<string, PositionModel> Positions { get; private set; }
        public ReadonlyDictionaryObserver<string, OrderModel> Orders { get; private set; }
        public ReadonlyDictionaryObserver<string, AssetModel> Assets { get; private set; }
        public AccountType? Type
        {
            get { return accType; }
            private set
            {
                if (accType != value)
                {
                    accType = value;
                    AccountTypeChanged();
                }
            }
        }
        public IStateProvider<States> State { get { return stateControl; } }

        public void Init()
        {
            this.uiUpdater = DataflowHelper.CreateUiActionBlock<Action>(a => a(), 100, 100, CancellationToken.None);
            positions.Clear();
            orders.Clear();
            assets.Clear();
        }

        public void UpdateSnapshots()
        {
            var histroy = connection.TradeProxy.Server.GetTradeTransactionReports(TimeDirection.Backward, true, DateTime.MinValue, DateTime.Now).ToArray();

            Type = connection.TradeProxy.Cache.AccountInfo.Type;

            var fdkPositionsArray = connection.TradeProxy.Cache.Positions;
            foreach (var fdkPosition in fdkPositionsArray)
                positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition));

            var fdkOrdersArray = connection.TradeProxy.Cache.TradeRecords;
            foreach (var fdkOrder in fdkOrdersArray)
                orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder));

            var fdkAssetsArray = connection.TradeProxy.Cache.AccountInfo.Assets;
            foreach (var fdkAsset in fdkAssetsArray)
                assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset));

            //stateControl.PushEvent(Events.DoneInit);
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

        private void UpsertPosition(Position report)
        {
            PositionModel position;
            if (positions.TryGetValue(report.Symbol, out position))
                position.Update(report);
            else
                positions.Add(report.Symbol, new PositionModel(report));
        }

        void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            if (IsEmpty(e.Report))
                uiUpdater.SendAsync(() => positions.Remove(e.Report.Symbol));
            else
                uiUpdater.SendAsync(() => UpsertPosition(e.Report));
        }



        void TradeProxy_ExecutionReport(object sender, SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
            var execType = e.Report.ExecutionType;

#warning Expression(execType == ExecutionType.Replace) used in two cases
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

            if (Type == AccountType.Cash)
                uiUpdater.SendAsync(() =>
                {
                    foreach (var asset in e.Report.Assets)
                        UpdateAsset(asset);
                });
        }

        private void UpdateAsset(AssetInfo assetInfo)
        {
            AssetModel asset;
            if (assets.TryGetValue(assetInfo.Currency, out asset))
            {
                if (IsEmpty(assetInfo))
                    assets.Remove(asset.Currency);
                else
                    asset.Update(assetInfo);
            }
            else
                assets.Add(assetInfo.Currency, new AssetModel(assetInfo));
        }

        void TradeProxy_AccountInfo(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        {
            Type = e.Information.Type;
        }

        void TradeProxy_CacheInitialized(object sender, SoftFX.Extended.Events.CacheEventArgs e)
        {
            stateControl.PushEvent(Events.CacheInitialized);
        }

        private bool IsEmpty(Position position)
        {
            return position.BuyAmount == 0
               && position.SellAmount == 0;
        }

        private bool IsEmpty(AssetInfo assetInfo)
        {
            return assetInfo.Balance == 0;
        }
    }
}
