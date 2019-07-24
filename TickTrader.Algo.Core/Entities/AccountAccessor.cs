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

namespace TickTrader.Algo.Core
{
    public class AccountAccessor : AccountDataProvider, IMarginAccountInfo2, ICashAccountInfo2
    {
        private PluginBuilder builder;
        private Dictionary<string, OrderFilteredCollection> bySymbolFilterCache;
        private Dictionary<string, OrderFilteredCollection> byTagFilterCache;
        private Dictionary<Predicate<Order>, OrderFilteredCollection> customFilterCache;
        private TradeHistory _history;
        private bool _blEventsEnabled;

        public AccountAccessor(PluginBuilder builder)
        {
            this.builder = builder;

            Orders = new OrdersCollection(builder);
            NetPositions = new PositionCollection(builder);
            Assets = new AssetsCollection(builder);

            //Equity = double.NaN;
            //Profit = double.NaN;
            //Commision = double.NaN;
            //MarginLevel = double.NaN;
            //Margin = double.NaN;
        }

        public OrdersCollection Orders { get; private set; }
        public PositionCollection NetPositions { get; private set; }
        public AssetsCollection Assets { get; private set; }
        public TradeHistory HistoryProvider { get { return _history; } set { _history = value; } }

        public string Id { get; set; }
        public double Balance { get; internal set; }
        public string BalanceCurrency { get; private set; }
        public Currency BalanceCurrencyInfo { get; private set; }
        public int Leverage { get; internal set; }
        public AccountTypes Type { get; internal set; }
        public bool Isolated { get; set; }
        public string InstanceId { get; internal set; }
        public NumberFormatInfo BalanceCurrencyFormat { get; private set; }

        public bool IsMarginType => Type == AccountTypes.Net || Type == AccountTypes.Gross;
        public bool IsCashType => Type == AccountTypes.Cash;

        public void Update(AccountEntity info, Dictionary<string, Currency> currencies)
        {
            Id = info.Id;
            Type = info.Type;
            Leverage = info.Leverage;
            Balance = info.Balance;
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
        public double Equity => (double?)MarginCalc?.Equity ?? double.NaN;
        public double Margin => (double?)MarginCalc?.Margin ?? double.NaN;
        public double MarginLevel => (double?)MarginCalc?.MarginLevel ?? double.NaN;
        public double Profit => (double?)MarginCalc?.Profit ?? double.NaN;
        public double Commision => (double?)MarginCalc?.Commission ?? double.NaN;

        double AccountDataProvider.Balance => (double)Balance;

        #region BO

        long IAccountInfo2.Id => 0;
        public BO.AccountingTypes AccountingType => TickTraderToAlgo.Convert(Type);
        double IMarginAccountInfo2.Balance => Balance;
        IEnumerable<IOrderModel2> IAccountInfo2.Orders => (IEnumerable<OrderAccessor>)Orders.OrderListImpl;
        IEnumerable<IPositionModel2> IMarginAccountInfo2.Positions => NetPositions;
        IEnumerable<BL.IAssetModel> ICashAccountInfo2.Assets => Assets;

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
        public event Action Reset = delegate { };
        public event Action<IPositionModel2> PositionChanged;
        public event Action<BL.IAssetModel, BL.AssetChangeTypes> AssetsChanged;

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

        private void OnAssetsChanged(BL.IAssetModel asset, AssetChangeType type)
        {
            UpdateAccountInfo($"Change asset({type})", () => AssetsChanged?.Invoke(asset, TickTraderToAlgo.Convert(type)));
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

        internal void IncreasePosition(string symbol, double amount, double price, OrderSide side, Func<string> idGenerator)
        {
            var pos = NetPositions.GetOrCreatePosition(symbol, idGenerator);
            pos.Increase(amount, price, side);
            OnPositionUpdated(pos);
        }

        internal void IncreaseAsset(string currency, double byAmount)
        {
            AssetChangeType chType;
            var asset = Assets.GetOrCreateAsset(currency, out chType);
            asset.IncreaseBy((decimal)byAmount);
            OnAssetsChanged(asset, chType);
        }

        #endregion

        public double? GetSymbolMargin(string symbol, OrderSide side)
        {
            return builder.Calculator?.GetSymbolMargin(symbol, side);
        }

        public double? CalculateOrderMargin(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None)
        {
            var symbolAccessor = builder?.Symbols?.GetOrDefault(symbol);
            if (symbolAccessor != null && builder.Calculator != null)
            {
                var amount = volume * symbolAccessor.ContractSize;

                return builder.Calculator.CalculateOrderMargin(symbolAccessor, amount, price, stopPrice, type, side, OrderEntity.IsHiddenOrder(maxVisibleVolume));
            }
            return null;
        }

        public bool HasEnoughMarginToOpenOrder(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None)
        {
            var symbolAccessor = builder?.Symbols?.GetOrDefault(symbol);
            if (symbolAccessor != null && builder.Calculator != null)
            {
                var amount = volume * symbolAccessor.ContractSize;

                return builder.Calculator.HasEnoughMarginToOpenOrder(symbolAccessor, amount, type, side, price, stopPrice, OrderEntity.IsHiddenOrder(maxVisibleVolume));
            }
            return false;
        }
    }
}
