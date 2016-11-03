using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public class TradeBot : AlgoPlugin
    {
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }

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

        protected void Exit()
        {
            Context.OnExit();
        }

        internal void InvokeStart()
        {
            OnStart();
        }

        internal void InvokeStop()
        {
            OnStop();
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

        public StatusApi Status { get { return Context.StatusApi; } }

        public void Print(string msg, params object[] parameters)
        {
            Context.Logger.Print(msg, parameters);
        }

        public void PrintError(string msg, params object[] parameters)
        {
            Context.Logger.PrintError(msg, parameters);
        }

        #endregion

        #region Order Commands

        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return Context.TradeApi.OpenOrder(false, symbol, type, side, volume, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side,  double volume, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return Context.TradeApi.OpenOrder(true, symbol, type, side, volume, price, sl, tp, comment);
        }

        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return Context.TradeApi.CloseOrder(false, orderId, volume).Result;
        }

        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return Context.TradeApi.CloseOrder(true, orderId, volume);
        }

        public OrderCmdResult CancelOrder(string orderId)
        {
            return Context.TradeApi.CancelOrder(false, orderId).Result;
        }

        public Task<OrderCmdResult> CancelOrderAsync(string orderId)
        {
            return Context.TradeApi.CancelOrder(true, orderId);
        }

        public OrderCmdResult ModifyOrder(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return Context.TradeApi.ModifyOrder(false, orderId, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return Context.TradeApi.ModifyOrder(true, orderId, price, sl, tp, comment);
        }

        #endregion

        #region Order Short Commands

        public OrderCmdResult MarketSell(double volume, double? sl = null, double? tp = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Sell, volume, 1, tp, sl, comment);
        }

        public OrderCmdResult MarketSell(string symbol, double volume, double? sl = null, double? tp = null,  string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Sell, volume, 1, tp, sl, comment);
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

        #endregion
    }
}
