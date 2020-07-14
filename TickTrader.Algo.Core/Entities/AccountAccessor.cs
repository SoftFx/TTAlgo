using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessLogic;
using TickTrader.Algo.Api;
using System.Globalization;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class AccountAccessor : AccountDataProvider, IMarginAccountInfo2, ICashAccountInfo2
    {
        private PluginBuilder builder;
        private Dictionary<string, OrderFilteredCollection> bySymbolFilterCache;
        private Dictionary<string, OrderFilteredCollection> byTagFilterCache;
        private Dictionary<Predicate<Order>, OrderFilteredCollection> customFilterCache;
        private TradeHistory _history;
        private OrdersCollection _orders;
        private PositionCollection _positions;
        private AssetsCollection _assets;
        private bool _blEventsEnabled;
        private decimal _balance;
        private double _dblBalance;
        private string _balanceCurrency;
        private string _id;
        private AccountInfo.Types.Type _type;
        private AccountTypes _apiType;

        public AccountAccessor(PluginBuilder builder)
        {
            this.builder = builder;
        }

        public OrdersCollection Orders
        {
            get
            {
                OnTradeInfoAccess();
                return _orders;
            }
        }

        public PositionCollection NetPositions
        {
            get
            {
                OnTradeInfoAccess();
                return _positions;
            }
        }

        public AssetsCollection Assets
        {
            get
            {
                OnCalcAccess();
                return _assets;
            }
        }

        public TradeHistory HistoryProvider { get { return _history; } set { _history = value; } }

        public string Id
        {
            get
            {
                OnTradeInfoAccess();
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public decimal Balance
        {
            get
            {
                OnTradeInfoAccess();
                return _balance;
            }
            set
            {
                _balance = value;
                _dblBalance = (double)value;
            }
        }
        public string BalanceCurrency
        {
            get
            {
                OnTradeInfoAccess();
                return _balanceCurrency;
            }
            private set
            {
                _balanceCurrency = value;
            }
        }
        public Currency BalanceCurrencyInfo { get; private set; }
        public int Leverage { get; internal set; }
        public AccountInfo.Types.Type Type
        {
            get
            {
                OnTradeInfoAccess();
                return _type;
            }
            internal set
            {
                _type = value;
                _apiType = ApiEnumConverter.Convert(value);
            }
        }
        public bool Isolated { get; set; }
        public string InstanceId { get; internal set; }
        public NumberFormatInfo BalanceCurrencyFormat { get; private set; }

        public bool IsMarginType => Type == AccountInfo.Types.Type.Net || Type == AccountInfo.Types.Type.Gross;
        public bool IsCashType => Type == AccountInfo.Types.Type.Cash;

        double IMarginAccountInfo2.Balance => _dblBalance;
        AccountTypes AccountDataProvider.Type
        {
            get
            {
                OnTradeInfoAccess();
                return _apiType;
            }
        }

        public void Update(Domain.AccountInfo info, Dictionary<string, Currency> currencies)
        {
            Id = info.Id;
            Type = info.Type;
            Leverage = info.Leverage;
            Balance = (decimal)info.Balance;
            UpdateCurrency(currencies.GetOrStub(info.BalanceCurrency));
            Assets.Clear();
            foreach (var asset in info.Assets)
                builder.Account.Assets.Update(asset, currencies);
        }

        internal void UpdateCurrency(Currency currency)
        {
            BalanceCurrency = currency.Name;
            BalanceCurrencyInfo = currency;
            BalanceCurrencyFormat = ((CurrencyEntity)currency).Format;
        }

        internal void ResetCurrency()
        {
            BalanceCurrency = "";
            BalanceCurrencyInfo = null;
            BalanceCurrencyFormat = null;
        }

        internal void FireBalanceUpdateEvent()
        {
            builder.InvokePluginMethod((b, p) => p.BalanceUpdated(), this);
        }

        internal void FireBalanceDividendEvent(BalanceDividendEventArgsImpl args)
        {
            builder.InvokePluginMethod((b, p) => p.BalanceDividend(args), this);
        }

        internal void FireResetEvent()
        {
            builder.InvokePluginMethod((b, p) => p.Reset(), this);
        }

        public OrderList OrdersByTag(string orderTag)
        {
            if (string.IsNullOrEmpty(orderTag))
                throw new ArgumentException("Order tag cannot be null or empty string!");

            if (byTagFilterCache == null)
                byTagFilterCache = new Dictionary<string, OrderFilteredCollection>();

            OrderFilteredCollection collection;
            if (byTagFilterCache.TryGetValue(orderTag, out collection))
                return collection;

            collection = new OrderFilteredCollection(Orders.OrderListImpl, o => o.Comment == orderTag);
            byTagFilterCache.Add(orderTag, collection);
            return collection;
        }

        public OrderList OrdersBySymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("Symbol cannot be null or empty string!");

            if (bySymbolFilterCache == null)
                bySymbolFilterCache = new Dictionary<string, OrderFilteredCollection>();

            OrderFilteredCollection collection;
            if (bySymbolFilterCache.TryGetValue(symbol, out collection))
                return collection;

            collection = new OrderFilteredCollection(Orders.OrderListImpl, o => o.Symbol == symbol);
            bySymbolFilterCache.Add(symbol, collection);
            return collection;
        }

        public OrderList OrdersBy(Predicate<Order> customCondition)
        {
            if (customCondition == null)
                throw new ArgumentNullException("customCondition");

            if (customFilterCache == null)
                customFilterCache = new Dictionary<Predicate<Order>, OrderFilteredCollection>();

            OrderFilteredCollection collection;
            if (customFilterCache.TryGetValue(customCondition, out collection))
                return collection;

            collection = new OrderFilteredCollection(Orders.OrderListImpl, customCondition);
            customFilterCache.Add(customCondition, collection);
            return collection;
        }

        OrderList AccountDataProvider.Orders
        {
            get
            {
                if (!Isolated)
                    return Orders.OrderListImpl;
                else
                    return OrdersBy(TagFilter);
            }
        }

        private bool TagFilter(Order order)
        {
            return InstanceId == order.InstanceId;
        }

        AssetList AccountDataProvider.Assets { get { return Assets.AssetListImpl; } }
        NetPositionList AccountDataProvider.NetPositions { get { return NetPositions.PositionListImpl; } }
        TradeHistory AccountDataProvider.TradeHistory => _history;

        internal MarginAccountCalc MarginCalc { get; set; }
        public double Equity => GetCalc()?.Equity ?? double.NaN;
        public double Margin => GetCalc()?.Margin ?? double.NaN;
        public double MarginLevel => GetCalc()?.MarginLevel ?? double.NaN;
        public double Profit => GetCalc()?.Profit ?? double.NaN;
        public double Commision => (double?)GetCalc()?.Commission ?? double.NaN;

        double AccountDataProvider.Balance => (double)Balance;

        #region Lazy init

        internal event Action CalcRequested;
        internal event Action TradeInfoRequested;

        private MarginAccountCalc GetCalc()
        {
            OnCalcAccess();
            return MarginCalc;
        }

        private void OnCalcAccess()
        {
            if (MarginCalc == null)
            {
                OnTradeInfoAccess();
                CalcRequested?.Invoke();
            }
        }

        internal void OnTradeInfoAccess()
        {
            if (_orders == null)
            {
                _orders = new OrdersCollection(builder);
                _positions = new PositionCollection(builder);
                _assets = new AssetsCollection(builder);
                TradeInfoRequested?.Invoke();
            }
        }

        internal void Init(IAccountInfoProvider dataProvider, Dictionary<string, Currency> currencies)
        {
            var accInfo = dataProvider.AccountInfo;

            _orders.Clear();
            _positions.Clear();
            _assets.Clear();

            Update(accInfo, currencies);

            foreach (var order in dataProvider.GetOrders())
                _orders.Add(order, this);
            foreach (var position in dataProvider.GetPositions())
                _positions.UpdatePosition(position.PositionInfo);
        }

        #endregion

        #region BO

        long IAccountInfo2.Id => 0;
        public AccountInfo.Types.Type AccountingType => Type;
        //decimal IMarginAccountInfo2.Balance => Balance;
        IEnumerable<IOrderModel2> IAccountInfo2.Orders => (IEnumerable<OrderAccessor>)Orders.OrderListImpl;
        IEnumerable<IPositionModel2> IMarginAccountInfo2.Positions => NetPositions;
        IEnumerable<IAssetModel2> ICashAccountInfo2.Assets => Assets;

        //void BL.IAccountInfo.LogInfo(string message)
        //{
        //}

        //void BL.IAccountInfo.LogWarn(string message)
        //{
        //}

        //void BL.IAccountInfo.LogError(string message)
        //{
        //}

        public event Action<IOrderModel2> OrderAdded = delegate { };
        public event Action<IEnumerable<IOrderModel2>> OrdersAdded { add { } remove { } }
        public event Action<IOrderModel2> OrderRemoved = delegate { };
        //public event Action<IOrderModel2> OrderReplaced = delegate { };
        public event Action BalanceUpdated = delegate { };
        public event Action<IBalanceDividendEventArgs> BalanceDividend = delegate { };
        public event Action Reset = delegate { };
        public event Action<IPositionModel2> PositionChanged;
        public event Action<IAssetModel2, AssetChangeType> AssetsChanged;

        internal void EnableBlEvents()
        {
            Orders.Added += OnOrderAdded;
            //Orders.Replaced += OnOrderReplaced;
            Orders.Removed += OnOrderRemoved;

            NetPositions.PositionUpdated += OnPositionUpdated;
            //NetPositions.PositionRemoved += OnPositionRemoved;

            Assets.AssetChanged += OnAssetsChanged;

            _blEventsEnabled = true;
        }

        internal void DisableBlEvents()
        {
            if (_blEventsEnabled)
            {
                Orders.Added -= OnOrderAdded;
                //Orders.Replaced -= OnOrderReplaced;
                Orders.Removed -= OnOrderRemoved;

                NetPositions.PositionUpdated -= OnPositionUpdated;
                //NetPositions.PositionRemoved -= OnPositionRemoved;

                Assets.AssetChanged -= OnAssetsChanged;

                _blEventsEnabled = false;
            }
        }

        private void OnOrderAdded(IOrderModel2 order)
        {
            UpdateAccountInfo("Add order", () => OrderAdded?.Invoke(order));
        }

        //private void OnOrderReplaced(IOrderModel2 order)
        //{
        //    UpdateAccountInfo("Replace order", () => OrderReplaced?.Invoke(order));
        //}

        private void OnOrderRemoved(IOrderModel2 order)
        {
            UpdateAccountInfo("Remove order", () => OrderRemoved?.Invoke(order));
        }

        private void OnPositionUpdated(IPositionModel2 position)
        {
            UpdateAccountInfo("Update position", () => PositionChanged?.Invoke(position));
        }

        //private void OnPositionRemoved(IPositionModel2 position)
        //{
        //    UpdateAccountInfo("Remove position", () => PositionChanged?.Invoke(position, PositionChageTypes.Removed));
        //}

        private void OnAssetsChanged(IAssetModel2 asset, AssetChangeType type)
        {
            UpdateAccountInfo($"Change asset({type})", () => AssetsChanged?.Invoke(asset, type));
        }

        private void UpdateAccountInfo(string actionName, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                builder.Logger.OnError($"Account calculator: {actionName} failed", ex);
            }
        }

        #endregion

        #region Emulation

        internal OrderAccessor GetOrderOrThrow(string orderId)
        {
            return Orders.GetOrderOrNull(orderId)
                ?? throw new OrderValidationError("Order Not Found " + orderId, OrderCmdResultCodes.OrderNotFound);
        }

        internal void IncreasePosition(string symbol, decimal amount, decimal price, Domain.OrderInfo.Types.Side side, Func<string> idGenerator)
        {
            var pos = NetPositions.GetOrCreatePosition(symbol, idGenerator);
            pos.Increase(amount, price, side);
            OnPositionUpdated(pos);
        }

        internal void IncreaseAsset(string currency, decimal byAmount)
        {
            var asset = Assets.GetOrCreateAsset(currency, out var chType);
            asset.IncreaseBy(byAmount);
            OnAssetsChanged(asset, chType);
        }

        #endregion

        public double? GetSymbolMargin(string symbol, OrderSide side)
        {
            return builder.Calculator?.GetSymbolMargin(symbol, side.ToCoreEnum());
        }

        public double? CalculateOrderMargin(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None)
        {
            var symbolAccessor = builder?.Symbols?.GetOrDefault(symbol);
            if (symbolAccessor != null && builder.Calculator != null)
            {
                var amount = volume * symbolAccessor.ContractSize;

                return builder.Calculator.CalculateOrderMargin(symbolAccessor, amount, price, stopPrice, type.ToCoreEnum(), side.ToCoreEnum(), OrderEntity.IsHiddenOrder(maxVisibleVolume));
            }
            return null;
        }

        public bool HasEnoughMarginToOpenOrder(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None)
        {
            var symbolAccessor = builder?.Symbols?.GetOrDefault(symbol);
            if (symbolAccessor != null && builder.Calculator != null)
            {
                var amount = volume * symbolAccessor.ContractSize;

                return builder.Calculator.HasEnoughMarginToOpenOrder(symbolAccessor, amount, type.ToCoreEnum(), side.ToCoreEnum(), price, stopPrice, OrderEntity.IsHiddenOrder(maxVisibleVolume), out _);
            }
            return false;
        }
    }
}
