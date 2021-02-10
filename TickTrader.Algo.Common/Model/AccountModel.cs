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
        private bool _isCalcStarted;
        private OrderUpdateAction _updateWatingForPosition = null;

        public AccountModel(IVarSet<string, CurrencyEntity> currecnies, IVarSet<string, SymbolModel> symbols)
        {
            _currencies = currecnies.Snapshot;
            _symbols = symbols.Snapshot;
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

            //_client.BalanceReceived += OnBalanceOperation;
            //_client.ExecutionReportReceived += OnReport;
            //_client.PositionReportReceived += OnReport;
        }

        internal void StartCalculator(IMarketDataProvider marketData)
        {
            if (!_isCalcStarted)
            {
                _isCalcStarted = true;

                Calc = AccountCalculatorModel.Create(this, marketData);
                Calc.Recalculate();
            }
        }

        internal void Init(AccountEntity accInfo, IEnumerable<OrderEntity> orders,
            IEnumerable<PositionEntity> positions, IEnumerable<AssetEntity> assets)
        {
            foreach(var pos in this.positions.Snapshot.Values)
                pos.Dispose();

            this.positions.Clear();
            this.orders.Clear();
            this.assets.Clear();

            var balanceCurrencyInfo = _currencies.Read(accInfo.BalanceCurrency);

            Id = accInfo.Id;
            Type = accInfo.Type;
            Balance = accInfo.Balance;
            BalanceCurrency = accInfo.BalanceCurrency;
            Leverage = accInfo.Leverage;
            BalanceDigits = balanceCurrencyInfo?.Digits ?? 2;

            foreach (var fdkPosition in positions)
                this.positions.Add(fdkPosition.Symbol, new PositionModel(fdkPosition, this));

            foreach (var fdkOrder in orders)
                this.orders.Add(fdkOrder.Id, new OrderModel(fdkOrder, this));

            foreach (var fdkAsset in assets)
                this.assets.Add(fdkAsset.Currency, new AssetModel(fdkAsset, _currencies));
        }

        public void Deinit()
        {
            //_client.BalanceReceived -= OnBalanceOperation;
            //_client.ExecutionReportReceived -= OnReport;
            //_client.PositionReportReceived -= OnReport;

            if (_isCalcStarted && Calc != null)
            {
                Calc.Dispose();
                Calc = null;
                _isCalcStarted = false;
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

        internal void Clear()
        {
            foreach (var position in positions.Snapshot.Values)
                position.Dispose();

            positions.Clear();
            orders.Clear();
            assets.Clear();

            Id = "";
            Type = null;
            Balance = 0;
            BalanceCurrency = null;
            Leverage = 0;
            BalanceDigits = 2;
        }

        #region Balance and assets management

        private void OnBalanceChanged()
        {
            if (_isCalcStarted)
                Calc.Recalculate();
        }

        internal EntityCacheUpdate GetBalanceUpdate(BalanceOperationReport report)
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
                        UpdateBalance(report.Balance);
                        break;
                }
            }

            if (Type == AccountTypes.Gross)
            {
                UpdateBalance(report.Balance);
            }

            if (Type == AccountTypes.Cash)
            {
                foreach (var asset in report.Assets)
                    UpdateAsset(asset);
            }
        }

        private void UpdateBalance(double balance)
        {
            if (double.IsNaN(balance))
                return;

            Balance = balance;
            OnBalanceChanged();
        }

        private void UpdateAsset(AssetEntity assetInfo)
        {
            if (assetInfo.IsEmpty)
                assets.Remove(assetInfo.Currency);
            else
                assets[assetInfo.Currency] = new AssetModel(assetInfo, _currencies);
        }

        public void UpdateBalance(double newBalance, string currency = null)
        {
            if (Type == AccountTypes.Gross || Type == AccountTypes.Net)
            {
                Balance = newBalance;
                OnBalanceChanged();
            }
            else if (Type == AccountTypes.Cash)
            {
                if (newBalance != 0)
                    assets[currency] = new AssetModel(newBalance, currency, _currencies);
                else
                {
                    if (assets.ContainsKey(currency))
                        assets.Remove(currency);
                }
            }
        }

        #endregion

        #region Postion management

        internal EntityCacheUpdate GetPositionUpdate(PositionEntity report)
        {
            System.Diagnostics.Debug.WriteLine("PR  #" + report.Symbol + " " + report.Side + " " + report.Volume + " p=" + report.Price);

            var update = DequeueWatingUpdate();
            _updateWatingForPosition = null;

            if (update == null)
                return GetPositionUpdateEntity(report, true);
            else
                update.Add(GetPositionUpdateEntity(report, false));

            return update;
        }

        private PositionUpdateAction GetPositionUpdateEntity(PositionEntity report, bool notify)
        {
            if (report.IsEmpty)
                return new PositionUpdateAction(report, OrderEntityAction.Removed, notify);
            else if (!positions.ContainsKey(report.Symbol))
                return new PositionUpdateAction(report, OrderEntityAction.Added, notify);
            else
                return new PositionUpdateAction(report, OrderEntityAction.Updated, notify);
        }

        private OrderUpdateAction DequeueWatingUpdate()
        {
            var update = _updateWatingForPosition;
            _updateWatingForPosition = null;
            return update;
        }

        public void UpdatePosition(PositionEntity position, bool notify)
        {
            var model = UpsertPosition(position);
            if (notify)
                PositionUpdate?.Invoke(model, position.Type);
        }

        public void OnPositionAdded(PositionEntity position, bool notify)
        {
            var model = UpsertPosition(position);
            if (notify)
                PositionUpdate?.Invoke(model, OrderExecAction.Opened);
        }

        public void RemovePosition(PositionEntity position, bool notify)
        {
            PositionModel model;

            if (!positions.TryGetValue(position.Symbol, out model))
                return;

            positions.Remove(model.Symbol);
            model.Dispose();
            if (notify)
                PositionUpdate?.Invoke(model, OrderExecAction.Closed);
        }

        private PositionModel UpsertPosition(PositionEntity position)
        {
            var isReplace = positions.TryGetValue(position.Symbol, out var oldModel);

            var positionModel = new PositionModel(position, this);
            positions[position.Symbol] = positionModel;

            if (isReplace)
                oldModel.Dispose();

            return positionModel;
        }

        #endregion

        #region Order management

        public void UpdateOrder(OrderExecAction execAction, OrderEntityAction entityAction, ExecutionReport report, PositionEntity netPosUpdate)
        {
            OrderModel order = null;

            if (entityAction == OrderEntityAction.Added)
            {
                order = new OrderModel(report, this);
                orders[order.Id] = order;
            }
            else if (entityAction == OrderEntityAction.Removed)
            {
                order = Orders.GetOrDefault(report.OrderId);
                orders.Remove(report.OrderId);
                order?.Update(report);
            }
            else if (entityAction == OrderEntityAction.Updated)
            {
                order = Orders.GetOrDefault(report.OrderId);

                if (order != null)
                {
                    bool replaceOrder = order.OrderType != report.OrderType; //crutch should be call before order Update

                    order.Update(report);

                    // workaround: dynamic collection filter can't react on field change
                    if (replaceOrder)
                        orders[order.Id] = order;
                }
            }
            else
                order = new OrderModel(report, this);

            OrderUpdate?.Invoke(new OrderUpdateInfo(report, execAction, entityAction, order, netPosUpdate));
        }

        public void UpdateOrder(OrderExecAction execAction, OrderEntityAction entityAction, OrderEntity report)
        {
            OrderModel order = null;

            if (entityAction == OrderEntityAction.Added)
            {
                order = new OrderModel(report, this);
                orders[order.Id] = order;
            }
            else if (entityAction == OrderEntityAction.Removed)
            {
                order = Orders.GetOrDefault(report.Id);
                orders.Remove(report.Id);
                order?.Update(report);
            }
            else if (entityAction == OrderEntityAction.Updated)
            {
                order = Orders.GetOrDefault(report.Id);

                bool typeChanged = order.OrderType != report.Type;

                order.Update(report);

                // workaround: dynamic collection filter can't react on field change
                if (typeChanged)
                    orders[order.Id] = order;
            }
            else
                order = new OrderModel(report, this);
        }

        internal EntityCacheUpdate GetOrderUpdate(ExecutionReport report)
        {
            System.Diagnostics.Debug.WriteLine($"ER({report.ExecutionType}, {report.OrderStatus})  #{report.OrderId} {report.OrderType} opId={report.TradeRequestId}");

            switch (report.ExecutionType)
            {
                case ExecutionType.New:
                    // Ignore
                    //if (report.OrderType == OrderType.Market)
                    //    return OnOrderOpened(report);
                    break;

                case ExecutionType.Calculated:
                    bool ignoreCalculate = (accType == AccountTypes.Gross && report.OrderType == OrderType.Market) || report.OrderStatus == OrderStatus.Executing;
                    if (!ignoreCalculate)
                    {
                        if (orders.TryGetValue(report.OrderId, out var order))
                        {
                            // ExecutionReport(Type=Calculated, Status=Calculated) is usually a transition from Executing state, which we currently ignore
                            // The only exception is fully filled pending orders on gross acc, which trigger position with same id
                            // StopLimit orders get new order id and opened as limit orders after activation
                            if ((order.OrderType == OrderType.Limit || order.OrderType == OrderType.Stop) &&  report.OrderType == OrderType.Position)
                                return OnOrderUpdated(report, OrderExecAction.Opened);
                            else break;
                        }
                        else
                            return OnOrderAdded(report, OrderExecAction.Opened);
                    }
                    else
                        break;

                case ExecutionType.Split:
                    return OnOrderUpdated(report, OrderExecAction.Splitted);

                case ExecutionType.Replace:
                    return OnOrderUpdated(report, OrderExecAction.Modified);

                case ExecutionType.Expired:
                    return OnOrderRemoved(report, OrderExecAction.Expired);

                case ExecutionType.Canceled:
                    return OnOrderCanceled(report, OrderExecAction.Canceled);

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
                        if (report.ImmediateOrCancel)
                            return OnIocFilled(report);

                        if (report.LeavesVolume != 0)
                            return OnOrderUpdated(report, OrderExecAction.Filled);

                        if (Type == AccountTypes.Gross)
                            return OnOrderUpdated(report, OrderExecAction.Filled);
                        else
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
                        return OnMarketFilled(report, OrderExecAction.Filled);
                    }
                    break;
            }

            return null;
        }

        private OrderUpdateAction OnOrderOpened(ExecutionReport report)
        {
            return new OrderUpdateAction(report, OrderExecAction.Opened, OrderEntityAction.None);
        }

        private OrderUpdateAction OnOrderAdded(ExecutionReport report, OrderExecAction algoAction)
        {
            if (report.OrderType == OrderType.Limit && report.ImmediateOrCancel)
                return new OrderUpdateAction(report, OrderExecAction.Opened, OrderEntityAction.None);

            var posUpdate = new OrderUpdateAction(report, algoAction, OrderEntityAction.Added);
            var orderUpdate = DequeueWatingUpdate();

            if (orderUpdate != null)
            {
                orderUpdate.Add(posUpdate);
                return orderUpdate;
            }
            else
                return posUpdate;
        }

        private OrderUpdateAction MockMarkedFilled(ExecutionReport report)
        {
            report.OrderType = OrderType.Position;
            report.LeavesVolume = report.InitialVolume.Value;
            return new OrderUpdateAction(report, OrderExecAction.Opened, OrderEntityAction.Added);
        }

        private OrderUpdateAction OnIocFilled(ExecutionReport report)
        {
            return JoinWithPosition(new OrderUpdateAction(report, OrderExecAction.Filled, OrderEntityAction.None));
        }

        private OrderUpdateAction OnMarketFilled(ExecutionReport report, OrderExecAction algoAction)
        {
            return JoinWithPosition(new OrderUpdateAction(report, algoAction, OrderEntityAction.None));
        }

        private OrderUpdateAction OnOrderRemoved(ExecutionReport report, OrderExecAction algoAction)
        {
            return JoinWithPosition(new OrderUpdateAction(report, algoAction, OrderEntityAction.Removed));
        }

        private OrderUpdateAction OnOrderUpdated(ExecutionReport report, OrderExecAction algoAction)
        {
            var orderUpdate = new OrderUpdateAction(report, algoAction, OrderEntityAction.Updated);

            // For gross stop/limit full fills: position opening is performed by updating old order, not adding new order
            if (report.OrderType == OrderType.Position && report.ExecutionType == ExecutionType.Calculated)
            {
                var waitingUpdate = DequeueWatingUpdate();
                if (waitingUpdate != null)
                {
                    waitingUpdate.Add(orderUpdate);
                    return waitingUpdate;
                }
            }

            return JoinWithPosition(orderUpdate);
        }

        private OrderUpdateAction OnOrderRejected(ExecutionReport report, OrderExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, OrderEntityAction.None);
        }

        private OrderUpdateAction OnOrderCanceled(ExecutionReport report, OrderExecAction algoAction)
        {
            // Limit Ioc don't get into order collection
            return new OrderUpdateAction(report, algoAction, (report.OrderType == OrderType.Limit && report.ImmediateOrCancel) ? OrderEntityAction.None : OrderEntityAction.Removed);
        }

        /// bread ration: position updates should be joined with exec reports to be atomic
        private OrderUpdateAction JoinWithPosition(OrderUpdateAction update)
        {
            if ((Type == AccountTypes.Gross || Type == AccountTypes.Net) && update.ExecAction == OrderExecAction.Filled)
            {
                _updateWatingForPosition = update;
                return null;
            }
            return update;
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
                _orders = orders ?? Enumerable.Empty<OrderEntity>();
                _positions = positions ?? Enumerable.Empty<PositionEntity>();
                _assets = assets ?? Enumerable.Empty<AssetEntity>();
            }

            public void Apply(EntityCache cache)
            {
                cache.Account.Init(_accInfo, _orders, _positions, _assets);
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
                acc.orders.Add(Order.Id, new OrderModel(Order, acc));
            }
        }

        [Serializable]
        private class OrderUpdateAction : EntityCacheUpdate
        {
            private ExecutionReport _report;
            private OrderExecAction _execAction;
            private OrderEntityAction _entityAction;
            private PositionUpdateAction _netPositionUpdate;
            private OrderUpdateAction _grossPositionUpdate;

            public OrderUpdateAction(ExecutionReport report, OrderExecAction execAction, OrderEntityAction entityAction)
            {
                _report = report;
                _execAction = execAction;
                _entityAction = entityAction;
            }

            public OrderExecAction ExecAction => _execAction;

            internal void Add(PositionUpdateAction position)
            {
                _netPositionUpdate = position;
            }

            internal void Add(OrderUpdateAction position)
            {
                _grossPositionUpdate = position;
            }

            public void Apply(EntityCache cache)
            {
                _netPositionUpdate?.Apply(cache);
                cache.Account.UpdateOrder(_execAction, _entityAction, _report, _netPositionUpdate?.Postion);
                cache.Account.UpdateBalance(_report);
                _grossPositionUpdate?.Apply(cache);
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
                acc.UpdateBalance(_report.Balance, _report.CurrencyCode);
                acc.BalanceUpdate?.Invoke(_report);
            }
        }

        [Serializable]
        private class PositionUpdateAction : EntityCacheUpdate
        {
            private PositionEntity _report;
            private OrderEntityAction _entityAction;
            private bool _notify;

            public PositionEntity Postion => _report;

            public PositionUpdateAction(PositionEntity report, OrderEntityAction action, bool notify)
            {
                _report = report;
                _entityAction = action;
                _notify = notify;
            }

            public void Apply(EntityCache cache)
            {
                if (_entityAction == OrderEntityAction.Added)
                    cache.Account.OnPositionAdded(_report, _notify);
                else if (_entityAction == OrderEntityAction.Updated)
                    cache.Account.UpdatePosition(_report, _notify);
                else if (_entityAction == OrderEntityAction.Removed)
                    cache.Account.RemovePosition(_report, _notify);
            }
        }
    }

    public class OrderUpdateInfo
    {
        public OrderUpdateInfo(ExecutionReport report, OrderExecAction execAction, OrderEntityAction entityAction, OrderModel order, PositionEntity position)
        {
            Report = report;
            ExecAction = execAction;
            EntityAction = entityAction;
            Order = order;
            Position = position;
        }

        public ExecutionReport Report { get; }
        public OrderExecAction ExecAction { get; }
        public OrderEntityAction EntityAction { get; }
        public OrderModel Order { get; }
        public PositionEntity Position { get; }
    }
}
