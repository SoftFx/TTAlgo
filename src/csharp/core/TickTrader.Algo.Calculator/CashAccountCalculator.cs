using System;
using System.Collections.Generic;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public class CashAccountCalculator : IDisposable
    {
        private readonly ICashAccountInfo2 account;
        private readonly Dictionary<string, IAssetInfo> assets = new Dictionary<string, IAssetInfo>();
        private Action<Exception, string> _onLogError;

        public CashAccountCalculator(ICashAccountInfo2 infoProvider, AlgoMarketState market, Action<Exception, string> onLogError)
        {
            if (infoProvider == null)
                throw new ArgumentNullException("infoProvider");

            //if (market == null)
            //    throw new ArgumentNullException("market");

            this.account = infoProvider;
            //this.market = market;
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

        public bool HasSufficientMarginToOpenOrder(OrderInfo.Types.Type type, OrderInfo.Types.Side side, ISymbolInfo symbol, double? marginMovement)
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

        public static double CalculateMarginFactor(OrderInfo.Types.Type type, ISymbolInfo symbol, bool isHidden)
        {
            double combinedMarginFactor = 1.0;
            if (type == OrderInfo.Types.Type.Stop || type == OrderInfo.Types.Type.StopLimit)
                combinedMarginFactor *= symbol.StopOrderMarginReduction;
            else if (type == OrderInfo.Types.Type.Limit && isHidden)
                combinedMarginFactor *= symbol.HiddenLimitOrderMarginReduction;
            return combinedMarginFactor;
        }

        public static double CalculateMargin(IOrderCalcInfo order, ISymbolInfo symbol)
        {
            return CalculateMargin(order.Type, order.RemainingAmount, order.Price, order.StopPrice, order.Side, symbol, order.IsHidden, order.Slippage);
        }

        public static double CalculateMargin(OrderInfo.Types.Type type, double amount, double? orderPrice, double? orderStopPrice, OrderInfo.Types.Side side, ISymbolInfo symbol, bool isHidden, double? slippage)
        {
            double combinedMarginFactor = CalculateMarginFactor(type, symbol, isHidden);
            if (side.IsSell())
                return combinedMarginFactor * amount;
            else
            {
                if (type != OrderInfo.Types.Type.Stop)
                    return combinedMarginFactor * amount * orderPrice.Value; // StopLimit orders should use limit price for locked amount
                else
                {
                    double slippageCoeff = 1.0;
                    if (slippage == null)
                        slippage = symbol.Slippage;
                    // expecting percent slippage
                    if (slippage.Value < 1 && slippage.Value > 0)
                        slippageCoeff = 1.0 + slippage.Value;

                    return combinedMarginFactor * amount * orderStopPrice.Value * slippageCoeff;
                }
            }
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
                this.assets.TryGetValue(asset.Currency, out var oldAsset);
                this.assets[asset.Currency] = asset;
                asset.Margin = oldAsset?.Margin ?? 0;
            }
        }

        public void AddOrder(IOrderCalcInfo order)
        {
            if (order.IgnoreCalculation)
                return;

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

                if (order.IgnoreCalculation)
                    return;

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
            if (order.IgnoreCalculation)
                return;

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
