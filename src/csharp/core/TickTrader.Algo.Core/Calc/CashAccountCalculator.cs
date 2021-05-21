using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Calc
{
    public class CashAccountCalculator : IDisposable
    {
        private readonly ICashAccountInfo2 account;
        private readonly Dictionary<string, IAssetInfo> assets = new Dictionary<string, IAssetInfo>();
        private MarketStateBase market;
        private Action<Exception, string> _onLogError;

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

        public CashAccountCalculator(ICashAccountInfo2 infoProvider, MarketStateBase market, Action<Exception, string> onLogError)
        {
            if (infoProvider == null)
                throw new ArgumentNullException("infoProvider");

            if (market == null)
                throw new ArgumentNullException("market");

            this.account = infoProvider;
            this.market = market;
            _onLogError = onLogError;

            if (this.account.Assets != null)
                this.account.Assets.ForEach(a => AddRemoveAsset(a, AssetChangeType.Added));
            this.account.AssetsChanged += AddRemoveAsset;
            this.AddOrdersBunch(this.account.Orders);
            this.account.OrderAdded += AddOrder;
            this.account.OrderRemoved += RemoveOrder;
            this.account.OrdersAdded += AddOrdersBunch;
            //this.account.OrderReplaced += UpdateOrder;
        }

        //public bool HasSufficientMarginToOpenOrder(IOrderModel2 order, decimal? marginMovement)
        //{
        //    var symbol = order.SymbolInfo ?? throw CreateNoSymbolException(order.Symbol);
        //    return HasSufficientMarginToOpenOrder(order.Type, order.Side, symbol, marginMovement);
        //}

        public bool HasSufficientMarginToOpenOrder(OrderInfo.Types.Type type, OrderInfo.Types.Side side, ISymbolInfo symbol, decimal? marginMovement)
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

            var marginAsset = GetMarginAsset(symbol, side);
            if (marginAsset == null || marginAsset.Amount == 0)
                throw new NotEnoughMoneyException($"Asset {GetMarginAssetCurrency(symbol, side)} is empty.");

            if (marginMovement.Value > marginAsset.FreeAmount)
                throw new NotEnoughMoneyException($"{marginAsset}, Margin={marginAsset.Margin}, MarginMovement={marginMovement.Value}.");

            return true;
        }

        public static decimal CalculateMarginFactor(OrderInfo.Types.Type type, ISymbolInfo symbol, bool isHidden)
        {
            decimal combinedMarginFactor = 1.0M;
            if (type == OrderInfo.Types.Type.Stop || type == OrderInfo.Types.Type.StopLimit)
                combinedMarginFactor *= (decimal)symbol.StopOrderMarginReduction;
            else if (type == OrderInfo.Types.Type.Limit && isHidden)
                combinedMarginFactor *= (decimal)symbol.HiddenLimitOrderMarginReduction;
            return combinedMarginFactor;
        }

        public static decimal CalculateMargin(IOrderCalcInfo order, ISymbolInfo symbol)
        {
            return CalculateMargin(order.Type, order.RemainingAmount, order.Price, order.StopPrice, order.Side, symbol, order.IsHidden);
        }

        public static decimal CalculateMargin(OrderInfo.Types.Type type, decimal amount, double? orderPrice, double? orderStopPrice, OrderInfo.Types.Side side, ISymbolInfo symbol, bool isHidden)
        {
            decimal combinedMarginFactor = CalculateMarginFactor(type, symbol, isHidden);

            double price = ((type == OrderInfo.Types.Type.Stop) || (type == OrderInfo.Types.Type.StopLimit)) ? orderStopPrice.Value : orderPrice.Value;

            if (side == OrderInfo.Types.Side.Buy)
                return combinedMarginFactor * amount * (decimal)price;
            else
                return combinedMarginFactor * amount;
        }

        public IAssetInfo GetMarginAsset(ISymbolInfo symbol, OrderInfo.Types.Side side)
        {
            //if (order.MarginCurrency == null || order.ProfitCurrency == null)
            //    throw new MarketConfigurationException("Order must have both margin & profit currencies specified.");

            return assets.GetOrDefault(GetMarginAssetCurrency(symbol, side));
        }

        public string GetMarginAssetCurrency(ISymbolInfo smb, OrderInfo.Types.Side side)
        {
            //var symbol = smb ?? throw CreateNoSymbolException(smb.Name);

            return (side == Domain.OrderInfo.Types.Side.Buy) ? smb.ProfitCurrency : smb.MarginCurrency;
        }

        public void AddRemoveAsset(IAssetInfo asset, AssetChangeType changeType)
        {
            if (changeType == AssetChangeType.Added)
                this.assets.Add(asset.Currency, asset);
            else if (changeType == AssetChangeType.Removed)
                this.assets.Remove(asset.Currency);
            else if (changeType == AssetChangeType.Updated)
            {
                var oldAsset = this.assets[asset.Currency];
                this.assets[asset.Currency] = asset;
                asset.Margin = oldAsset.Margin;
            }
        }

        public void AddOrder(IOrderCalcInfo order)
        {
            try
            {
                var symbol = order.SymbolInfo;
                if (symbol == null) //can be caused by server misconfiguration
                {
                    _onLogError?.Invoke(null, $"{nameof(CashAccountCalculator)} failed to add order: symbol not found. {order?.GetSnapshotString()}");
                    return;
                }

                order.CashMargin = CalculateMargin(order, symbol);
                //order.Margin = margin;
                //OrderLightClone clone = new OrderLightClone(order);
                //orders.Add(order.OrderId, clone);

                var marginAsset = GetMarginAsset(symbol, order.Side);
                if (marginAsset != null)
                    marginAsset.Margin += order.CashMargin;

                order.EssentialsChanged += OnOrderChanged;
            }
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(CashAccountCalculator)} failed to add order. {order?.GetSnapshotString()}");
            }
        }

        public void OnOrderChanged(OrderEssentialsChangeArgs args)
        {
            try
            {
                var order = args.Order;
                var symbol = order.SymbolInfo;
                if (symbol == null)
                {
                    // theoretically impossible
                    _onLogError?.Invoke(null, $"{nameof(CashAccountCalculator)} failed to handle order update: symbol not found. {order?.GetSnapshotString()}");
                    order.EssentialsChanged -= OnOrderChanged;
                    return;
                }

                //OrderLightClone clone = GetOrderOrThrow(order.OrderId);
                var marginAsset = GetMarginAsset(symbol, order.Side);
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
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(CashAccountCalculator)} failed to handle order update. {args.Order?.GetSnapshotString()}");
            }
        }

        public void AddOrdersBunch(IEnumerable<IOrderCalcInfo> bunch)
        {
            bunch.ForEach(AddOrder);
        }

        public void RemoveOrder(IOrderCalcInfo order)
        {
            try
            {
                //OrderLightClone clone = GetOrderOrThrow(order.OrderId);
                //orders.Remove(order.OrderId);

                var symbol = order.SymbolInfo;
                if (symbol == null) //can be caused by server misconfiguration
                {
                    _onLogError?.Invoke(null, $"{nameof(CashAccountCalculator)} failed to remove order: symbol not found. {order?.GetSnapshotString()}");
                    return;
                }

                var marginAsset = GetMarginAsset(symbol, order.Side);
                if (marginAsset != null)
                    marginAsset.Margin -= order.CashMargin;

                order.EssentialsChanged -= OnOrderChanged;
            }
            catch (Exception ex)
            {
                _onLogError?.Invoke(ex, $"{nameof(CashAccountCalculator)} failed to remove order. {order?.GetSnapshotString()}");
            }
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
    }
}
