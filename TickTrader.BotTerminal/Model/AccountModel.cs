using Caliburn.Micro;
using Machinarium.State;
using NLog;
using SoftFX.Extended;
using SoftFX.Extended.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Api;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class AccountModel : NoTimeoutByRefObject, IAccountInfoProvider
    {
        private Logger logger;
        private DynamicDictionary<string, PositionModel> positions = new DynamicDictionary<string, PositionModel>();
        private DynamicDictionary<string, AssetModel> assets = new DynamicDictionary<string, AssetModel>();
        private DynamicDictionary<string, OrderModel> orders = new DynamicDictionary<string, OrderModel>();
        private ConnectionModel connection;
        private SymbolCollectionModel symbols;
        private ActionBlock<System.Action> uiUpdater;
        private AccountType? accType;

        public AccountModel(ConnectionModel connection, SymbolCollectionModel symbols)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            this.connection = connection;
            this.symbols = symbols;
            TradeHistory = new TradeHistoryProvider(connection);

            connection.State.StateChanged += State_StateChanged;
            //connection.SysInitalizing += Connection_Initalizing;
            connection.Connected += Connection_Connected;
            connection.SysDeinitalizing += Connection_Deinitalizing;

            connection.Connecting += () =>
            {
                connection.TradeProxy.AccountInfo += AccountInfoChanged;
                connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
                connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
            };

            connection.Disconnecting += () =>
            {
                connection.TradeProxy.AccountInfo -= AccountInfoChanged;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
            };
        }

        private void Connection_Connected()
        {
            Init();
        }

        private void State_StateChanged(ConnectionModel.States oldState, ConnectionModel.States newState)
        {
            if (newState == ConnectionModel.States.Connecting)
            {
                positions.Clear();
                orders.Clear();
                assets.Clear();
            }
        }

        private Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            return Init();
        }

        private Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            return Deinit();
        }

        public event System.Action AccountTypeChanged = delegate { };
        public IDynamicDictionarySource<string, PositionModel> Positions { get { return positions; } }
        public IDynamicDictionarySource<string, OrderModel> Orders { get { return orders; } }
        public IDynamicDictionarySource<string, AssetModel> Assets { get { return assets; } }
        public ConnectionModel Connection { get { return connection; } }
        public TradeHistoryProvider TradeHistory { get; private set; }
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

        public double Balance { get; private set; }
        public string BalanceCurrencyCode { get; private set; }
        public string Account { get; private set; }

        public event AsyncEventHandler Starting; // main thread event
        public event AsyncEventHandler Stopping; // background thread event

        public async Task Init()
        {
            Action<System.Action> uiActionhandler = (a) =>
            {
                try
                {
                    a();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ui Action failed.");
                }
            };

            this.uiUpdater = DataflowHelper.CreateUiActionBlock<System.Action>(uiActionhandler, 100, 100, CancellationToken.None);
            UpdateSnapshots();
            await Starting.InvokeAsync(this);
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
                orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder, symbols[fdkOrder.Symbol]));

            var fdkAssetsArray = connection.TradeProxy.Cache.AccountInfo.Assets;
            foreach (var fdkAsset in fdkAssetsArray)
                assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset));
        }

        public async Task Deinit()
        {
            await Stopping.InvokeAsync(this);

            await Task.Factory.StartNew(() =>
                {
                    uiUpdater.Complete();
                    uiUpdater.Completion.Wait();
                });
        }

        private OrderModel UpsertOrder(ExecutionReport report)
        {
            OrderModel order = new OrderModel(report, symbols[report.Symbol]);
            orders[order.Id] = order;
            return order;
        }

        private void UpsertPosition(Position report)
        {
            positions[report.Symbol] = new PositionModel(report);
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
                    if (orders.ContainsKey(report.OrderId))
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
                        else if (Type != AccountType.Gross)
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
            var orderCopy = orders[report.OrderId];
            orders.Remove(report.OrderId);
            ExecReportToAlgo(algoAction, OrderEntityAction.Removed, report, orderCopy);
            orderCopy.Dispose();
        }

        private void OnOrderUpdated(ExecutionReport report, OrderExecAction algoAction)
        {
            var order = UpsertOrder(report);
            ExecReportToAlgo(algoAction, OrderEntityAction.Updated, report, order);
        }

        private void UpdateAsset(AssetInfo assetInfo)
        {
            if (IsEmpty(assetInfo))
                assets.Remove(assetInfo.Currency);
            else
                assets[assetInfo.Currency] = new AssetModel(assetInfo);
        }

        void AccountInfoChanged(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        {
            Type = e.Information.Type;
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

        void IAccountInfoProvider.SyncInvoke(System.Action action)
        {
            Caliburn.Micro.Execute.OnUIThread(action);
        }

        List<OrderEntity> IAccountInfoProvider.GetOrders()
        {
            return Orders.Snapshot.Select(pair => pair.Value.ToAlgoOrder()).ToList();
        }

        IEnumerable<OrderEntity> IAccountInfoProvider.GetPosition()
        {
            throw new NotImplementedException();
        }

        IEnumerable<AssetEntity> IAccountInfoProvider.GetAssets()
        {
            return Assets.Snapshot.Select(pair => pair.Value.ToAlgoAsset()).ToList();
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

    internal class TradeHistoryProvider
    {
        private Logger _logger;
        private ConnectionModel _connectionModel;

        public event Action<TradeTransactionModel> OnTradeReport = delegate { };

        public TradeHistoryProvider(ConnectionModel connectionModel)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _connectionModel = connectionModel;

            _connectionModel.Connecting += () => { _connectionModel.TradeProxy.TradeTransactionReport += TradeTransactionReport; };
            _connectionModel.Disconnecting += () => { _connectionModel.TradeProxy.TradeTransactionReport -= TradeTransactionReport; };
        }

        public Task<TradeTransactionModel[]> DownloadHistoryAsync(DateTime from, DateTime to)
        {
            return DownloadHistoryAsync(from, to, CancellationToken.None);
        }
        public Task<TradeTransactionModel[]> DownloadHistoryAsync(DateTime from, DateTime to, CancellationToken token)
        {
            return DownloadHistoryAsync(from, to, CancellationToken.None, null);
        }
        public Task<TradeTransactionModel[]> DownloadHistoryAsync(DateTime from, DateTime to, CancellationToken token, IProgress<TradeTransactionModel> progress)
        {
            return StartDownloadingHistory(from, to, token, progress);
        }

        private Task<TradeTransactionModel[]> StartDownloadingHistory(DateTime from, DateTime to, CancellationToken token, IProgress<TradeTransactionModel> progress)
        {
            return Task.Run(() =>
            {
                try
                {
                    var tradesList = new List<TradeTransactionModel>();
                    token.ThrowIfCancellationRequested();

                    var historyStream = _connectionModel.TradeProxy.Server.GetTradeTransactionReports(TimeDirection.Forward, true, from, to);

                    while (!historyStream.EndOfStream)
                    {
                        token.ThrowIfCancellationRequested();

                        var historyItem = new TradeTransactionModel(historyStream.Item);
                        tradesList.Add(historyItem);
                        progress?.Report(historyItem);

                        historyStream.Next();
                    }

                    return tradesList.ToArray();
                }
                catch(OperationCanceledException) { throw; }
                catch(Exception ex) { _logger.Error(ex, "DownloadHistoryAsync FAILED"); throw; }
            }, token);

        }
        private void TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            OnTradeReport(new TradeTransactionModel(e.Report));
        }
    }
}
