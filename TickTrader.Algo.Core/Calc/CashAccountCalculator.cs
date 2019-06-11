using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core.Calc
{
    public class CashAccountCalculator : IDisposable
    {
        private readonly ICashAccountInfo2 account;
        private readonly Dictionary<string, IAssetModel> assets = new Dictionary<string, IAssetModel>();
        private MarketStateBase market;

        public MarketStateBase Market
        {
            get { return market; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), @"Market property cannot be null.");

                if (ReferenceEquals(market, value))
                    return;

                market = value;
            }
        }

        public CashAccountCalculator(ICashAccountInfo2 infoProvider, MarketStateBase market)
        {
            if (infoProvider == null)
                throw new ArgumentNullException("infoProvider");

            if (market == null)
                throw new ArgumentNullException("market");

            this.account = infoProvider;
            this.market = market;

            if (this.account.Assets != null)
                this.account.Assets.Foreach2(a => AddRemoveAsset(a, AssetChangeTypes.Added));
            this.account.AssetsChanged += AddRemoveAsset;
            this.AddOrdersBunch(this.account.Orders);
            this.account.OrderAdded += AddOrder;
            this.account.OrderRemoved += RemoveOrder;
            this.account.OrdersAdded += AddOrdersBunch;
            //this.account.OrderReplaced += UpdateOrder;
        }

        public bool HasSufficientMarginToOpenOrder(IOrderModel2 order, decimal? marginMovement)
        {
            var symbol = order.SymbolInfo ?? throw CreateNoSymbolException(order.Symbol);
            return HasSufficientMarginToOpenOrder(order.Type, order.Side, symbol, marginMovement);
        }

        public bool HasSufficientMarginToOpenOrder(OrderTypes type, OrderSides side, SymbolAccessor symbol, decimal? marginMovement)
        {
            //if (order == null)
            //    throw new ArgumentNullException("order");

            //if (type != OrderTypes.Limit && type != OrderTypes.StopLimit)
            //    throw new ArgumentException("Invalid Order Type", "order");

            //if (type == OrderTypes.Stop || type == OrderTypes.StopLimit)
            //{
            //    if (stopPrice == null || stopPrice <= 0)
            //        throw new ArgumentException("Invalid Stop Price", "order");
            //}

            //if (type != OrderTypes.Stop)
            //{
            //    if (price == null || price <= 0)
            //        throw new ArgumentException("Invalid Price", "order");
            //}

            //if (order.Amount <= 0)
            //    throw new ArgumentException("Invalid Amount", "order");

            if (marginMovement == null)
                throw new MarginNotCalculatedException("Provided order must have calculated Margin.");

            IAssetModel marginAsset = GetMarginAsset(symbol, side);
            if (marginAsset == null || marginAsset.Amount == 0)
                throw new NotEnoughMoneyException($"Asset {GetMarginAssetCurrency(symbol, side)} is empty.");

            if (marginMovement.Value > marginAsset.FreeAmount)
                throw new NotEnoughMoneyException($"{marginAsset}, Margin={marginAsset.Margin}, MarginMovement={marginMovement.Value}.");

            return true;
        }

        public static decimal CalculateMarginFactor(OrderTypes type, SymbolAccessor symbol, bool isHidden)
        {
            decimal combinedMarginFactor = 1.0M;
            if (type == OrderTypes.Stop || type == OrderTypes.StopLimit)
                combinedMarginFactor *= (decimal)symbol.StopOrderMarginReduction;
            else if (type == OrderTypes.Limit && isHidden)
                combinedMarginFactor *= (decimal)symbol.HiddenLimitOrderMarginReduction;
            return combinedMarginFactor;
        }

        public static decimal CalculateMargin(IOrderModel2 order, SymbolAccessor symbol)
        {
            return CalculateMargin(order.Type, order.RemainingAmount, order.Price, order.StopPrice, order.Side, symbol, order.IsHidden);
        }

        public static decimal CalculateMargin(OrderTypes type, double remAmount, double? orderPrice, double? orderStopPrice, OrderSides side, SymbolAccessor symbol, bool isHidden)
        {
            decimal combinedMarginFactor = CalculateMarginFactor(type, symbol, isHidden);

            decimal amount = (decimal)remAmount;
            double price = ((type == OrderTypes.Stop) || (type == OrderTypes.StopLimit)) ? orderStopPrice.Value : orderPrice.Value;

            if (side == OrderSides.Buy)
                return combinedMarginFactor * amount * (decimal)price;
            else
                return combinedMarginFactor * amount;
        }

        public IAssetModel GetMarginAsset(IOrderModel2 order)
        {
            //if (order.MarginCurrency == null || order.ProfitCurrency == null)
            //    throw new MarketConfigurationException("Order must have both margin & profit currencies specified.");

            var symbol = order.SymbolInfo ?? throw CreateNoSymbolException(order.Symbol);
            return assets.GetOrDefault(GetMarginAssetCurrency(symbol, order.Side));
        }

        public IAssetModel GetMarginAsset(SymbolAccessor symbol, OrderSides side)
        {
            //if (order.MarginCurrency == null || order.ProfitCurrency == null)
            //    throw new MarketConfigurationException("Order must have both margin & profit currencies specified.");

            return assets.GetOrDefault(GetMarginAssetCurrency(symbol, side));
        }

        public string GetMarginAssetCurrency(SymbolAccessor smb, OrderSides side)
        {
            //var symbol = smb ?? throw CreateNoSymbolException(smb.Name);

            return (side == OrderSides.Buy) ? smb.ProfitCurrency : smb.MarginCurrency;
        }

        public void AddRemoveAsset(IAssetModel asset, AssetChangeTypes changeType)
        {
            if (changeType == AssetChangeTypes.Added)
                this.assets.Add(asset.Currency, asset);
            else if (changeType == AssetChangeTypes.Removed)
                this.assets.Remove(asset.Currency);
            else if (changeType == AssetChangeTypes.Replaced)
            {
                var oldAsset = this.assets[asset.Currency];
                this.assets[asset.Currency] = asset;
                asset.Margin = oldAsset.Margin;
            }
        }

        public void AddOrder(IOrderModel2 order)
        {
            var symbol = order.SymbolInfo ?? throw CreateNoSymbolException(order.Symbol);
            order.CashMargin = CalculateMargin(order, symbol);
            //order.Margin = margin;
            //OrderLightClone clone = new OrderLightClone(order);
            //orders.Add(order.OrderId, clone);

            IAssetModel marginAsset = GetMarginAsset(order);
            if (marginAsset != null)
                marginAsset.Margin += order.CashMargin;

            order.EssentialsChanged += OnOrderChanged;
        }

        public void OnOrderChanged(OrderEssentialsChangeArgs args)
        {
            var order = args.Order;
            var symbol = order.SymbolInfo ?? throw CreateNoSymbolException(order.Symbol);
            //OrderLightClone clone = GetOrderOrThrow(order.OrderId);
            IAssetModel marginAsset = GetMarginAsset(order);
            marginAsset.Margin -= order.CashMargin;
            order.CashMargin = CalculateMargin(order, symbol);
            marginAsset.Margin += order.CashMargin;

            //OrderLightClone newClone = new OrderLightClone(order);
            //orders[order.OrderId] = newClone;

            //if (clone.OrderModelRef != order) // resubscribe if order model is replaced
            //{
            //    clone.OrderModelRef.EssentialParametersChanged -= UpdateOrder;
            //    order.EssentialParametersChanged += UpdateOrder;
            //}
        }

        public void AddOrdersBunch(IEnumerable<IOrderModel2> bunch)
        {
            bunch.Foreach2(AddOrder);
        }

        public void RemoveOrder(IOrderModel2 order)
        {
            //OrderLightClone clone = GetOrderOrThrow(order.OrderId);
            //orders.Remove(order.OrderId);

            IAssetModel marginAsset = GetMarginAsset(order);
            if (marginAsset != null)
                marginAsset.Margin -= order.CashMargin;

            order.EssentialsChanged -= OnOrderChanged;
        }

        //OrderLightClone GetOrderOrThrow(long orderId)
        //{
        //    OrderLightClone clone;
        //    if (!orders.TryGetValue(orderId, out clone))
        //        throw new InvalidOperationException("Order Not Found: " + orderId);
        //    return clone;
        //}

        public void Dispose()
        {
            this.account.AssetsChanged -= AddRemoveAsset;
            this.account.OrderAdded -= AddOrder;
            this.account.OrderRemoved -= RemoveOrder;
            this.account.OrdersAdded -= AddOrdersBunch;
            //this.account.OrderReplaced -= UpdateOrder;

            foreach (var order in account.Orders)
            {
                //orders.Remove(order.OrderId);
                order.EssentialsChanged -= OnOrderChanged;
            }
        }

        private Exception CreateNoSymbolException(string smbName)
        {
            return new MarketConfigurationException("Symbol not found: " + smbName);
        }
    }
}
