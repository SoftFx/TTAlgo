using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessLogic;
using TickTrader.Algo.Core.Entities;
using System.Linq;

namespace TickTrader.Algo.Core
{
    public class AccountEntity : AccountDataProvider, BL.IMarginAccountInfo, BL.ICashAccountInfo
    {
        private PluginBuilder builder;
        private Dictionary<string, OrderFilteredCollection> bySymbolFilterCache;
        private Dictionary<string, OrderFilteredCollection> byTagFilterCache;
        private Dictionary<Predicate<Order>, OrderFilteredCollection> customFilterCache;
        private TradeHistoryAdapter _historyAdapter;

        public AccountEntity(PluginBuilder builder)
        {
            this.builder = builder;

            Orders = new OrdersCollection(builder);
            NetPositions = new PositionCollection(builder);
            Assets = new AssetsCollection(builder);

            _historyAdapter = new TradeHistoryAdapter();

            Orders.Added += OnOrderAdded;
            Orders.Replaced += OnOrderReplaced;
            Orders.Removed += OnOrderRemoved;

            NetPositions.PositionUpdated += OnPositionUpdated;
            NetPositions.PositionRemoved += OnPositionRemoved;

            Assets.AssetChanged += OnAssetsChanged;
        }

        public void Dispose()
        {
            Orders.Added -= OnOrderAdded;
            Orders.Replaced -= OnOrderReplaced;
            Orders.Removed -= OnOrderRemoved;

            NetPositions.PositionUpdated -= OnPositionUpdated;
            NetPositions.PositionRemoved -= OnPositionRemoved;

            Assets.AssetChanged -= OnAssetsChanged;
        }

        public OrdersCollection Orders { get; private set; }
        public PositionCollection NetPositions { get; private set; }
        public AssetsCollection Assets { get; private set; }
        public ITradeHistoryProvider HistoryProvider { get { return _historyAdapter.Provider; } set { _historyAdapter.Provider = value; } }

        public string Id { get; set; }
        public double Balance { get; set; }
        public string BalanceCurrency { get; set; }
        public int Leverage { get; set; }
        public AccountTypes Type { get; set; }
        public bool Isolated { get; set; }
        public string InstanceId { get; set; }

        internal void FireBalanceUpdateEvent()
        {
            builder.InvokePluginMethod(() => BalanceUpdated());
        }

        internal void FireResetEvent()
        {
            builder.InvokePluginMethod(() => Reset());
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
        TradeHistory AccountDataProvider.TradeHistory => _historyAdapter;

        public double Equity { get; set; }
        public double Margin { get; set; }
        public double MarginLevel { get; set; }
        public double Profit { get; set; }
        public double Commision { get; set; }

        #region BO

        long BL.IAccountInfo.Id => 0;
        public BO.AccountingTypes AccountingType => TickTraderToAlgo.Convert(Type);
        decimal BL.IMarginAccountInfo.Balance => (decimal)Balance;
        IEnumerable<BL.IOrderModel> BL.IAccountInfo.Orders => Enumerable.Empty<BL.IOrderModel>();
        IEnumerable<BL.IPositionModel> BL.IMarginAccountInfo.Positions => Enumerable.Empty<BL.IPositionModel>();
        IEnumerable<BL.IAssetModel> BL.ICashAccountInfo.Assets => Assets;

        void BL.IAccountInfo.LogInfo(string message)
        {
        }

        void BL.IAccountInfo.LogWarn(string message)
        {
        }

        void BL.IAccountInfo.LogError(string message)
        {
        }

        public event Action<BL.IOrderModel> OrderAdded = delegate { };
        public event Action<IEnumerable<BL.IOrderModel>> OrdersAdded { add { } remove { } }
        public event Action<BL.IOrderModel> OrderRemoved = delegate { };
        public event Action<BL.IOrderModel> OrderReplaced = delegate { };
        public event Action BalanceUpdated = delegate { };
        public event Action Reset = delegate { };
        public event Action<BL.IPositionModel, BL.PositionChageTypes> PositionChanged;
        public event Action<BL.IAssetModel, BL.AssetChangeTypes> AssetsChanged;

        #endregion

        private void OnOrderAdded(BL.IOrderModel order)
        {
            UpdateAccountInfo(() => { OrderAdded?.Invoke(order); builder.Logger.OnPrint($"DEBUG | Calculator added #{order.OrderId}"); });
        }

        private void OnOrderReplaced(BL.IOrderModel order)
        {
            UpdateAccountInfo(() => { OrderReplaced?.Invoke(order); builder.Logger.OnPrint($"DEBUG | Calculator replaced #{order.OrderId}"); });
        }

        private void OnOrderRemoved(BL.IOrderModel order)
        {
            UpdateAccountInfo(() => { OrderRemoved?.Invoke(order); builder.Logger.OnPrint($"DEBUG | Calculator added #{order.OrderId}"); });
        }

        private void OnPositionUpdated(BL.IPositionModel position)
        {
            UpdateAccountInfo(() => PositionChanged?.Invoke(position, BL.PositionChageTypes.AddedModified));
        }

        private void OnPositionRemoved(BL.IPositionModel position)
        {
            UpdateAccountInfo(() => PositionChanged?.Invoke(position, BL.PositionChageTypes.Removed));
        }

        private void OnAssetsChanged(BL.IAssetModel asset, AssetChangeType type)
        {
            UpdateAccountInfo(() => AssetsChanged?.Invoke(asset, TickTraderToAlgo.Convert(type)));
        }

        private void UpdateAccountInfo(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                builder.Logger.OnError(ex);
            }
        }
    }
}
