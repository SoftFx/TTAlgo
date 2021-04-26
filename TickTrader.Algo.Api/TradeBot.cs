using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public class TradeBot : AlgoPlugin
    {
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual Task AsyncStop() { return Task.FromResult(this); }
        protected virtual double GetOptimizationMetric() { return context.DefaultOptimizationMetric; }

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

        /// <summary>
        /// Override this method to react to market changes. Update frequency is determined by user on setup.
        /// Method might be called after each quote or once in timeframe period after current bar is closed.
        /// </summary>
        protected virtual void OnModelTick() { }

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

        internal double InvokeGetMetric()
        {
            return GetOptimizationMetric();
        }

        internal void InvokeOnModelTick()
        {
            OnModelTick();
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
            return context.TradeApi.OpenOrder(false, OpenOrderRequest.Template.Create().WithParams(symbol, side, type, volume, price, null, null, tp, sl, comment, options, tag, null, null).MakeRequest()).Result;
        }

        [Obsolete]
        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume = null, double? price = null, double? stopPrice = null, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null, DateTime? expiration = null)
        {
            return context.TradeApi.OpenOrder(false, OpenOrderRequest.Template.Create().WithParams(symbol, side, type, volume, price, stopPrice, maxVisibleVolume, tp, sl, comment, options, tag, expiration, null).MakeRequest()).Result;
        }

        public OrderCmdResult OpenOrder(OpenOrderRequest request)
        {
            return context.TradeApi.OpenOrder(false, request).Result;
        }

        [Obsolete]
        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            return context.TradeApi.OpenOrder(true, OpenOrderRequest.Template.Create().WithParams(symbol, side, type, volume, price, null, null, tp, sl, comment, options, tag, null, null).MakeRequest());
        }

        [Obsolete]
        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side, double volume, double? maxVisibleVolume, double? price, double? stopPrice, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null, DateTime? expiration = null)
        {
            return context.TradeApi.OpenOrder(true, OpenOrderRequest.Template.Create().WithParams(symbol, side, type, volume, price, stopPrice, maxVisibleVolume, tp, sl, comment, options, tag, expiration, null).MakeRequest());
        }

        public Task<OrderCmdResult> OpenOrderAsync(OpenOrderRequest request)
        {
            return context.TradeApi.OpenOrder(true, request);
        }

        public OrderCmdResult CloseNetPosition(CloseNetPositionRequest request)
        {
            return context.TradeApi.CloseNetPosition(false, request).Result;
        }

        public Task<OrderCmdResult> CloseNetPositionAsync(CloseNetPositionRequest request)
        {
            return context.TradeApi.CloseNetPosition(true, request);
        }

        public OrderCmdResult CloseOrder(CloseOrderRequest request)
        {
            return context.TradeApi.CloseOrder(false, request).Result;
        }

        [Obsolete]
        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return context.TradeApi.CloseOrder(false, CloseOrderRequest.Template.Create().WithParams(orderId, volume).MakeRequest()).Result;
        }

        public Task<OrderCmdResult> CloseOrderAsync(CloseOrderRequest request)
        {
            return context.TradeApi.CloseOrder(true, request);
        }

        [Obsolete]
        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return context.TradeApi.CloseOrder(true, CloseOrderRequest.Template.Create().WithParams(orderId, volume).MakeRequest());
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

        public OrderCmdResult ModifyOrder(ModifyOrderRequest request)
        {
            return context.TradeApi.ModifyOrder(false, request).Result;
        }

        [Obsolete]
        public OrderCmdResult ModifyOrder(string orderId, double price, double? sl = null, double? tp = null, string comment = null)
        {
            return context.TradeApi.ModifyOrder(false, ModifyOrderRequest.Template.Create().WithParams(orderId, price, tp, sl).WithComment(comment).MakeRequest()).Result;
        }

        [Obsolete]
        public OrderCmdResult ModifyOrder(string orderId, double? price, double? stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = null, DateTime? expiration = null, double? volume = null, OrderExecOptions? options = null)
        {
            return context.TradeApi.ModifyOrder(false, ModifyOrderRequest.Template.Create().WithParams(orderId, price, stopPrice, volume, maxVisibleVolume, tp, sl, comment, expiration, options, null).MakeRequest()).Result;
        }

        public Task<OrderCmdResult> ModifyOrderAsync(ModifyOrderRequest request)
        {
            return context.TradeApi.ModifyOrder(true, request);
        }

        [Obsolete]
        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double price, double? sl = null, double? tp = null, string comment = null)
        {
            return context.TradeApi.ModifyOrder(true, ModifyOrderRequest.Template.Create().WithParams(orderId, price, tp, sl).WithComment(comment).MakeRequest());
        }


        [Obsolete]
        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double? price, double? stopPrice, double? maxVisibleVolume = null, double? sl = null, double? tp = null, string comment = null, DateTime? expiration = null, double? volume = null, OrderExecOptions? options = null)
        {
            return context.TradeApi.ModifyOrder(true, ModifyOrderRequest.Template.Create().WithParams(orderId, price, stopPrice, volume, maxVisibleVolume, tp, sl, comment, expiration, options, null).MakeRequest());
        }

        #endregion

        #region Order Short Commands

        #region Market
        public OrderCmdResult MarketSell(double volume, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Sell, volume, tp: tp, sl: sl, comment: comment, tag: tag);
        }

        public OrderCmdResult MarketSell(string symbol, double volume, double? sl = null, double? tp = null, string comment = "", string tag = null)
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

        #region Setup

        /// <summary>
        /// Changes the size of input buffers. This method can be called only from Init().
        /// </summary>
        /// <param name="newSize"></param>
        public void SetInputSize(int newSize)
        {
            context.SetFeedBufferSize(newSize);
        }

        #endregion
    }

    public interface Timer : IDisposable
    {
        void Change(int periodMs);
        void Change(TimeSpan period);
    }
}
