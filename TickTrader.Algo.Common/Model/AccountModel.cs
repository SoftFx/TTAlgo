using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Api;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class AccountModel : CrossDomainObject, IOrderDependenciesResolver, IMarginAccountInfo2, ICashAccountInfo2
    {
        private readonly VarDictionary<string, PositionModel> _positions = new VarDictionary<string, PositionModel>();
        private readonly VarDictionary<string, AssetModel> _assets = new VarDictionary<string, AssetModel>();
        private readonly VarDictionary<string, OrderModel> _orders = new VarDictionary<string, OrderModel>();
        private Domain.AccountInfo.Types.Type? _accType;
        private readonly IReadOnlyDictionary<string, CurrencyEntity> _currencies;
        private readonly IReadOnlyDictionary<string, SymbolModel> _symbols;
        private bool _isCalcStarted;
        private OrderUpdateAction _updateWatingForPosition = null;

        public AlgoMarketState Market { get; }

        public CashAccountCalculator CashCalculator { get; private set; }

        public MarginAccountCalculator MarginCalculator { get; private set; }

        public AccountModel(IVarSet<string, CurrencyEntity> currecnies, IVarSet<string, SymbolModel> symbols)
        {
            _currencies = currecnies.Snapshot;
            _symbols = symbols.Snapshot;

            Market = new AlgoMarketState();
        }

        public event System.Action AccountTypeChanged = delegate { };
        public IVarSet<string, PositionModel> Positions => _positions;
        public IVarSet<string, OrderModel> Orders => _orders;
        public IVarSet<string, AssetModel> Assets => _assets;

        public Domain.AccountInfo.Types.Type? Type
        {
            get { return _accType; }
            private set
            {
                if (_accType != value)
                {
                    _accType = value;
                    AccountTypeChanged();
                }
            }
        }

        public string Id { get; private set; }
        public double Balance { get; private set; }
        public string BalanceCurrency { get; private set; }
        public int BalanceDigits { get; private set; }
        public int Leverage { get; private set; }

        long IAccountInfo2.Id => 0;

        public AccountInfo.Types.Type AccountingType => Type ?? AccountInfo.Types.Type.Gross;


        IEnumerable<IPositionModel2> IMarginAccountInfo2.Positions => Positions.Snapshot.Values;

        IEnumerable<IOrderModel2> IAccountInfo2.Orders => Orders.Snapshot.Values;

        IEnumerable<IAssetModel2> ICashAccountInfo2.Assets => Assets.Snapshot.Values;

        //public AccountCalculatorModel Calc { get; private set; }

        public event Action<OrderUpdateInfo> OrderUpdate;
        public event Action<PositionModel, Domain.OrderExecReport.Types.ExecAction> PositionUpdate;
        public event Action<Domain.BalanceOperation> BalanceUpdate;

        public event Action<IPositionModel2> PositionChanged;
        public event Action<IOrderModel2> OrderAdded;
        public event Action<IEnumerable<IOrderModel2>> OrdersAdded;
        public event Action<IOrderModel2> OrderRemoved;
        public event Action<IAssetModel2, AssetChangeType> AssetsChanged;

        public EntityCacheUpdate CreateSnaphotUpdate(Domain.AccountInfo accInfo, List<Domain.OrderInfo> tradeRecords, List<Domain.PositionInfo> positions, List<Domain.AssetInfo> assets)
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
            Market.Init(marketData.Symbols.Snapshot.Values, marketData.Currencies.Snapshot.Values);

            if (!_isCalcStarted)
            {
                _isCalcStarted = true;

                //Calc = AccountCalculatorModel.Create(this, marketData);
                //Calc.Recalculate();

                switch (Type)
                {
                    case AccountInfo.Types.Type.Cash:
                        CashCalculator = new CashAccountCalculator(this, Market);
                        break;
                    case AccountInfo.Types.Type.Net:
                    case AccountInfo.Types.Type.Gross:
                        MarginCalculator = new MarginAccountCalculator(this, Market, true);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported account type: {Type}");
                }
            }
        }

        internal void Init(Domain.AccountInfo accInfo, IEnumerable<Domain.OrderInfo> orders,
            IEnumerable<Domain.PositionInfo> positions, IEnumerable<Domain.AssetInfo> assets)
        {
            _positions.Clear();
            _orders.Clear();
            _assets.Clear();

            var balanceCurrencyInfo = _currencies.Read(accInfo.BalanceCurrency);

            Id = accInfo.Id;
            Type = accInfo.Type;
            Balance = accInfo.Balance;
            BalanceCurrency = accInfo.BalanceCurrency;
            Leverage = accInfo.Leverage;
            BalanceDigits = balanceCurrencyInfo?.Digits ?? 2;

            foreach (var fdkPosition in positions)
            {
                var model = new PositionModel(fdkPosition, this);
                this._positions.Add(fdkPosition.Symbol, model);
                PositionChanged?.Invoke(model);
            }

            foreach (var fdkOrder in orders)
                this._orders.Add(fdkOrder.Id, new OrderModel(fdkOrder, this));

            OrdersAdded?.Invoke(orders.Cast<IOrderModel2>());

            foreach (var fdkAsset in assets)
            {
                var model = new AssetModel(fdkAsset, _currencies);
                this._assets.Add(fdkAsset.Currency, model);

                AssetsChanged?.Invoke(model, AssetChangeType.Added);
            }
        }

        public void Deinit()
        {
            //_client.BalanceReceived -= OnBalanceOperation;
            //_client.ExecutionReportReceived -= OnReport;
            //_client.PositionReportReceived -= OnReport;

            if (_isCalcStarted)
            {
                CashCalculator?.Dispose();
                MarginCalculator?.Dispose();

                CashCalculator = null;
                MarginCalculator = null;
                _isCalcStarted = false;
            }
        }

        public Domain.AccountInfo GetAccountInfo()
        {
            return new Domain.AccountInfo(_accType != Domain.AccountInfo.Types.Type.Cash ? Balance : (double?)null, BalanceCurrency,
                Assets.Snapshot.Values.Select(a => a.GetInfo()).ToArray())
            {
                Id = Id,
                Leverage = Leverage,
                Type = Type ?? Domain.AccountInfo.Types.Type.Gross,
            };
        }

        internal void Clear()
        {
            _positions.Clear();
            _orders.Clear();
            _assets.Clear();

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
            //if (_isCalcStarted)
            //    Calc.Recalculate();
        }

        internal EntityCacheUpdate GetBalanceUpdate(Domain.BalanceOperation report)
        {
            return new BalanceUpdateAction(report);
        }

        private void UpdateBalance(ExecutionReport report)
        {
            if (Type == Domain.AccountInfo.Types.Type.Net && report.ExecutionType == ExecutionType.Trade)
            {
                switch (report.OrderStatus)
                {
                    case OrderStatus.Calculated:
                    case OrderStatus.Filled:
                        UpdateBalance(report.Balance);
                        break;
                }
            }

            if (Type == Domain.AccountInfo.Types.Type.Gross)
            {
                UpdateBalance(report.Balance);
            }

            if (Type == Domain.AccountInfo.Types.Type.Cash)
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

        private void UpdateAsset(Domain.AssetInfo assetInfo)
        {
            var model = new AssetModel(assetInfo, _currencies);

            if (assetInfo.Balance == 0)
                _assets.Remove(assetInfo.Currency);
            else
                _assets[assetInfo.Currency] = model;

            AssetsChanged?.Invoke(model, assetInfo.Balance == 0 ? AssetChangeType.Removed : AssetChangeType.Updated);
        }

        public void UpdateBalance(double newBalance, string currency = null)
        {
            if (Type == Domain.AccountInfo.Types.Type.Gross || Type == Domain.AccountInfo.Types.Type.Net)
            {
                Balance = newBalance;
                OnBalanceChanged();
            }
            else if (Type == Domain.AccountInfo.Types.Type.Cash)
            {
                var model = new AssetModel(newBalance, currency, _currencies);

                if (newBalance != 0)
                    _assets[currency] = model;
                else
                {
                    if (_assets.ContainsKey(currency))
                        _assets.Remove(currency);
                }

                AssetsChanged?.Invoke(model, newBalance == 0 ? AssetChangeType.Removed : AssetChangeType.Updated);
            }
        }

        #endregion

        #region Postion management

        internal EntityCacheUpdate GetPositionUpdate(Domain.PositionExecReport report)
        {
            var pos = report.PositionCopy;
            System.Diagnostics.Debug.WriteLine("PR  #" + pos.Symbol + " " + pos.Side + " " + pos.Volume + " p=" + pos.Price);

            var update = DequeueWatingUpdate();
            _updateWatingForPosition = null;

            if (update == null)
                return GetPositionUpdateEntity(report, true);
            else
                update.Add(GetPositionUpdateEntity(report, false));

            return update;
        }

        private PositionUpdateAction GetPositionUpdateEntity(Domain.PositionExecReport report, bool notify)
        {
            if (report.PositionCopy.IsEmpty)
                return new PositionUpdateAction(report, OrderEntityAction.Removed, notify);
            else if (!_positions.ContainsKey(report.PositionCopy.Symbol))
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

        public void UpdatePosition(Domain.PositionExecReport position, bool notify)
        {
            var model = UpsertPosition(position.PositionCopy);
            if (notify)
            {
                PositionUpdate?.Invoke(model, position.ExecAction);
                PositionChanged?.Invoke(model);
            }
        }

        public void OnPositionAdded(Domain.PositionExecReport position, bool notify)
        {
            var model = UpsertPosition(position.PositionCopy);
            if (notify)
            {
                PositionUpdate?.Invoke(model, Domain.OrderExecReport.Types.ExecAction.Opened);
                PositionChanged?.Invoke(model);
            }
        }

        public void RemovePosition(Domain.PositionExecReport position, bool notify)
        {
            if (!_positions.TryGetValue(position.PositionCopy.Symbol, out PositionModel model))
                return;

            _positions.Remove(model.Symbol);

            if (notify)
            {
                PositionUpdate?.Invoke(model, Domain.OrderExecReport.Types.ExecAction.Closed);
                PositionChanged?.Invoke(model);
            }
        }

        private PositionModel UpsertPosition(Domain.PositionInfo position)
        {
            var positionModel = new PositionModel(position, this);
            _positions[position.Symbol] = positionModel;

            return positionModel;
        }

        #endregion

        #region Order management

        public void UpdateOrder(Domain.OrderExecReport.Types.ExecAction execAction, Domain.OrderExecReport.Types.EntityAction entityAction, ExecutionReport report, Domain.PositionInfo netPosUpdate)
        {
            OrderModel order = null;

            if (entityAction == Domain.OrderExecReport.Types.EntityAction.Added)
            {
                order = new OrderModel(report, this);
                _orders[order.Id] = order;

                OrderAdded?.Invoke(order);
            }
            else if (entityAction == Domain.OrderExecReport.Types.EntityAction.Removed)
            {
                order = Orders.GetOrDefault(report.OrderId);
                _orders.Remove(report.OrderId);
                order?.Update(report);

                OrderRemoved?.Invoke(order);
            }
            else if (entityAction == Domain.OrderExecReport.Types.EntityAction.Updated)
            {
                order = Orders.GetOrDefault(report.OrderId);

                if (order != null)
                {
                    bool replaceOrder = order.OrderType != report.OrderType; //crutch should be call before order Update

                    order.Update(report);

                    // workaround: dynamic collection filter can't react on field change
                    if (replaceOrder)
                        _orders[order.Id] = order;
                }
            }
            else
                order = new OrderModel(report, this);

            OrderUpdate?.Invoke(new OrderUpdateInfo(report, execAction, entityAction, order, netPosUpdate));
        }

        public void UpdateOrder(OrderExecAction execAction, OrderEntityAction entityAction, Domain.OrderInfo report)
        {
            OrderModel order = null;

            if (entityAction == OrderEntityAction.Added)
            {
                order = new OrderModel(report, this);
                _orders[order.Id] = order;

                OrderAdded?.Invoke(order);
            }
            else if (entityAction == OrderEntityAction.Removed)
            {
                order = Orders.GetOrDefault(report.Id);
                _orders.Remove(report.Id);
                order?.Update(report);

                OrderRemoved?.Invoke(order);
            }
            else if (entityAction == OrderEntityAction.Updated)
            {
                order = Orders.GetOrDefault(report.Id);

                bool typeChanged = order.OrderType != report.Type;

                order.Update(report);

                // workaround: dynamic collection filter can't react on field change
                if (typeChanged)
                    _orders[order.Id] = order;
            }
            else
                order = new OrderModel(report, this);
        }

        internal EntityCacheUpdate GetOrderUpdate(ExecutionReport report)
        {
            System.Diagnostics.Debug.WriteLine("ER  #" + report.OrderId + " " + report.OrderType + " " + report.ExecutionType + " opId=" + report.TradeRequestId);

            switch (report.ExecutionType)
            {
                case ExecutionType.New:
                    // Ignore
                    //if (report.OrderType == OrderType.Market)
                    //    return OnOrderOpened(report);
                    break;

                case ExecutionType.Calculated:
                    bool ignoreCalculate = (_accType == Domain.AccountInfo.Types.Type.Gross && report.OrderType == Domain.OrderInfo.Types.Type.Market);
                    if (!ignoreCalculate)
                    {
                        if (_orders.ContainsKey(report.OrderId))
                            return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Opened);
                        else
                            return OnOrderAdded(report, Domain.OrderExecReport.Types.ExecAction.Opened);
                    }
                    else
                        break;

                case ExecutionType.Split:
                    return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Splitted);

                case ExecutionType.Replace:
                    return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Modified);

                case ExecutionType.Expired:
                    return OnOrderRemoved(report, Domain.OrderExecReport.Types.ExecAction.Expired);

                case ExecutionType.Canceled:
                    return OnOrderCanceled(report, Domain.OrderExecReport.Types.ExecAction.Canceled);

                case ExecutionType.Rejected:
                    return OnOrderRejected(report, Domain.OrderExecReport.Types.ExecAction.Rejected);

                case ExecutionType.None:
                    if (report.OrderStatus == OrderStatus.Rejected)
                        return OnOrderRejected(report, Domain.OrderExecReport.Types.ExecAction.Rejected);
                    break;

                case ExecutionType.Trade:
                    if (report.OrderType == Domain.OrderInfo.Types.Type.StopLimit)
                    {
                        return OnOrderRemoved(report, Domain.OrderExecReport.Types.ExecAction.Activated);
                    }
                    else if (report.OrderType == Domain.OrderInfo.Types.Type.Limit || report.OrderType == Domain.OrderInfo.Types.Type.Stop)
                    {
                        if (report.ImmediateOrCancel)
                            return OnIocFilled(report);

                        if (report.LeavesVolume != 0)
                            return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Filled);

                        if (Type == Domain.AccountInfo.Types.Type.Gross)
                            return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Filled);
                        else
                            return OnOrderRemoved(report, Domain.OrderExecReport.Types.ExecAction.Filled);
                    }
                    else if (report.OrderType == Domain.OrderInfo.Types.Type.Position)
                    {
                        if (report.OrderStatus == OrderStatus.PartiallyFilled)
                            return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Closed);

                        if (report.OrderStatus == OrderStatus.Filled)
                            return OnOrderRemoved(report, Domain.OrderExecReport.Types.ExecAction.Closed);
                    }
                    else if (report.OrderType == Domain.OrderInfo.Types.Type.Market)
                    {
                        return OnMarketFilled(report, Domain.OrderExecReport.Types.ExecAction.Filled);
                    }
                    break;
            }

            return null;
        }

        private OrderUpdateAction OnOrderOpened(ExecutionReport report)
        {
            return new OrderUpdateAction(report, Domain.OrderExecReport.Types.ExecAction.Opened, Domain.OrderExecReport.Types.EntityAction.NoAction);
        }

        private OrderUpdateAction OnOrderAdded(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction algoAction)
        {
            if (report.OrderType == Domain.OrderInfo.Types.Type.Limit && report.ImmediateOrCancel)
                return new OrderUpdateAction(report, Domain.OrderExecReport.Types.ExecAction.Opened, Domain.OrderExecReport.Types.EntityAction.NoAction);

            var posUpdate = new OrderUpdateAction(report, algoAction, Domain.OrderExecReport.Types.EntityAction.Added);
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
            report.OrderType = Domain.OrderInfo.Types.Type.Position;
            report.LeavesVolume = report.InitialVolume.Value;
            return new OrderUpdateAction(report, Domain.OrderExecReport.Types.ExecAction.Opened, Domain.OrderExecReport.Types.EntityAction.Added);
        }

        private OrderUpdateAction OnIocFilled(ExecutionReport report)
        {
            return JoinWithPosition(new OrderUpdateAction(report, Domain.OrderExecReport.Types.ExecAction.Filled, Domain.OrderExecReport.Types.EntityAction.NoAction));
        }

        private OrderUpdateAction OnMarketFilled(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction algoAction)
        {
            return JoinWithPosition(new OrderUpdateAction(report, algoAction, Domain.OrderExecReport.Types.EntityAction.NoAction));
        }

        private OrderUpdateAction OnOrderRemoved(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction algoAction)
        {
            return JoinWithPosition(new OrderUpdateAction(report, algoAction, Domain.OrderExecReport.Types.EntityAction.Removed));
        }

        private OrderUpdateAction OnOrderUpdated(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction algoAction)
        {
            var orderUpdate = new OrderUpdateAction(report, algoAction, Domain.OrderExecReport.Types.EntityAction.Updated);

            // For gross stop/limit full fills: position opening is performed by updating old order, not adding new order
            if (report.OrderType == Domain.OrderInfo.Types.Type.Position && report.ExecutionType == ExecutionType.Calculated)
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

        private OrderUpdateAction OnOrderRejected(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction algoAction)
        {
            return new OrderUpdateAction(report, algoAction, Domain.OrderExecReport.Types.EntityAction.NoAction);
        }

        private OrderUpdateAction OnOrderCanceled(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction algoAction)
        {
            // Limit Ioc don't get into order collection
            return new OrderUpdateAction(report, algoAction, (report.OrderType == Domain.OrderInfo.Types.Type.Limit && report.ImmediateOrCancel) 
                ? Domain.OrderExecReport.Types.EntityAction.NoAction : Domain.OrderExecReport.Types.EntityAction.Removed);
        }

        /// bread ration: position updates should be joined with exec reports to be atomic
        private OrderUpdateAction JoinWithPosition(OrderUpdateAction update)
        {
            if ((Type == Domain.AccountInfo.Types.Type.Gross || Type == Domain.AccountInfo.Types.Type.Net) && update.ExecAction == Domain.OrderExecReport.Types.ExecAction.Filled)
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
            var orders = Orders.Snapshot.Values.Select(o => o.GetInfo()).ToList();
            var positions = Positions.Snapshot.Values.Select(p => p.GetInfo()).ToList();
            var assets = Assets.Snapshot.Values.Select(a => a.GetInfo()).ToList();

            return new Snapshot(info, orders, positions, assets);
        }

        [Serializable]
        public class Snapshot : EntityCacheUpdate
        {
            private Domain.AccountInfo _accInfo;
            private IEnumerable<Domain.OrderInfo> _orders;
            private IEnumerable<Domain.PositionInfo> _positions;
            private IEnumerable<Domain.AssetInfo> _assets;

            public Snapshot(Domain.AccountInfo accInfo, IEnumerable<Domain.OrderInfo> orders,
                IEnumerable<Domain.PositionInfo> positions, IEnumerable<Domain.AssetInfo> assets)
            {
                _accInfo = accInfo;
                _orders = orders ?? Enumerable.Empty<Domain.OrderInfo>();
                _positions = positions ?? Enumerable.Empty<Domain.PositionInfo>();
                _assets = assets ?? Enumerable.Empty<Domain.AssetInfo>();
            }

            public void Apply(EntityCache cache)
            {
                cache.Account.Init(_accInfo, _orders, _positions, _assets);
            }
        }

        [Serializable]
        public class LoadOrderUpdate : EntityCacheUpdate
        {
            public LoadOrderUpdate(Domain.OrderInfo order)
            {
                Order = order ?? throw new ArgumentNullException("symbol");
            }

            private Domain.OrderInfo Order { get; }

            public void Apply(EntityCache cache)
            {
                var acc = cache.Account;
                acc._orders.Add(Order.Id, new OrderModel(Order, acc));
            }
        }

        [Serializable]
        private class OrderUpdateAction : EntityCacheUpdate
        {
            private ExecutionReport _report;
            private Domain.OrderExecReport.Types.ExecAction _execAction;
            private Domain.OrderExecReport.Types.EntityAction _entityAction;
            private PositionUpdateAction _netPositionUpdate;
            private OrderUpdateAction _grossPositionUpdate;

            public OrderUpdateAction(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction execAction, Domain.OrderExecReport.Types.EntityAction entityAction)
            {
                _report = report;
                _execAction = execAction;
                _entityAction = entityAction;
            }

            public Domain.OrderExecReport.Types.ExecAction ExecAction => _execAction;

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
                cache.Account.UpdateOrder(_execAction, _entityAction, _report, _netPositionUpdate?.Position.PositionCopy);
                cache.Account.UpdateBalance(_report);
                _grossPositionUpdate?.Apply(cache);
            }
        }

        [Serializable]
        private class BalanceUpdateAction : EntityCacheUpdate
        {
            private Domain.BalanceOperation _report;

            public BalanceUpdateAction(Domain.BalanceOperation report)
            {
                _report = report;
            }

            public void Apply(EntityCache cache)
            {
                var acc = cache.Account;
                acc.UpdateBalance(_report.Balance, _report.Currency);
                acc.BalanceUpdate?.Invoke(_report);
            }
        }

        [Serializable]
        private class PositionUpdateAction : EntityCacheUpdate
        {
            private Domain.PositionExecReport _report;
            private OrderEntityAction _entityAction;
            private bool _notify;

            public Domain.PositionExecReport Position => _report;

            public PositionUpdateAction(Domain.PositionExecReport report, OrderEntityAction action, bool notify)
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
        public OrderUpdateInfo(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction execAction, Domain.OrderExecReport.Types.EntityAction entityAction, OrderModel order, Domain.PositionInfo position)
        {
            Report = report;
            ExecAction = execAction;
            EntityAction = entityAction;
            Order = order;
            Position = position;
        }

        public ExecutionReport Report { get; }
        public Domain.OrderExecReport.Types.ExecAction ExecAction { get; }
        public Domain.OrderExecReport.Types.EntityAction EntityAction { get; }
        public OrderModel Order { get; }
        public Domain.PositionInfo Position { get; }
    }
}
