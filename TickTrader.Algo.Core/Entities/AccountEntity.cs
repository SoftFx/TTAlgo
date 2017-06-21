using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    public class AccountEntity : AccountDataProvider, BL.IMarginAccountInfo, BL.ICashAccountInfo
    {
        private PluginBuilder builder;
        private Dictionary<string, OrderFilteredCollection> bySymbolFilterCache;
        private Dictionary<string, OrderFilteredCollection> byTagFilterCache;
        private Dictionary<Predicate<Order>, OrderFilteredCollection> customFilterCache;

        public AccountEntity(PluginBuilder builder)
        {
            this.builder = builder;

            Orders = new OrdersCollection(builder);
            Assets = new AssetsCollection(builder);
        }

        public OrdersCollection Orders { get; private set; }
        public AssetsCollection Assets { get; private set; }

        public string Id { get; set; }
        public double Balance { get; set; }
        public string BalanceCurrency { get; set; }
        public int Leverage { get; set; }
        public AccountTypes Type { get; set; }

        internal void FireBalanceUpdateEvent()
        {
            builder.InvokePluginMethod(() => BalanceUpdated());
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

        public void LogInfo(string message)
        {
        }

        public void LogWarn(string message)
        {
        }

        public void LogError(string message)
        {
        }

        OrderList AccountDataProvider.Orders { get { return Orders.OrderListImpl; } }
        AssetList AccountDataProvider.Assets { get { return Assets.AssetListImpl; } }

        public double Equity { get; set; }
        public double Margin { get; set; }
        public double MarginLevel { get; set; }
        public double Profit { get; set; }
        public double Commision { get; set; }

        public NetPositionList NetPositions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #region BO

        long BL.IAccountInfo.Id => 0;
        public BO.AccountingTypes AccountingType => TickTraderToAlgo.Convert(Type);
        decimal BL.IMarginAccountInfo.Balance => (decimal)Balance;
        IEnumerable<BL.IOrderModel> BL.IAccountInfo.Orders => (IEnumerable<OrderAccessor>)Orders.OrderListImpl;
        public IEnumerable<BL.IPositionModel> Positions => Enumerable.Empty<BL.IPositionModel>();
        IEnumerable<BL.IAssetModel> BL.ICashAccountInfo.Assets => Enumerable.Empty<BL.IAssetModel>();

        public event Action<BL.IOrderModel> OrderAdded { add { Orders.Added += value; } remove { Orders.Added -= value; } }
        public event Action<IEnumerable<BL.IOrderModel>> OrdersAdded { add { } remove { } }
        public event Action<BL.IOrderModel> OrderRemoved { add { Orders.Removed += value; } remove { Orders.Removed -= value; } }
        public event Action<BL.IOrderModel> OrderReplaced { add { Orders.Replaced += value; } remove { Orders.Replaced -= value; } }
        public event Action BalanceUpdated = delegate { };
        public event Action<BL.IPositionModel, BL.PositionChageTypes> PositionChanged;
        public event Action<BL.IAssetModel, BL.AssetChangeTypes> AssetsChanged;

        #endregion
    }
}
