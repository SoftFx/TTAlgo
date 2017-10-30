using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public class TradeBot : AlgoPlugin
    {
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual Task AsyncStop() { return Task.FromResult(this); }

        /// <summary>
        /// Override this method to react on rate updates.
        /// Note: Some outdated quotes may be skipped by the Algo framework to give you most relevant ones.
        /// You can override OnRateUpdate() instead of OnQuote() to get all quotes.
        /// </summary>
        /// <param name="quote"></param>
        protected virtual void OnQuote(Quote quote) { }

        /// <summary>
        /// Extended version of OnQuote(). Provides additional information. Includes skipped quotes.
        /// </summary>
        /// <param name="update"></param>
        protected virtual void OnRateUpdate(RateUpdate update) { }

        protected bool IsStopped { get { return context.IsStopped; } }

        protected void Exit()
        {
            context.OnExit();
        }

        internal void InvokeStart()
        {
            OnStart();
        }

        internal void InvokeStop()
        {
            OnStop();
        }

        internal Task InvokeAsyncStop()
        {
            return AsyncStop();
        }

        internal void InvokeOnQuote(Quote quote)
        {
            OnQuote(quote);
        }

        internal void InvokeRateUpdate(RateUpdate update)
        {
            OnRateUpdate(update);
        }

        #region Logger

        public StatusApi Status { get { return context.StatusApi; } }

        public void Print(string msg)
        {
            context.Logger.Print(msg);
        }

        public void Print(string msg, params object[] parameters)
        {
            context.Logger.Print(msg, parameters);
        }

        public void PrintError(string msg)
        {
            context.Logger.PrintError(msg);
        }

        public void PrintError(string msg, params object[] parameters)
        {
            context.Logger.PrintError(msg, parameters);
        }

        #endregion

        #region Order Commands
        [Obsolete]
        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            return context.TradeApi.OpenOrder(false, symbol, type, side, volume, price, sl, tp, comment, options, tag).Result;
        }

        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume = null, double? price = null, double? stopPrice = null, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null, DateTime? expiration = null)
        {
            return context.TradeApi.OpenOrder(false, symbol, type, side, volume, maxVisibleVolume, price, stopPrice, sl, tp, comment, options, tag, expiration).Result;
        }

        [Obsolete]
        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side,  double volume, double price, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            return context.TradeApi.OpenOrder(true, symbol, type, side, volume, price, sl, tp, comment, options, tag);
        }

        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null, DateTime? expiration = null)
        {
            return context.TradeApi.OpenOrder(true, symbol, type, side, volume, maxVisibleVolume, price, stopPrice, sl, tp, comment, options, tag, expiration);
        }

        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return context.TradeApi.CloseOrder(false, orderId, volume).Result;
        }

        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return context.TradeApi.CloseOrder(true, orderId, volume);
        }

        public OrderCmdResult CloseOrderBy(string orderId, string byOrderId)
        {
            return context.TradeApi.CloseOrderBy(false, orderId, byOrderId).Result;
        }

        public Task<OrderCmdResult> CloseOrderByAsync(string orderId, string byOrderId)
        {
            return context.TradeApi.CloseOrderBy(true, orderId, byOrderId);
        }

        public OrderCmdResult CancelOrder(string orderId)
        {
            return context.TradeApi.CancelOrder(false, orderId).Result;
        }

        public Task<OrderCmdResult> CancelOrderAsync(string orderId)
        {
            return context.TradeApi.CancelOrder(true, orderId);
        }
        [Obsolete]
        public OrderCmdResult ModifyOrder(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return context.TradeApi.ModifyOrder(false, orderId, price, sl, tp, comment).Result;
        }

        public OrderCmdResult ModifyOrder(string orderId, double? price, double? stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", DateTime? expiration = null)
        {
            return context.TradeApi.ModifyOrder(false, orderId, price, stopPrice, maxVisibleVolume, sl, tp, comment, expiration).Result;
        }

        [Obsolete]
        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return context.TradeApi.ModifyOrder(true, orderId, price, sl, tp, comment);
        }

        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double? price, double? stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", DateTime? expiration = null)
        {
            return context.TradeApi.ModifyOrder(true, orderId, price, stopPrice, maxVisibleVolume, sl, tp, comment, expiration);
        }

        #endregion

        #region Order Short Commands

        #region Market
        public OrderCmdResult MarketSell(double volume, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Sell, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }

        public OrderCmdResult MarketSell(string symbol, double volume, double? sl = null, double? tp = null,  string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Sell, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }

        public OrderCmdResult MarketBuy(double volume, double? tp = null, double? sl = null, string comment = "", string tag = null)
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }

        public OrderCmdResult MarketBuy(string symbol, double volume, double? tp = null, double? sl = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Buy, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }

        public OrderCmdResult OpenMarketOrder(OrderSide side, double volume, double? tp = null, double? sl = null, string comment = "", string tag = null)
        {
            return OpenOrder(Symbol.Name, OrderType.Market, side, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }

        public OrderCmdResult OpenMarketOrder(string symbol, OrderSide side, double volume, double? tp = null, double? sl = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Market, side, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }
        #endregion

        #region Limit
        public OrderCmdResult LimitSell(double volume, double price, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenLimitOrder(OrderSide.Sell, Symbol.Name, volume, price, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult LimitSell(string symbol, double volume, double price, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenLimitOrder(OrderSide.Sell, symbol, volume, price, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult LimitBuy(double volume, double price, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenLimitOrder(OrderSide.Buy, Symbol.Name, volume, price, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult LimitBuy(string symbol, double volume, double price, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenLimitOrder(OrderSide.Buy, symbol, volume, price, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult OpenLimitOrder(OrderSide side, string symbol, double volume, double price, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Limit, side, volume, maxVisibleVolume, price, sl: sl, tp: tp, comment: comment, tag: tag);
        }
        #endregion

        #region Stop
        public OrderCmdResult StopSell(double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopOrder(OrderSide.Sell, Symbol.Name, volume, price, sl, tp, comment, tag);
        }

        public OrderCmdResult StopSell(string symbol, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopOrder(OrderSide.Sell, symbol, volume, price, sl, tp, comment, tag);
        }

        public OrderCmdResult StopBuy(double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopOrder(OrderSide.Buy, Symbol.Name, volume, price, sl, tp, comment, tag);
        }

        public OrderCmdResult StopBuy(string symbol, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopOrder(OrderSide.Buy, symbol, volume, price, sl, tp, comment, tag);
        }

        public OrderCmdResult OpenStopOrder(OrderSide side, string symbol, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Stop, side, volume, stopPrice: price, sl: sl, tp: tp, comment: comment, tag: tag);
        }
        #endregion

        #region StopLimit
        public OrderCmdResult StopLimitSell(double volume, double price, double stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopLimitOrder(OrderSide.Sell, Symbol.Name, volume, price, stopPrice, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult StopLimitSell(string symbol, double volume, double price, double stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopLimitOrder(OrderSide.Sell, symbol, volume, price, stopPrice, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult StopLimitBuy(double volume, double price, double stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopLimitOrder(OrderSide.Buy, Symbol.Name, volume, price, stopPrice, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult StopLimitBuy(string symbol, double volume, double price, double stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenStopLimitOrder(OrderSide.Buy, symbol, volume, price, stopPrice, maxVisibleVolume, sl, tp, comment, tag);
        }

        public OrderCmdResult OpenStopLimitOrder(OrderSide side, string symbol, double volume, double price, double stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.StopLimit, side, volume, maxVisibleVolume, price, stopPrice, sl: sl, tp: tp, comment: comment, tag: tag);
        }
        #endregion

        #region IoC
        public OrderCmdResult OpenLimitIoC(string symbol, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Limit, side, volume, null, price, null, sl, tp, comment, OrderExecOptions.ImmediateOrCancel, tag);
        }

        public Task<OrderCmdResult> OpenLimitIoCAsync(string symbol, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrderAsync(symbol, OrderType.Limit, side, volume, null, price, null, sl, tp, comment, OrderExecOptions.ImmediateOrCancel, tag);
        }
        #endregion

        #endregion
    }
}
