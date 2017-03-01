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
using System.Diagnostics;

namespace TickTrader.BotTerminal
{
    internal class AccountModel : CrossDomainObject, IAccountInfoProvider
    {
        private Logger logger;
        private DynamicDictionary<string, PositionModel> positions = new DynamicDictionary<string, PositionModel>();
        private DynamicDictionary<string, AssetModel> assets = new DynamicDictionary<string, AssetModel>();
        private DynamicDictionary<string, OrderModel> orders = new DynamicDictionary<string, OrderModel>();
        private TraderClientModel clientModel;
        private ConnectionModel connection;
        private ActionBlock<System.Action> uiUpdater;
        private AccountType? accType;
        private IDictionary<string, CurrencyInfo> _currencies;

        public AccountModel(TraderClientModel clientModel)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            this.clientModel = clientModel;
            this.connection = clientModel.Connection;
            TradeHistory = new TradeHistoryProvider(clientModel);

            clientModel.IsConnectingChanged += UpdateConnectingState;

            connection.Connecting += () =>
            {
                connection.TradeProxy.AccountInfo += AccountInfoChanged;
                connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            connection.Disconnecting += () =>
            {
                connection.TradeProxy.AccountInfo -= AccountInfoChanged;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation -= TradeProxy_BalanceOperation;
            };
        }

        private void UpdateConnectingState()
        {
            if (clientModel.IsConnecting)
            {
                positions.Clear();
                orders.Clear();
                assets.Clear();
            }
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
        public AccountCalculatorModel Calc { get; private set; }

        public double Balance { get; private set; }
        public string BalanceCurrency { get; private set; }
        public int BalanceDigits { get; private set; }
        public string Account { get; private set; }
        public int Leverage { get; private set; }

        public void Init(IDictionary<string, CurrencyInfo> currencies)
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

            try
            {
                this.uiUpdater = DataflowHelper.CreateUiActionBlock<System.Action>(uiActionhandler, 100, 100, CancellationToken.None);
                UpdateData(currencies);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Init() failed.");
            }
        }

        public void UpdateData(IDictionary<string, CurrencyInfo> currencies)
        {
            var accInfo = connection.TradeProxy.Cache.AccountInfo;
            var balanceCurrencyInfo = currencies.GetOrDefault(accInfo.Currency);
            _currencies = currencies;

            Account = accInfo.AccountId;
            Type = accInfo.Type;
            Balance = accInfo.Balance;
            BalanceCurrency = accInfo.Currency;
            Leverage = accInfo.Leverage;
            BalanceDigits = balanceCurrencyInfo?.Precision ?? 2;

            var fdkPositionsArray = connection.TradeProxy.Cache.Positions;
            foreach (var fdkPosition in fdkPositionsArray)
                positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition));

            var fdkOrdersArray = connection.TradeProxy.Cache.TradeRecords;
            foreach (var fdkOrder in fdkOrdersArray)
                orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder, clientModel.Symbols));

            var fdkAssetsArray = connection.TradeProxy.Cache.AccountInfo.Assets;
            foreach (var fdkAsset in fdkAssetsArray)
                assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset, _currencies));

            Calc = AccountCalculatorModel.Create(this, clientModel);
            Calc.Recalculate();
        }

        public async Task Deinit()
        {
            Calc.Dispose();

            await Task.Factory.StartNew(() =>
                {
                    uiUpdater.Complete();
                    uiUpdater.Completion.Wait();
                });
        }

        private void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            // TO DO: save updates in a buffer and apply them on Init()
            var uiUpdaterCopy = this.uiUpdater;
            if (uiUpdaterCopy == null)
                return;

            uiUpdater.SendAsync(() => ApplyReport(e.Report));
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
                {
                    Balance = e.Data.Balance;
                    // TO DO : Calc should listen to BalanceUpdated event and recalculate iteself
                    Calc.Recalculate();
                }
                else if (Type == AccountType.Cash)
                    assets[e.Data.TransactionCurrency] = new AssetModel(e.Data.Balance, e.Data.TransactionCurrency, _currencies);

                AlgoEvent_BalanceUpdated(new BalanceOperationReport(e.Data.Balance, e.Data.TransactionCurrency));
            });
        }

        private void TradeProxy_TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            var a = e.Report;
        }

        private void ApplyReport(Position report)
        {
            if (IsEmpty(report))
                OnPositionRemoved(report);
            else if (!positions.ContainsKey(report.Symbol))
                OnPositionAdded(report);
            else
                OnPositionUpdated(report);
        }

        private void OnPositionUpdated(Position report)
        {
            var position = UpsertPosition(report);
            AlgoEvent_PositionUpdated(new PositionExecReport(OrderExecAction.Modified, position.ToAlgoPosition()));
        }

        private void OnPositionAdded(Position report)
        {
            var position = UpsertPosition(report);
            AlgoEvent_PositionUpdated(new PositionExecReport(OrderExecAction.Opened, position.ToAlgoPosition()));
        }

        private void OnPositionRemoved(Position report)
        {
            PositionModel position;

            if (!positions.TryGetValue(report.Symbol, out position))
                return;

            positions.Remove(report.Symbol);
            AlgoEvent_PositionUpdated(new PositionExecReport(OrderExecAction.Closed, position.ToAlgoPosition()));
        }

        private PositionModel UpsertPosition(Position position)
        {
            var positionModel = new PositionModel(position);
            positions[position.Symbol] = positionModel;

            return positionModel;
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
                        Balance = report.Balance;

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

        private OrderModel UpsertOrder(ExecutionReport report)
        {
            OrderModel order = new OrderModel(report, clientModel.Symbols);
            orders[order.Id] = order;
            return order;
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
                assets[assetInfo.Currency] = new AssetModel(assetInfo, _currencies);
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
            if (report.Assets != null)
                algoReport.Assets = report.Assets.Select(assetInfo => new AssetModel(assetInfo, _currencies).ToAlgoAsset()).ToList();
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
}
