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
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class AccountModel : NoTimeoutByRefObject, IAccountInfoProvider
    {
        private Logger logger;
        public enum States { Offline, WaitingData, Canceled, Online, Deinitializing }
        public enum Events { Connected, ConnectionCanceled, CacheInitialized, Diconnected, DoneDeinit }

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
                    connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
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
        public double Balance { get; private set; }
        public string BalanceCurrencyCode { get; private set; }
        public string Account { get; private set; }

        public void Init()
        {
            this.uiUpdater = DataflowHelper.CreateUiActionBlock<Action>(a => a(), 100, 100, CancellationToken.None);
            positions.Clear();
            orders.Clear();
            assets.Clear();
        }

        public void UpdateSnapshots()
        {
            var accInfo = connection.TradeProxy.Cache.AccountInfo;

            Account = accInfo.AccountId;
            Type = accInfo.Type;
            Balance = accInfo.Balance;
            BalanceCurrencyCode = accInfo.Currency;

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

        private OrderModel UpsertOrder(ExecutionReport report)
        {
            OrderModel order;
            if (orders.TryGetValue(report.OrderId, out order))
                order.Update(report);
            else
            {
                order = new OrderModel(report);
                orders.Add(report.OrderId, order);
            }
            return order;
        }

        private void UpsertPosition(Position report)
        {
            PositionModel position;
            if (positions.TryGetValue(report.Symbol, out position))
                position.Update(report);
            else
                positions.Add(report.Symbol, new PositionModel(report));
        }

        private void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            if (IsEmpty(e.Report))
                uiUpdater.SendAsync(() => positions.Remove(e.Report.Symbol));
            else
                uiUpdater.SendAsync(() => UpsertPosition(e.Report));
        }

        private void TradeProxy_ExecutionReport(object sender, SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
            uiUpdater.SendAsync(() => ApplyReport(e.Report));
        }

        private void TradeProxy_BalanceOperation(object sender, SoftFX.Extended.Events.NotificationEventArgs<BalanceOperation> e)
        {
            uiUpdater.SendAsync(() =>
            {
                if (Type == AccountType.Gross || Type == AccountType.Net)
                    Balance = e.Data.Balance;
                else if (Type == AccountType.Cash)
                    assets[e.Data.TransactionCurrency] = new AssetModel(e.Data.Balance, e.Data.TransactionCurrency);

                AlgoEvent_BalanceUpdated(new BalanceOperationReport(e.Data.Balance, e.Data.TransactionCurrency));
            });
        }

        private void TradeProxy_TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            var a = e.Report;
        }

        private void ApplyReport(ExecutionReport report)
        {
            switch (report.ExecutionType)
            {
                case ExecutionType.Calculated:
                    if (report.OrderType == TradeRecordType.Position)
                        OnOrderUpdated(report, OrderExecAction.Opened);
                    else
                        OnOrderAdded(report, OrderExecAction.Opened);
                    break;

                case ExecutionType.Replace:
                    OnOrderUpdated(report, OrderExecAction.Modified);
                    break;

                case ExecutionType.Expired:
                    OnOrderRemoved(report, OrderExecAction.Expired);
                    break;

                case ExecutionType.Canceled:
                    OnOrderRemoved(report, OrderExecAction.Canceled);
                    break;

                case ExecutionType.Trade:
                    if (report.OrderType == TradeRecordType.Limit
                        || report.OrderType == TradeRecordType.Stop)
                    {
                        if (report.LeavesVolume != 0)
                            OnOrderUpdated(report, OrderExecAction.Filled);
                        else if (accType != AccountType.Gross)
                            OnOrderRemoved(report, OrderExecAction.Filled);
                    }
                    else if (report.OrderType == TradeRecordType.Position)
                    {
                        if (report.LeavesVolume != 0)
                            OnOrderUpdated(report, OrderExecAction.Closed);
                        else
                            OnOrderRemoved(report, OrderExecAction.Closed);
                    }
                    break;
            }

            if (Type == AccountType.Cash)
            {
                foreach (var asset in report.Assets)
                    UpdateAsset(asset);
            }
        }

        private void OnOrderAdded(ExecutionReport report, OrderExecAction algoAction)
        {
            var order = UpsertOrder(report);
            ExecReportToAlgo(algoAction, OrderEntityAction.Added, report, order);
        }

        private void OnOrderRemoved(ExecutionReport report, OrderExecAction algoAction)
        {
            orders.Remove(report.OrderId);
            ExecReportToAlgo(algoAction, OrderEntityAction.Removed, report);
        }

        private void OnOrderUpdated(ExecutionReport report, OrderExecAction algoAction)
        {
            var order = UpsertOrder(report);
            ExecReportToAlgo(algoAction, OrderEntityAction.Updated, report, order);
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

        #region IAccountInfoProvider

        private event Action<OrderExecReport> AlgoEvent_OrderUpdated = delegate { };
        private event Action<PositionExecReport> AlgoEvent_PositionUpdated = delegate { };
        private event Action<BalanceOperationReport> AlgoEvent_BalanceUpdated = delegate { };

        private void ExecReportToAlgo(OrderExecAction action, OrderEntityAction entityAction, ExecutionReport report, OrderModel newOrder = null)
        {
            OrderExecReport algoReport = new OrderExecReport();
            if (newOrder != null)
                algoReport.OrderCopy = newOrder.ToAlgoOrder();
            algoReport.OrderId = report.OrderId;
            algoReport.ExecAction = action;
            algoReport.Action = entityAction;
            if (!double.IsNaN(report.Balance))
                algoReport.NewBalance = report.Balance;
            AlgoEvent_OrderUpdated(algoReport);
        }

        AccountTypes IAccountInfoProvider.AccountType { get { return FdkToAlgo.Convert(Type.Value); } }

        void IAccountInfoProvider.SyncInvoke(Action action)
        {
            Caliburn.Micro.Execute.OnUIThread(action);
        }

        List<OrderEntity> IAccountInfoProvider.GetOrders()
        {
            return Orders.Select(pair => pair.Value.ToAlgoOrder()).ToList();
        }

        IEnumerable<OrderEntity> IAccountInfoProvider.GetPosition()
        {
            throw new NotImplementedException();
        }

        IEnumerable<AssetEntity> IAccountInfoProvider.GetAssets()
        {
            return Assets.Select(pair => pair.Value.ToAlgoAsset()).ToList();
        }

        event Action<OrderExecReport> IAccountInfoProvider.OrderUpdated
        {
            add { AlgoEvent_OrderUpdated += value; }
            remove { AlgoEvent_OrderUpdated -= value; }
        }

        event Action<PositionExecReport> IAccountInfoProvider.PositionUpdated
        {
            add { AlgoEvent_PositionUpdated += value; }
            remove { AlgoEvent_PositionUpdated -= value; }
        }

        event Action<BalanceOperationReport> IAccountInfoProvider.BalanceUpdated
        {
            add { AlgoEvent_BalanceUpdated += value; }
            remove { AlgoEvent_BalanceUpdated -= value; }
        }

        #endregion
    }
}
