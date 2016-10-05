using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public class TradeBot : AlgoPlugin
    {
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnQuote(Quote quote) { }

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

        #region Logger

        protected StatusApi Status { get { return GetStatusApi(); } }

        protected void Print(string msg, params object[] parameters)
        {
            GetLogger().Print(msg, parameters);
        }

        protected void PrintError(string msg, params object[] parameters)
        {
            GetLogger().PrintError(msg, parameters);
        }

        #endregion

        #region Order Commands

        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return OpenOrderAsync(symbol, type, side, volume, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side,  double volume, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return GetTradeApi().OpenOrder(symbol, type, side, volume, price, sl, tp, comment);
        }

        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return CloseOrderAsync(orderId, volume).Result;
        }

        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return GetTradeApi().CloseOrder(orderId, volume);
        }

        public OrderCmdResult CancelOrder(string orderId)
        {
            return CancelOrderAsync(orderId).Result;
        }

        public Task<OrderCmdResult> CancelOrderAsync(string orderId)
        {
            return GetTradeApi().CancelOrder(orderId);
        }

        public OrderCmdResult ModifyOrder(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return ModifyOrderAsync(orderId, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return GetTradeApi().ModifyOrder(orderId, price, sl, tp, comment);
        }

        #endregion

        #region Order Short Commands

        public OrderCmdResult MarketSell(double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Sell, 1, volume, tp, sl, comment);
        }

        public OrderCmdResult MarketSell(string symbol, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Sell, 1, volume, tp, sl, comment);
        }

        public OrderCmdResult MarketBuy(double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, 1, volume, tp, sl, comment);
        }

        public OrderCmdResult MarketBuy(string symbol, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, OrderSide.Buy, 1, volume, tp, sl, comment);
        }

        public OrderCmdResult OpenMarketOrder(OrderSide side, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(Symbol.Name, OrderType.Market, side, 1, volume, tp, sl, comment);
        }

        public OrderCmdResult OpenMarketOrder(string symbol, OrderSide side, double volume, double? tp = null, double? sl = null, string comment = "")
        {
            return OpenOrder(symbol, OrderType.Market, side, volume, 1, tp, sl, comment);
        }

        #endregion
    }
}
