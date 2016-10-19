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

        public StatusApi Status { get { return GetStatusApi(); } }

        public void Print(string msg, params object[] parameters)
        {
            GetLogger().Print(msg, parameters);
        }

        public void PrintError(string msg, params object[] parameters)
        {
            GetLogger().PrintError(msg, parameters);
        }

        #endregion

        #region Order Commands

        public OrderCmdResult OpenOrder(string symbol, OrderType type, OrderSide side, double volume, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return GetTradeApi().OpenOrder(false, symbol, type, side, volume, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> OpenOrderAsync(string symbol, OrderType type, OrderSide side,  double volume, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return GetTradeApi().OpenOrder(true, symbol, type, side, volume, price, sl, tp, comment);
        }

        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return GetTradeApi().CloseOrder(false, orderId, volume).Result;
        }

        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return GetTradeApi().CloseOrder(true, orderId, volume);
        }

        public OrderCmdResult CancelOrder(string orderId)
        {
            return GetTradeApi().CancelOrder(false, orderId).Result;
        }

        public Task<OrderCmdResult> CancelOrderAsync(string orderId)
        {
            return GetTradeApi().CancelOrder(true, orderId);
        }

        public OrderCmdResult ModifyOrder(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return GetTradeApi().ModifyOrder(false, orderId, price, sl, tp, comment).Result;
        }

        public Task<OrderCmdResult> ModifyOrderAsync(string orderId, double price, double? sl = null, double? tp = null, string comment = "")
        {
            return GetTradeApi().ModifyOrder(true, orderId, price, sl, tp, comment);
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
