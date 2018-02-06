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
    public class AccountModel : CrossDomainObject, IOrderDependenciesResolver
    {
        private readonly VarDictionary<string, PositionModel> positions = new VarDictionary<string, PositionModel>();
        private readonly VarDictionary<string, AssetModel> assets = new VarDictionary<string, AssetModel>();
        private readonly VarDictionary<string, OrderModel> orders = new VarDictionary<string, OrderModel>();
        private AccountTypes? accType;
        private readonly IReadOnlyDictionary<string, CurrencyEntity> _currencies;
        private readonly IReadOnlyDictionary<string, SymbolModel> _symbols;
        private bool _isCalcEnabled;

        public AccountModel(IVarSet<string, CurrencyEntity> currecnies, IVarSet<string, SymbolModel> symbols, AccountModelOptions options)
        {
            _currencies = currecnies.Snapshot;
            _symbols = symbols.Snapshot;
            _isCalcEnabled = options.HasFlag(AccountModelOptions.EnableCalculator);
        }

        public event System.Action AccountTypeChanged = delegate { };
        public IVarSet<string, PositionModel> Positions { get { return positions; } }
        public IVarSet<string, OrderModel> Orders { get { return orders; } }
        public IVarSet<string, AssetModel> Assets { get { return assets; } }

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

        public event Action<OrderUpdateInfo> OrderUpdate;
        public event Action<PositionModel, OrderExecAction> PositionUpdate;
        public event Action<BalanceOperationReport> BalanceUpdate;

        public EntityCacheUpdate CreateSnaphotUpdate(AccountEntity accInfo, List<OrderEntity> tradeRecords, List<PositionEntity> positions, List<AssetEntity> assets)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Init() symbols:{0} orders:{1} positions:{2}",
                _symbols.Count, tradeRecords.Count, positions.Count));

            return new Snapshot(accInfo, tradeRecords, positions, assets);

            //UpdateData(accInfo, tradeRecords, positions, accInfo.Assets);

            //if (_isCalcEnabled)
            //{
            //    Calc = AccountCalculatorModel.Create(this, _client);
            //    Calc.Recalculate();
            //}

            //_client.BalanceReceived += OnBalanceOperation;
            //_client.ExecutionReportReceived += OnReport;
            //_client.PositionReportReceived += OnReport;
        }

        public void Deinit()
        {
            //_client.BalanceReceived -= OnBalanceOperation;
            //_client.ExecutionReportReceived -= OnReport;
            //_client.PositionReportReceived -= OnReport;

            if (_isCalcEnabled && Calc != null)
            {
                Calc.Dispose();
                Calc = null;
            }
        }

        public AccountEntity GetAccountInfo()
        {
            return new AccountEntity
            {
                Id = Id,
                Balance = Balance,
                BalanceCurrency = BalanceCurrency,
                Leverage = Leverage,
                Type = Type ?? AccountTypes.Gross,
                Assets = Assets.Snapshot.Values.Select(a => a.GetEntity()).ToArray()
            };
        }

        //internal void UpdateData(AccountEntity accInfo,
        //    IEnumerable<OrderEntity> orders,
        //    IEnumerable<PositionEntity> positions,
        //    IEnumerable<AssetEntity> assets)
        //{
        //    Id = accInfo.Id;

        //    this.positions.Clear();
        //    this.orders.Clear();
        //    this.assets.Clear();

        //    var balanceCurrencyInfo = _currencies.Read(accInfo.BalanceCurrency);

        //    Account = accInfo.Id;
        //    Type = accInfo.Type;
        //    Balance = accInfo.Balance;
        //    BalanceCurrency = accInfo.BalanceCurrency;
        //    Leverage = accInfo.Leverage;
        //    BalanceDigits = balanceCurrencyInfo?.Digits ?? 2;

        //    foreach (var fdkPosition in positions)
        //        this.positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition, this));

        //    foreach (var fdkOrder in orders)
        //        this.orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder, this));

        //    foreach (var fdkAsset in assets)
        //        this.assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset, _currencies));
        //}

        #region Balance and assets management

        private void OnBalanceChanged()
        {
            if (_isCalcEnabled)
                Calc.Recalculate();
        }

        internal EntityCacheUpdate OnBalanceOperation(BalanceOperationReport report)
        {
            return new BalanceUpdateAction(report);
        }

        private void UpdateBalance(ExecutionReport report)
        {
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

        private void UpdateAsset(AssetEntity assetInfo)
        {
            if (assetInfo.IsEmpty)
                assets.Remove(assetInfo.Currency);
            else
                assets[assetInfo.Currency] = new AssetModel(assetInfo, _currencies);
        }

        #endregion

        #region Postion management

        internal EntityCacheUpdate OnReport(PositionEntity report)
        {
            if (report.IsEmpty)
                return new PositionUpdateAction(report, OrderEntityAction.Removed);
            else if (!positions.ContainsKey(report.Symbol))
                return new PositionUpdateAction(report, OrderEntityAction.Added);
            else
                return new PositionUpdateAction(report, OrderEntityAction.Updated);
        }

        private void OnPositionUpdated(PositionEntity position)
        {
            var model = UpsertPosition(position);
            PositionUpdate?.Invoke(model, OrderExecAction.Modified);
        }

        private void OnPositionAdded(PositionEntity position)
        {
            var model = UpsertPosition(position);
            PositionUpdate?.Invoke(model, OrderExecAction.Opened);
        }

        private void OnPositionRemoved(PositionEntity position)
        {
            PositionModel model;

            if (!positions.TryGetValue(position.Symbol, out model))
                return;

            positions.Remove(model.Symbol);
            PositionUpdate?.Invoke(model, OrderExecAction.Closed);
        }

        private PositionModel UpsertPosition(PositionEntity position)
        {
            var positionModel = new PositionModel(position, this);
            positions[position.Symbol] = positionModel;

            return positionModel;
        }

        #endregion

        #region Order management

        internal EntityCacheUpdate GetOrderUpdate(ExecutionReport report)
        {
            switch (report.ExecutionType)
            {
                case ExecutionType.Calculated:
                    if (orders.ContainsKey(report.OrderId))
                        return OnOrderUpdated(report, OrderExecAction.Opened);
                    else
                        return OnOrderAdded(report, OrderExecAction.Opened);

                case ExecutionType.Replace:
                    return OnOrderUpdated(report, OrderExecAction.Modified);

                case ExecutionType.Expired:
                    return OnOrderRemoved(report, OrderExecAction.Expired);

                case ExecutionType.Canceled:
                    return OnOrderRemoved(report, OrderExecAction.Canceled);

                case ExecutionType.Rejected:
                    return OnOrderRejected(report, OrderExecAction.Rejected);

                case ExecutionType.None:
                    if (report.OrderStatus == OrderStatus.Rejected)
                        return OnOrderRejected(report, OrderExecAction.Rejected);
                    break;

                case ExecutionType.Trade:
                    if (report.OrderType == OrderType.StopLimit)
                    {
                        return OnOrderRemoved(report, OrderExecAction.Activated);
                    }
                    else if (report.OrderType == OrderType.Limit || report.OrderType == OrderType.Stop)
                    {
                        if (report.LeavesVolume != 0)
                            return OnOrderUpdated(report, OrderExecAction.Filled);
                        else if (Type != AccountTypes.Gross)
                            return OnOrderRemoved(report, OrderExecAction.Filled);
                    }
                    else if (report.OrderType == OrderType.Position)
                    {
                        if (report.OrderStatus == OrderStatus.PartiallyFilled)
                            return OnOrderUpdated(report, OrderExecAction.Closed);
                        if (report.OrderStatus == OrderStatus.Filled)
                            return OnOrderRemoved(report, OrderExecAction.Closed);
                    }
                    else if (report.OrderType == OrderType.Market)
                    {
                        if (Type == AccountTypes.Gross)
                            return MockMarkedFilled(report);
                        else if (Type == AccountTypes.Net || Type == AccountTypes.Cash)
                            return OnMarketFilled(report, OrderExecAction.Filled);
                    }
                    break;
            }

            return null;
        }

        private OrderUpdateAction OnOrderAdded(ExecutionReport report, OrderExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, OrderEntityAction.Added);

            //var order = UpsertOrder(report);
            //ExecReportToAlgo(algoAction, OrderEntityAction.Added, report, order);
            //OrderUpdate?.Invoke(report, order, algoAction);
        }

        private OrderUpdateAction MockMarkedFilled(ExecutionReport report)
        {
            report.OrderType = OrderType.Position;
            return new OrderUpdateAction(report, OrderExecAction.Opened, OrderEntityAction.Updated);

            //var order = new OrderModel(report, this);
            //order.OrderType = OrderType.Position;
            //order.RemainingAmount = order.Amount;
            //ExecReportToAlgo(OrderExecAction.Opened, OrderEntityAction.Added, report, order);
            //OrderUpdate?.Invoke(report, order, OrderExecAction.Opened);
        }

        private OrderUpdateAction OnMarketFilled(ExecutionReport report, OrderExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, OrderEntityAction.None);
            //var order = new OrderModel(report, this);
            //ExecReportToAlgo(algoAction, OrderEntityAction.None, report, order);
            //OrderUpdate?.Invoke(report, order, algoAction);
        }

        private OrderUpdateAction OnOrderRemoved(ExecutionReport report, OrderExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, OrderEntityAction.Removed);
            //orders.Remove(report.OrderId);
            //var order = new OrderModel(report, this);
            //ExecReportToAlgo(algoAction, OrderEntityAction.Removed, report, order);
            //OrderUpdate?.Invoke(report, order, algoAction);
        }

        private OrderUpdateAction OnOrderUpdated(ExecutionReport report, OrderExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, OrderEntityAction.Updated);
            //var order = UpsertOrder(report);
            //ExecReportToAlgo(algoAction, OrderEntityAction.Updated, report, order);
            //OrderUpdate?.Invoke(report, order, algoAction);
        }

        private OrderUpdateAction OnOrderRejected(ExecutionReport report, OrderExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, OrderEntityAction.None);
            //ExecReportToAlgo(algoAction, OrderEntityAction.None, report);
            //OrderUpdate?.Invoke(report, null, algoAction);
        }

        #endregion

        SymbolModel IOrderDependenciesResolver.GetSymbolOrNull(string name)
        {
            return _symbols.GetOrDefault(name);
        }

        public EntityCacheUpdate GetSnapshotUpdate()
        {
            var info = GetAccountInfo();
            var orders = Orders.Snapshot.Values.Select(o => o.GetEntity()).ToList();
            var positions = Positions.Snapshot.Values.Select(p => p.GetEntity()).ToList();
            var assets = Assets.Snapshot.Values.Select(a => a.GetEntity()).ToList();

            return new Snapshot(info, orders, positions, assets);
        }

        [Serializable]
        public class Snapshot : EntityCacheUpdate
        {
            private AccountEntity _accInfo;
            private IEnumerable<OrderEntity> _orders;
            private IEnumerable<PositionEntity> _positions;
            private IEnumerable<AssetEntity> _assets;

            public Snapshot(AccountEntity accInfo, IEnumerable<OrderEntity> orders,
                IEnumerable<PositionEntity> positions, IEnumerable<AssetEntity> assets)
            {
                _accInfo = accInfo;
                _orders = orders;
                _positions = positions;
                _assets = assets;
            }

            public void Apply(EntityCache cache)
            {
                var acc = cache.Account;

                acc.positions.Clear();
                acc.orders.Clear();
                acc.assets.Clear();

                var balanceCurrencyInfo = acc._currencies.Read(_accInfo.BalanceCurrency);

                acc.Account = _accInfo.Id;
                acc.Type = _accInfo.Type;
                acc.Balance = _accInfo.Balance;
                acc.BalanceCurrency = _accInfo.BalanceCurrency;
                acc.Leverage = _accInfo.Leverage;
                acc.BalanceDigits = balanceCurrencyInfo?.Digits ?? 2;

                foreach (var fdkPosition in _positions)
                    acc.positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition, acc));

                foreach (var fdkOrder in _orders)
                    acc.orders.Add(fdkOrder.OrderId, new OrderModel(fdkOrder, acc));

                foreach (var fdkAsset in _assets)
                    acc.assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset, acc._currencies));
            }
        }

        [Serializable]
        public class LoadOrderUpdate : EntityCacheUpdate
        {
            public LoadOrderUpdate(OrderEntity order)
            {
                Order = order ?? throw new ArgumentNullException("symbol");
            }

            private OrderEntity Order { get; }

            public void Apply(EntityCache cache)
            {
                var acc = cache.Account;
                acc.orders.Add(Order.OrderId, new OrderModel(Order, acc));
            }
        }

        [Serializable]
        private class ClearDataAction : EntityCacheUpdate
        {
            public void Apply(EntityCache cache)
            {
            }
        }

        [Serializable]
        private class OrderUpdateAction: EntityCacheUpdate
        {
            private ExecutionReport _report;
            private OrderExecAction _execAction;
            private OrderEntityAction _entityAction;

            public OrderUpdateAction(ExecutionReport report, OrderExecAction execAction, OrderEntityAction entityAction)
            {
                _report = report;
                _execAction = execAction;
                _entityAction = entityAction;
            }

            public void Apply(EntityCache cache)
            {
                if (_entityAction == OrderEntityAction.Added)
                {
                    var order = new OrderModel(_report, cache.Account);
                    cache.Account.orders[order.Id] = order;
                }
                else if (_entityAction == OrderEntityAction.Removed)
                    cache.Account.orders.Remove(_report.OrderId);

                cache.Account.OrderUpdate?.Invoke(new OrderUpdateInfo(_report, _execAction, _entityAction));
                cache.Account.UpdateBalance(_report);
            }
        }

        [Serializable]
        private class BalanceUpdateAction : EntityCacheUpdate
        {
            private BalanceOperationReport _report;

            public BalanceUpdateAction(BalanceOperationReport report)
            {
                _report = report;
            }

            public void Apply(EntityCache cache)
            {
                var acc = cache.Account;

                if (acc.Type == AccountTypes.Gross || acc.Type == AccountTypes.Net)
                {
                    acc.Balance = _report.Balance;
                    acc.OnBalanceChanged();
                }
                else if (acc.Type == AccountTypes.Cash)
                {
                    if (_report.Balance > 0)
                        acc.assets[_report.CurrencyCode] = new AssetModel(_report.Balance, _report.CurrencyCode, acc._currencies);
                    else
                    {
                        if (acc.assets.ContainsKey(_report.CurrencyCode))
                            acc.assets.Remove(_report.CurrencyCode);
                    }
                }

                acc.BalanceUpdate?.Invoke(_report);
            }
        }

        [Serializable]
        private class PositionUpdateAction : EntityCacheUpdate
        {
            private PositionEntity _report;
            private OrderEntityAction _entityAction;

            public PositionUpdateAction(PositionEntity report, OrderEntityAction action)
            {
                _report = report;
                _entityAction = action;
            }

            public void Apply(EntityCache cache)
            {
                if (_entityAction == OrderEntityAction.Added)
                    cache.Account.OnPositionAdded(_report);
                else if (_entityAction == OrderEntityAction.Updated)
                    cache.Account.OnPositionUpdated(_report);
                else if (_entityAction == OrderEntityAction.Removed)
                    cache.Account.OnPositionRemoved(_report);
            }
        }
    }

    public class OrderUpdateInfo
    {
        public OrderUpdateInfo(ExecutionReport report, OrderExecAction execAction, OrderEntityAction entityAction)
        {
            Report = report;
            ExecAction = execAction;
            EntityAction = entityAction;
            //Order = order;
        }

        public ExecutionReport Report { get; }
        public OrderExecAction ExecAction { get; }
        public OrderEntityAction EntityAction { get; }
        public OrderModel Order { get; }
    }

    [Flags]
    public enum AccountModelOptions
    {
        None = 0,
        EnableCalculator = 1
    }
}
