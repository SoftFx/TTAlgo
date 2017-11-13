using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Api;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class AccountModel : CrossDomainObject, IAccountInfoProvider
    {
        private readonly DynamicDictionary<string, PositionModel> positions = new DynamicDictionary<string, PositionModel>();
        private readonly DynamicDictionary<string, AssetModel> assets = new DynamicDictionary<string, AssetModel>();
        private readonly DynamicDictionary<string, OrderModel> orders = new DynamicDictionary<string, OrderModel>();
        private AccountTypes? accType;
        private IOrderDependenciesResolver orderResolver;
        private IReadOnlyDictionary<string, CurrencyEntity> _currencies;
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

        public AccountTypes? Type
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

        public string Id { get; private set; }
        public double Balance { get; private set; }
        public string BalanceCurrency { get; private set; }
        public int BalanceDigits { get; private set; }
        public string Account { get; private set; }
        public int Leverage { get; private set; }
        public AccountCalculatorModel Calc { get; private set; }

        public void Init()
        {
            var cache = _client.Cache;
            var currencies = _client.Currencies;
            var accInfo = cache.AccountInfo.Value;
            var balanceCurrencyInfo = currencies.Snapshot.Read(accInfo.BalanceCurrency);
            var tradeRecords = cache.TradeRecords.Snapshot.Values;
            var positions = cache.Positions.Snapshot.Values;

            UpdateData(accInfo, currencies.Snapshot, _client.Symbols, tradeRecords, positions, accInfo.Assets);

            if (_isCalcEnabled)
            {
                Calc = AccountCalculatorModel.Create(this, _client);
                Calc.Recalculate();
            }
        }

        public void Deinit()
        {
            if (_isCalcEnabled && Calc != null)
            {
                Calc.Dispose();
                Calc = null;
            }
        }

        private void UpdateData(AccountEntity accInfo,
            IReadOnlyDictionary<string, CurrencyEntity> currencies,
            IOrderDependenciesResolver orderResolver,
            IEnumerable<OrderEntity> orders,
            IEnumerable<PositionEntity> positions,
            IEnumerable<AssetEntity> assets)
        {
            Id = accInfo.Id;

            _currencies = currencies;

            this.positions.Clear();
            this.orders.Clear();
            this.assets.Clear();

            this.orderResolver = orderResolver;

            var balanceCurrencyInfo = currencies.Read(accInfo.BalanceCurrency);

            Account = accInfo.Id;
            Type = accInfo.Type;
            Balance = accInfo.Balance;
            BalanceCurrency = accInfo.BalanceCurrency;
            Leverage = accInfo.Leverage;
            BalanceDigits = balanceCurrencyInfo?.Digits ?? 2;

            foreach (var fdkPosition in positions)
                this.positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition, orderResolver));

            foreach (var fdkOrder in orders)
                this.orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder, orderResolver));

            foreach (var fdkAsset in assets)
                this.assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset, currencies));
        }

        protected void OnBalanceChanged()
        {
            if (_isCalcEnabled)
                Calc.Recalculate();
        }

        private void OnBalanceOperation(BalanceOperationReport report)
        {
            if (Type == AccountTypes.Gross || Type == AccountTypes.Net)
            {
                Balance = report.Balance;
                OnBalanceChanged();
            }
            else if (Type == AccountTypes.Cash)
            {
                if (report.Balance > 0)
                {
                    assets[report.CurrencyCode] = new AssetModel(report.Balance, report.CurrencyCode, _currencies);
                }
                else
                {
                    if (assets.ContainsKey(report.CurrencyCode))
                    {
                        assets.Remove(report.CurrencyCode);
                    }
                }
            }

            AlgoEvent_BalanceUpdated(new BalanceOperationReport(report.Balance, report.CurrencyCode, report.Amount));
        }

        protected void OnReport(PositionEntity report)
        {
            if (report.IsEmpty)
                OnPositionRemoved(report);
            else if (!positions.ContainsKey(report.Symbol))
                OnPositionAdded(report);
            else
                OnPositionUpdated(report);
        }

        private void OnPositionUpdated(PositionEntity position)
        {
            var model = UpsertPosition(position);
            AlgoEvent_PositionUpdated(model.ToReport(OrderExecAction.Modified));
        }

        private void OnPositionAdded(PositionEntity position)
        {
            var model = UpsertPosition(position);
            AlgoEvent_PositionUpdated(model.ToReport(OrderExecAction.Opened));
        }

        private void OnPositionRemoved(PositionEntity position)
        {
            PositionModel model;

            if (!positions.TryGetValue(position.Symbol, out model))
                return;

            positions.Remove(model.Symbol);
            AlgoEvent_PositionUpdated(model.ToReport(OrderExecAction.Closed));
        }

        private PositionModel UpsertPosition(PositionEntity position)
        {
            var positionModel = new PositionModel(position, orderResolver);
            positions[position.Symbol] = positionModel;

            return positionModel;
        }

        private void OnReport(ExecutionReport report)
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

                case ExecutionType.Rejected:
                    OnOrderRejected(report, OrderExecAction.Rejected);
                    break;

                case ExecutionType.None:
                    if (report.OrderStatus == OrderStatus.Rejected)
                        OnOrderRejected(report, OrderExecAction.Rejected);
                    break;

                case ExecutionType.Trade:
                    if (report.OrderType == OrderType.StopLimit)
                    {
                        OnOrderRemoved(report, OrderExecAction.Activated);
                    }
                    else if (report.OrderType == OrderType.Limit || report.OrderType == OrderType.Stop)
                    {
                        if (report.LeavesVolume != 0)
                            OnOrderUpdated(report, OrderExecAction.Filled);
                        else if (Type != AccountTypes.Gross)
                            OnOrderRemoved(report, OrderExecAction.Filled);
                    }
                    else if (report.OrderType == OrderType.Position)
                    {
                        if (!double.IsNaN(report.Balance))
                            Balance = report.Balance;

                        if (report.LeavesVolume != 0)
                            OnOrderUpdated(report, OrderExecAction.Closed);
                        else
                            OnOrderRemoved(report, OrderExecAction.Closed);
                    }
                    else if (report.OrderType == OrderType.Market
                        && (Type == AccountTypes.Net || Type == AccountTypes.Cash))
                    {
                        OnMarketFilled(report, OrderExecAction.Filled);
                    }
                    break;
            }

            if (Type == AccountTypes.Net && report.ExecutionType == ExecutionType.Trade)
            {
                switch (report.OrderStatus)
                {
                    case OrderStatus.Calculated:
                    case OrderStatus.Filled:
                        if (!double.IsNaN(report.Balance))
                        {
                            Balance = report.Balance;
                            OnBalanceChanged();
                        }
                        break;
                }
            }

            if (Type == AccountTypes.Cash)
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

        private void UpdateAsset(AssetEntity assetInfo)
        {
            if (assetInfo.IsEmpty)
                assets.Remove(assetInfo.Currency);
            else
                assets[assetInfo.Currency] = new AssetModel(assetInfo, _currencies);
        }

        //void AccountInfoChanged(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        //{
        //    Type = e.Information.Type;
        //}

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
                algoReport.ResultCode = report.RejectReason;
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

        public AccountEntity AccountInfo
        {
            get
            {
                return new AccountEntity
                {
                    Id = Id,
                    Balance = Balance,
                    BalanceCurrency = BalanceCurrency,
                    Leverage = Leverage,
                    Type = Type.Value,
                    Assets = Assets.Snapshot.Values.Select(a => a.ToAlgoAsset()).ToArray()
                };
            }
        }

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
