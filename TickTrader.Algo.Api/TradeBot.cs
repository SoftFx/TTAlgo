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
            GetLogger().WriteLog(msg, parameters);
        }

        protected void UpdateStatus(string status)
        {
            GetLogger().UpdateStatus(status);
        }

        #endregion

        #region Order Commands

        public OrderCmdResult MarketBuy(string symbolCode, double price, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return GetTradeApi().OpenOrder(
                new OpenOrdeRequest()
                {
                    Type = OrderTypes.Market,
                    Side = OrderSides.Buy,
                    Price = price,
                    Volume = volume,
                    StopLoss = stopLoss,
                    TaskProfit = takeProfit,
                    Comment = comment
                });
        }

        public OrderCmdResult MarketSell(string symbolCode, double price, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return GetTradeApi().OpenOrder(
                new OpenOrdeRequest()
                {
                    Type = OrderTypes.Market,
                    Side = OrderSides.Sell,
                    Price = price,
                    Volume = volume,
                    StopLoss = stopLoss,
                    TaskProfit = takeProfit,
                    Comment = comment
                });
        }

        public OrderCmdResult OpenMarketOrder(string symbolCode, OrderSides side, double price, OrderVolume volume, double? stopLoss = null, double? takeProfit = null, string comment = null)
        {
            return GetTradeApi().OpenOrder(
                new OpenOrdeRequest()
                {
                    Type = OrderTypes.Market,
                    Side = side,
                    Price = price,
                    Volume = volume,
                    StopLoss = stopLoss,
                    TaskProfit = takeProfit,
                    Comment = comment
                });
        }

        #endregion
    }
}
