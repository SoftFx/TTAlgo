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

        public void Print(string msg, params object[] parameters)
        {
            context.Logger.Print(msg, parameters);
        }

        public void PrintError(string msg, params object[] parameters)
        {
            context.Logger.PrintError(msg, parameters);
        }

        #endregion

        #region Order Commands

        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            return context.TradeApi.OpenOrder(false, symbol, type, side, volume, price, sl, tp, comment, options, tag).Result;
        }

        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side,  double volume, double price, double? sl = null, double? tp = null, string comment = "", OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            return context.TradeApi.OpenOrder(true, symbol, type, side, volume, price, sl, tp, comment, options, tag);
        }

        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return context.TradeApi.CloseOrder(false, orderId, volume).Result;
        }

        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return context.TradeApi.CloseOrder(true, orderId, volume);
        }

        public OrderCmdResult CancelOrder(string orderId)
        {
            return context.TradeApi.CancelOrder(false, orderId).Result;
        }

        public Task<OrderCmdResult> CancelOrderAsync(string orderId)
        {
            return context.TradeApi.CancelOrder(true, orderId);
        }

        public OrderCmdResult ModifyOrder(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return context.TradeApi.ModifyOrder(false, orderId, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return context.TradeApi.ModifyOrder(true, orderId, price, sl, tp, comment);
        }

        #endregion

        #region Order Short Commands

        public OrderCmdResult MarketSell(double volume, double? sl = null, double? tp = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Sell, volume, 1, tp, sl, comment);
        }

        public OrderCmdResult MarketSell(string symbol, double volume, double? sl = null, double? tp = null,  string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Sell, volume, 1, sl, tp, comment);
        }

        public OrderCmdResult MarketBuy(double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, volume, 1, tp, sl, comment);
        }

        public OrderCmdResult MarketBuy(string symbol, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Buy, volume, 1, tp, sl, comment);
        }

        public OrderCmdResult OpenMarketOrder(OrderSide side, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, side, volume, 1, tp, sl, comment);
        }

        public OrderCmdResult OpenMarketOrder(string symbol, OrderSide side, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, side, volume, 1, tp, sl, comment);
        }

        public OrderCmdResult OpenLimitIoC(string symbol, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrder(symbol, OrderType.Limit, side, volume, price, sl, tp, comment, OrderExecOptions.ImmediateOrCancel, tag);
        }

        public Task<OrderCmdResult> OpenLimitIoCAsync(string symbol, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "", string tag = null)
        {
            return OpenOrderAsync(symbol, OrderType.Limit, side, volume, price, sl, tp, comment, OrderExecOptions.ImmediateOrCancel, tag);
        }

        #endregion
    }
}
