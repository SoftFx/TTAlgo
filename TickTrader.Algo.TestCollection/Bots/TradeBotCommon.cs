using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public abstract class TradeBotCommon : TradeBot
    {
        public string ToObjectPropertiesString(object obj)
        {
            var sb = new StringBuilder();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var pNname = descriptor.Name;
                var pValue = descriptor.GetValue(obj);
                sb.AppendLine($"{pNname} = {pValue}");
            }
            return sb.ToString();
        }

        public string ToObjectPropertiesString(string name, object obj)
        {
            var sb = new StringBuilder();
            sb.AppendLine($" ------------ {name} ------------");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                var pNname = descriptor.Name;
                var pValue = descriptor.GetValue(obj);
                sb.AppendLine($"{pNname} = {pValue}");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public string ToObjectPropertiesString(string name, Type type, object obj)
        {
            var sb = new StringBuilder();
            sb.AppendLine($" ------------ {name} ------------");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(type))
            {
                var pNname = descriptor.Name;
                var pValue = descriptor.GetValue(obj);
                sb.AppendLine($"{pNname} = {pValue}");
            }
            sb.AppendLine();
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