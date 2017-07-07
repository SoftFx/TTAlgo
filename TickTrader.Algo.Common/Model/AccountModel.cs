using Machinarium.State;
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
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Api;
using Machinarium.Qnil;
using System.Diagnostics;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class AccountModel : CrossDomainObject, IAccountInfoProvider
    {
        private readonly DynamicDictionary<string, PositionModel> positions = new DynamicDictionary<string, PositionModel>();
        private readonly DynamicDictionary<string, AssetModel> assets = new DynamicDictionary<string, AssetModel>();
        private readonly DynamicDictionary<string, OrderModel> orders = new DynamicDictionary<string, OrderModel>();
        private AccountType? accType;
        private IOrderDependenciesResolver orderResolver;
        private IDictionary<string, CurrencyInfo> _currencies;
        private ClientCore _client;
        private bool _isCalcEnabled;

        public AccountModel(ClientCore client, AccountModelOptions options)
        {
            _client = client;
            _isCalcEnabled = options.HasFlag(AccountModelOptions.EnableCalculator);

            _client.BalanceReceived += OnBalanceOperation;
            _client.ExecutionReportReceived += OnReport;
            _client.PositionReportReceived += OnReport;
        }

        public event System.Action AccountTypeChanged = delegate { };
        public IDynamicDictionarySource<string, PositionModel> Positions { get { return positions; } }
        public IDynamicDictionarySource<string, OrderModel> Orders { get { return orders; } }
        public IDynamicDictionarySource<string, AssetModel> Assets { get { return assets; } }

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
        public string BalanceCurrency { get; private set; }
        public int BalanceDigits { get; private set; }
        public string Account { get; private set; }
        public int Leverage { get; private set; }
        public AccountCalculatorModel Calc { get; private set; }

        public void Init()
        {
            var cache = _client.TradeProxy.Cache;
            var currencies = _client.Currencies;
            var accInfo = cache.AccountInfo;
            var balanceCurrencyInfo = currencies.GetOrDefault(accInfo.Currency);
            

            UpdateData(accInfo, currencies, _client.Symbols, cache.TradeRecords, cache.Positions, cache.AccountInfo.Assets);

            if (_isCalcEnabled)
            {
                Calc = AccountCalculatorModel.Create(this, _client);
                Calc.Recalculate();
            }
        }

        public void Deinit()
        {
            if (_isCalcEnabled)
                Calc.Dispose();
        }

        private void UpdateData(AccountInfo accInfo,
            IDictionary<string, CurrencyInfo> currencies,
            IOrderDependenciesResolver orderResolver,
            IEnumerable<TradeRecord> orders,
            IEnumerable<Position> positions,
            IEnumerable<AssetInfo> assets)
        {
            _currencies = currencies;

            this.positions.Clear();
            this.orders.Clear();
            this.assets.Clear();

            this.orderResolver = orderResolver;

            var balanceCurrencyInfo = currencies.GetOrDefault(accInfo.Currency);

            Account = accInfo.AccountId;
            Type = accInfo.Type;
            Balance = accInfo.Balance;
            BalanceCurrency = accInfo.Currency;
            Leverage = accInfo.Leverage;
            BalanceDigits = balanceCurrencyInfo?.Precision ?? 2;

            foreach (var fdkPosition in positions)
                this.positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition, orderResolver));

            foreach (var fdkOrder in orders)
                this.orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder, orderResolver));

            foreach (var fdkAsset in assets)
                this.assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset, currencies));
        }

        protected void OnBalanceChanged()
        {
        }

        private void OnBalanceOperation(SoftFX.Extended.Events.NotificationEventArgs<BalanceOperation> e)
        {
            if (Type == AccountType.Gross || Type == AccountType.Net)
            {
                Balance = e.Data.Balance;
                OnBalanceChanged();
            }
            else if (Type == AccountType.Cash)
            {
                if (e.Data.Balance > 0)
                {
                    assets[e.Data.TransactionCurrency] = new AssetModel(e.Data.Balance, e.Data.TransactionCurrency, _currencies);
                }
                else
                {
                    if (assets.ContainsKey(e.Data.TransactionCurrency))
                    {
                        assets.Remove(e.Data.TransactionCurrency);
                    }
                }
            }

            AlgoEvent_BalanceUpdated(new BalanceOperationReport(e.Data.Balance, e.Data.TransactionCurrency, e.Data.TransactionAmount));
        }

        protected void OnTransactionReport(TradeTransactionReport report)
        {
            // TODO: Remove after TTS 1.28 will be on live servers
            // Workaround. FDK does not provide balance changes in PositionReport
            if (Type == AccountType.Net)
            {
                if (report.TradeTransactionReportType == TradeTransactionReportType.OrderFilled)
                {
                    Balance = report.AccountBalance;
                    OnBalanceChanged();
                }
            }
        }

        protected void OnReport(SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            var report = e.Report;

            if (IsEmpty(report))
                OnPositionRemoved(report);
            else if (!positions.ContainsKey(report.Symbol))
                OnPositionAdded(report);
            else
                OnPositionUpdated(report);
        }

        private void OnPositionUpdated(Position position)
        {
            var model = UpsertPosition(position);
            AlgoEvent_PositionUpdated(model.ToReport(OrderExecAction.Modified));
        }

        private void OnPositionAdded(Position position)
        {
            var model = UpsertPosition(position);
            AlgoEvent_PositionUpdated(model.ToReport(OrderExecAction.Opened));
        }

        private void OnPositionRemoved(Position position)
        {
            PositionModel model;

            if (!positions.TryGetValue(position.Symbol, out model))
                return;

            positions.Remove(model.Symbol);
            AlgoEvent_PositionUpdated(model.ToReport(OrderExecAction.Closed));
        }

        private PositionModel UpsertPosition(Position position)
        {
            var positionModel = new PositionModel(position, orderResolver);
            positions[position.Symbol] = positionModel;

            return positionModel;
        }

        private void OnReport(SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
            var report = e.Report;

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

                case ExecutionType.Rejected:
                    OnOrderRejected(report, OrderExecAction.Rejected);
                    break;

                case ExecutionType.None:
                    if (report.OrderStatus == OrderStatus.Rejected)
                        OnOrderRejected(report, OrderExecAction.Rejected);
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
                        if (!double.IsNaN(report.Balance))
                            Balance = report.Balance;

                        if (report.LeavesVolume != 0)
                            OnOrderUpdated(report, OrderExecAction.Closed);
                        else
                            OnOrderRemoved(report, OrderExecAction.Closed);
                    }
                    else if (report.OrderType == TradeRecordType.Market 
                        && (Type == AccountType.Net || Type == AccountType.Cash))
                    {
                        OnMarketFilled(report, OrderExecAction.Filled);
                    }
                    break;
            }

            // TODO: Enable after TTS 1.28 will be on live servers
            //if (Type == AccountType.Net && report.ExecutionType == ExecutionType.Trade)
            //{
            //    switch (report.OrderStatus)
            //    {
            //        case OrderStatus.Calculated:
            //        case OrderStatus.Filled:
            //            if (!double.IsNaN(report.Balance))
            //            {
            //                Balance = report.Balance;
            //            }
            //            break;
            //    }
            //}

            if (Type == AccountType.Cash)
            {
                foreach (var asset in report.Assets)
                    UpdateAsset(asset);
            }
        }

        private OrderModel UpsertOrder(ExecutionReport report)
        {
            OrderModel order = new OrderModel(report, orderResolver);
            orders[order.Id] = order;
            return order;
        }

        private void OnOrderAdded(ExecutionReport report, OrderExecAction algoAction)
        {
            var order = UpsertOrder(report);
            ExecReportToAlgo(algoAction, OrderEntityAction.Added, report, order);
        }

        private void OnMarketFilled(ExecutionReport report, OrderExecAction algoAction)
        {
            var order = new OrderModel(report, orderResolver);
            ExecReportToAlgo(algoAction, OrderEntityAction.None, report, order);
        }

        private void OnOrderRemoved(ExecutionReport report, OrderExecAction algoAction)
        {
            orders.Remove(report.OrderId);
            var order = new OrderModel(report, orderResolver);
            ExecReportToAlgo(algoAction, OrderEntityAction.Removed, report, order);
        }

        private void OnOrderUpdated(ExecutionReport report, OrderExecAction algoAction)
        {
            var order = UpsertOrder(report);
            ExecReportToAlgo(algoAction, OrderEntityAction.Updated, report, order);
        }

        private void OnOrderRejected(ExecutionReport report, OrderExecAction algoAction)
        {
            ExecReportToAlgo(algoAction, OrderEntityAction.None, report);
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
            algoReport.OperationId = GetOperationId(report);
            algoReport.OrderId = report.OrderId;
            algoReport.ExecAction = action;
            algoReport.Action = entityAction;
            if (algoReport.ExecAction == OrderExecAction.Rejected)
                algoReport.ResultCode = FdkToAlgo.Convert(report.RejectReason, report.Text);
            if (!double.IsNaN(report.Balance))
                algoReport.NewBalance = report.Balance;
            if (report.Assets != null)
                algoReport.Assets = report.Assets.Select(assetInfo => new AssetModel(assetInfo, _currencies).ToAlgoAsset()).ToList();
            AlgoEvent_OrderUpdated(algoReport);
        }

        private string GetOperationId(ExecutionReport report)
        {
            if (!string.IsNullOrEmpty(report.ClosePositionRequestId))
                return report.ClosePositionRequestId;
            if (!string.IsNullOrEmpty(report.TradeRequestId))
                return report.TradeRequestId;
            return report.ClientOrderId;
        }

        AccountTypes IAccountInfoProvider.AccountType { get { return FdkToAlgo.Convert(Type.Value); } }

        void IAccountInfoProvider.SyncInvoke(Action syncAction)
        {
            _client.TradeSync.Invoke(syncAction);
        }

        List<OrderEntity> IAccountInfoProvider.GetOrders()
        {
            return Orders.Snapshot.Select(pair => pair.Value.ToAlgoOrder()).ToList();
        }

        IEnumerable<PositionExecReport> IAccountInfoProvider.GetPositions()
        {
            return Positions.Snapshot.Select(pair => pair.Value.ToReport(OrderExecAction.Opened)).ToList();
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

    [Flags]
    public enum AccountModelOptions
    {
        None = 0,
        EnableCalculator = 1
    }
}
