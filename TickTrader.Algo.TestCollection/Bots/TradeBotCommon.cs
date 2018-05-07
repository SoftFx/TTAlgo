using System;
using System.ComponentModel;
using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public abstract class TradeBotCommon : TradeBot
    {
        public string ToObjectPropertiesString<T>(T obj)
        {
            var sb = new StringBuilder();
            sb.PrintPropertiesColumn(obj);
            return sb.ToString();
        }

        public string ToObjectPropertiesString<T>(string name, T obj)
        {
            var sb = new StringBuilder();
            sb.PrintPropertiesColumn(name, obj);
            return sb.ToString();
        }

        protected double GetCurrentPrice(BarPriceType type, int timeoutMs = 1000)
        {
            return GetPrice(type);
        }

        protected double GetCurrentPrice(OrderSide side)
        {
            return GetCurrentPrice(side == OrderSide.Buy ? BarPriceType.Ask : BarPriceType.Bid);
        }

        private double GetPrice(BarPriceType type)
        {
            return type == BarPriceType.Ask ? Symbol.Ask : Symbol.Bid;
        }
    }
}