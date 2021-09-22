using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Calculator;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class AccountModel : IMarginAccountInfo2, ICashAccountInfo2, IMarketStateAccountInfo
    {
        private IAlgoLogger _logger;

        private readonly VarDictionary<string, PositionInfo> _positions = new VarDictionary<string, PositionInfo>();
        private readonly VarDictionary<string, AssetInfo> _assets = new VarDictionary<string, AssetInfo>();
        private readonly VarDictionary<string, OrderInfo> _orders = new VarDictionary<string, OrderInfo>();
        private Domain.AccountInfo.Types.Type? _accType;
        private readonly IReadOnlyDictionary<string, CurrencyInfo> _currencies;
        private readonly IReadOnlyDictionary<string, SymbolInfo> _symbols;
        private bool _isCalcStarted;
        private OrderUpdateAction _updateWatingForPosition = null;
        private IFeedSubscription _subscriptions;

        public AlgoMarketState Market { get; }

        public CashAccountCalculator CashCalculator { get; private set; }

        public MarginAccountCalculator MarginCalculator { get; private set; }

        public AccountModel(IVarSet<string, CurrencyInfo> currecnies, IVarSet<string, SymbolInfo> symbols)
        {
            _logger = AlgoLoggerFactory.GetLogger<AccountModel>();

            _currencies = currecnies.Snapshot;
            _symbols = symbols.Snapshot;

            Market = new AlgoMarketState();
        }

        public event System.Action AccountTypeChanged = delegate { };
        public IVarSet<string, PositionInfo> Positions => _positions;
        public IVarSet<string, OrderInfo> Orders => _orders;
        public IVarSet<string, AssetInfo> Assets => _assets;

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
        public CurrencyInfo BalanceCurrencyInfo { get; private set; }
        public int Leverage { get; private set; }

        AccountInfo.Types.Type IAccountInfo2.Type => Type ?? AccountInfo.Types.Type.Gross;

        IEnumerable<IOrderCalcInfo> IAccountInfo2.Orders => Orders.Snapshot.Values;

        IEnumerable<IAssetInfo> ICashAccountInfo2.Assets => Assets.Snapshot.Values;

        IEnumerable<IPositionInfo> IMarginAccountInfo2.Positions => Positions.Snapshot.Values;

        CurrencyInfo IMarketStateAccountInfo.BalanceCurrency => BalanceCurrencyInfo;

        public event Action<OrderUpdateInfo> OrderUpdate;
        public event Action<PositionInfo, Domain.OrderExecReport.Types.ExecAction> PositionUpdate;
        public event Action<Domain.BalanceOperation> BalanceOperationUpdate;
        public event Action BalanceUpdate;

        public event Action<IPositionInfo> PositionChanged;
        public event Action<IOrderCalcInfo> OrderAdded;
        public event Action<IEnumerable<IOrderCalcInfo>> OrdersAdded;
        public event Action<IOrderCalcInfo> OrderRemoved;
        public event Action<IAssetInfo, AssetChangeType> AssetsChanged;
        public event Action<IPositionInfo> PositionRemoved;

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
            Market.Init(this, marketData.Symbols.Snapshot.Values, marketData.Currencies.Snapshot.Values);
            Market.StartCalculators();
            _subscriptions = marketData.Distributor.AddSubscription((rate) => Market.GetUpdateNode(rate), marketData.Symbols.Snapshot.Keys);


            if (!_isCalcStarted)
            {
                _isCalcStarted = true;

                switch (Type)
                {
                    case AccountInfo.Types.Type.Cash:
                        CashCalculator = new CashAccountCalculator(this, Market, _logger.Error);
                        break;
                    case AccountInfo.Types.Type.Net:
                    case AccountInfo.Types.Type.Gross:
                        MarginCalculator = new MarginAccountCalculator(this, Market, _logger.Error);
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
            BalanceCurrencyInfo = balanceCurrencyInfo;

            //Market.StartCalculators();

            foreach (var fdkPosition in positions)
            {
                var model = fdkPosition;
                this._positions.Add(fdkPosition.Symbol, model);
                PositionChanged?.Invoke(model);
            }

            foreach (var fdkOrder in orders)
            {
                fdkOrder.SetSymbol(_symbols.GetOrDefault(fdkOrder.Symbol));
                this._orders.Add(fdkOrder.Id, fdkOrder);
            }

            OrdersAdded?.Invoke(orders);

            foreach (var fdkAsset in assets)
            {
                var model = fdkAsset;
                this._assets.Add(fdkAsset.Currency, model);

                AssetsChanged?.Invoke(model, AssetChangeType.Added);
            }

            _orders.Updated += OrdersUpdated;
        }

        private void OrdersUpdated(DictionaryUpdateArgs<string, OrderInfo> args)
        {
            switch (args.Action)
            {
                case DLinqAction.Insert:
                    OrderAdded?.Invoke(args.NewItem);
                    break;
                case DLinqAction.Remove:
                    OrderRemoved?.Invoke(args.OldItem);
                    break;
                case DLinqAction.Replace:
                    OrderRemoved?.Invoke(args.OldItem);
                    OrderAdded?.Invoke(args.NewItem);
                    break;
                default:
                    break;
            }
        }

        public void Deinit()
        {
            if (_isCalcStarted)
            {
                CashCalculator?.Dispose();
                MarginCalculator?.Dispose();

                CashCalculator = null;
                MarginCalculator = null;
                _isCalcStarted = false;
            }

            _subscriptions?.CancelAll();
            _orders.Updated -= OrdersUpdated;
        }

        public Domain.AccountInfo GetAccountInfo()
        {
            return new Domain.AccountInfo(_accType != Domain.AccountInfo.Types.Type.Cash ? Balance : (double?)null, BalanceCurrency,
                Assets.Snapshot.Values.ToArray())
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
            BalanceUpdate?.Invoke();
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
            var model = assetInfo;

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
                var model = new AssetInfo(newBalance, currency);

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
                return new PositionUpdateAction(report, Domain.OrderExecReport.Types.EntityAction.Removed, notify);
            else if (!_positions.ContainsKey(report.PositionCopy.Symbol))
                return new PositionUpdateAction(report, Domain.OrderExecReport.Types.EntityAction.Added, notify);
            else
                return new PositionUpdateAction(report, Domain.OrderExecReport.Types.EntityAction.Updated, notify);
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

            PositionChanged?.Invoke(model);

            if (notify)
                PositionUpdate?.Invoke(model, position.ExecAction);
        }

        public void OnPositionAdded(Domain.PositionExecReport position, bool notify)
        {
            var model = UpsertPosition(position.PositionCopy);

            PositionChanged?.Invoke(model);

            if (notify)
                PositionUpdate?.Invoke(model, Domain.OrderExecReport.Types.ExecAction.Opened);
        }

        public void RemovePosition(Domain.PositionExecReport position, bool notify)
        {
            if (!_positions.TryGetValue(position.PositionCopy.Symbol, out PositionInfo model))
                return;

            _positions.Remove(model.Symbol);

            PositionRemoved?.Invoke(model);
            if (notify)
                PositionUpdate?.Invoke(model, Domain.OrderExecReport.Types.ExecAction.Closed);
        }

        private PositionInfo UpsertPosition(Domain.PositionInfo position)
        {
            var positionModel = position;
            _positions[position.Symbol] = positionModel;

            return positionModel;
        }

        #endregion

        #region Order management

        public void UpdateOrder(Domain.OrderExecReport.Types.ExecAction execAction, Domain.OrderExecReport.Types.EntityAction entityAction, ExecutionReport report, Domain.PositionInfo netPosUpdate)
        {
            var order = UpdateOrderCollection(entityAction, report);

            OrderUpdate?.Invoke(new OrderUpdateInfo(report, execAction, entityAction, order, netPosUpdate));
        }

        public OrderInfo UpdateOrderCollection(Domain.OrderExecReport.Types.EntityAction entityAction, IOrderUpdateInfo report)
        {
            var order = Orders.GetOrDefault(report?.Id) ?? new OrderInfo(_symbols.GetOrDefault(report.Symbol), report);

            switch (entityAction)
            {
                case Domain.OrderExecReport.Types.EntityAction.Added:
                    _orders[order.Id] = order;
                    break;
                case Domain.OrderExecReport.Types.EntityAction.Removed:
                    _orders.Remove(report.Id);
                    order?.Update(report);
                    break;
                case Domain.OrderExecReport.Types.EntityAction.Updated:
                    order.Update(report);
                    _orders[order.Id] = order;
                    break;
                default:
                    break;
            }

            return order;
        }

        internal EntityCacheUpdate GetOrderUpdate(ExecutionReport report)
        {
            System.Diagnostics.Debug.WriteLine($"ER({report.ExecutionType}, {report.OrderStatus})  #{report.Id} {report.Type} opId={report.TradeRequestId}");

            switch (report.ExecutionType)
            {
                case ExecutionType.New:
                    // Ignore
                    //if (report.OrderType == OrderType.Market)
                    //    return OnOrderOpened(report);
                    break;

                case ExecutionType.Calculated:
                    bool ignoreCalculate = (_accType == Domain.AccountInfo.Types.Type.Gross && report.Type == Domain.OrderInfo.Types.Type.Market && !report.IsContingentOrder) || report.OrderStatus == OrderStatus.Executing;
                    if (!ignoreCalculate)
                    {
                        if (_orders.TryGetValue(report.Id, out var order))
                        {
                            // ExecutionReport(Type=Calculated, Status=Calculated) is usually a transition from Executing state, which we currently ignore
                            // The only exception is fully filled pending orders on gross acc, which trigger position with same id
                            // StopLimit orders get new order id and opened as limit orders after activation
                            if ((order.Type == OrderInfo.Types.Type.Limit || order.Type == OrderInfo.Types.Type.Stop) && report.Type == OrderInfo.Types.Type.Position)
                                return OnOrderUpdated(report, OrderExecReport.Types.ExecAction.Opened);
                            else break;
                        }
                        else
                            return OnOrderAdded(report, Domain.OrderExecReport.Types.ExecAction.Opened);
                    }
                    else
                        break;

                case ExecutionType.Split:
                    return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Splitted);

                case ExecutionType.Replace:
                    if (report.OrderStatus != OrderStatus.Executing)
                        return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Modified);
                    else
                        break;

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
                    if (report.Type == Domain.OrderInfo.Types.Type.StopLimit)
                    {
                        return OnOrderRemoved(report, Domain.OrderExecReport.Types.ExecAction.Activated);
                    }
                    else if (report.Type == Domain.OrderInfo.Types.Type.Limit || report.Type == Domain.OrderInfo.Types.Type.Stop)
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
                    else if (report.Type == Domain.OrderInfo.Types.Type.Position)
                    {
                        if (report.OrderStatus == OrderStatus.PartiallyFilled)
                            return OnOrderUpdated(report, Domain.OrderExecReport.Types.ExecAction.Closed);

                        if (report.OrderStatus == OrderStatus.Filled)
                            return OnOrderRemoved(report, Domain.OrderExecReport.Types.ExecAction.Closed);
                    }
                    else if (report.Type == Domain.OrderInfo.Types.Type.Market)
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
            if (report.Type == Domain.OrderInfo.Types.Type.Limit && report.ImmediateOrCancel)
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
            report.Type = Domain.OrderInfo.Types.Type.Position;
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
            if (report.Type == Domain.OrderInfo.Types.Type.Position && report.ExecutionType == ExecutionType.Calculated)
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
            return new OrderUpdateAction(report, algoAction, (report.Type == Domain.OrderInfo.Types.Type.Limit && report.ImmediateOrCancel)
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

        public SymbolInfo GetSymbolOrNull(string name) => _symbols.GetOrDefault(name);

        public EntityCacheUpdate GetSnapshotUpdate()
        {
            var info = GetAccountInfo();
            var orders = Orders.Snapshot.Values.ToList();
            var positions = Positions.Snapshot.Values.ToList();
            var assets = Assets.Snapshot.Values.ToList();

            return new Snapshot(info, orders, positions, assets);
        }

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
                Order.SetSymbol(cache.Symbols.GetOrDefault(Order.Symbol));
                acc._orders.Add(Order.Id, Order);
            }
        }

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
                acc.BalanceOperationUpdate?.Invoke(_report);
                acc.BalanceUpdate?.Invoke();
            }
        }

        private class PositionUpdateAction : EntityCacheUpdate
        {
            private Domain.PositionExecReport _report;
            private Domain.OrderExecReport.Types.EntityAction _entityAction;
            private bool _notify;

            public Domain.PositionExecReport Position => _report;

            public PositionUpdateAction(Domain.PositionExecReport report, Domain.OrderExecReport.Types.EntityAction action, bool notify)
            {
                _report = report;
                _entityAction = action;
                _notify = notify;
            }

            public void Apply(EntityCache cache)
            {
                if (_entityAction == Domain.OrderExecReport.Types.EntityAction.Added)
                    cache.Account.OnPositionAdded(_report, _notify);
                else if (_entityAction == Domain.OrderExecReport.Types.EntityAction.Updated)
                    cache.Account.UpdatePosition(_report, _notify);
                else if (_entityAction == Domain.OrderExecReport.Types.EntityAction.Removed)
                    cache.Account.RemovePosition(_report, _notify);
            }
        }
    }

    public class OrderUpdateInfo
    {
        public OrderUpdateInfo(ExecutionReport report, Domain.OrderExecReport.Types.ExecAction execAction, Domain.OrderExecReport.Types.EntityAction entityAction, OrderInfo order, Domain.PositionInfo position)
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
        public OrderInfo Order { get; }
        public Domain.PositionInfo Position { get; }
    }
}
