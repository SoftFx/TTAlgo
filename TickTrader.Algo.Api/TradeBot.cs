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

        protected void Print(string msg, params object[] parameters)
        {
            GetLogger().Print(msg, parameters);
        }

        protected void UpdateStatus(string status)
        {
            GetLogger().UpdateStatus(status);
        }

        #endregion

        #region Order Commands

        public OrderCmdResult MarketBuy(string symbolCode, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return MarketBuyAsync(symbolCode, volume, stopLoss, takeProfit, comment).Result;
        }

        public Task<OrderCmdResult> MarketBuyAsync(string symbolCode, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return GetTradeApi().OpenMarketOrder(symbolCode, OrderSides.Buy, volume, stopLoss, takeProfit, comment);
        }

        public OrderCmdResult MarketSell(string symbolCode, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return MarketSellAsync(symbolCode, volume, stopLoss, takeProfit, comment).Result;
        }

        public Task<OrderCmdResult> MarketSellAsync(string symbolCode, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return GetTradeApi().OpenMarketOrder(symbolCode, OrderSides.Sell, volume, stopLoss, takeProfit, comment);
        }

        public OrderCmdResult OpenMarketOrder(string symbolCode, OrderSides side, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return OpenMarketOrderAsync(symbolCode, side, volume, stopLoss, takeProfit, comment).Result;
        }

        public Task<OrderCmdResult> OpenMarketOrderAsync(string symbolCode, OrderSides side, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return GetTradeApi().OpenMarketOrder(symbolCode, side, volume, stopLoss, takeProfit, comment);
        }

        public Task<OrderCmdResult> CloseOrderAsync(string orderId, double? volume = null)
        {
            return GetTradeApi().CloseOrder(orderId, volume);
        }

        public OrderCmdResult CloseOrder(string orderId, double? volume = null)
        {
            return CloseOrderAsync(orderId, volume).Result;
        }

        #endregion
    }
}
