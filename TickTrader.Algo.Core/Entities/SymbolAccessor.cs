using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class SymbolAccessor : Api.Symbol
    {
        private SymbolEntity entity;
        private IPluginSubscriptionHandler handler;

        public SymbolAccessor(SymbolEntity entity, IPluginSubscriptionHandler handler)
        {
            this.entity = entity;
            this.handler = handler;

            this.Point = System.Math.Pow(10, -entity.Digits);

            this.Ask = double.NaN;
            this.Bid = double.NaN;
        }

        public string Name { get { return entity.Name; } }
        public int Digits { get { return entity.Digits; } }
        public double ContractSize { get { return entity.LotSize; } }
        public double MaxTradeVolume { get { return entity.MaxAmount; } }
        public double MinTradeVolume { get { return entity.MinAmount; } }
        public double TradeVolumeStep { get { return entity.AmountStep; } }
        public string BaseCurrency { get { return entity.BaseCurrencyCode; } }
        public string CounterCurrency { get { return entity.CounterCurrencyCode; } }
        public bool IsNull { get { return false; } }
        public double Point { get; private set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public bool IsTradeAllowed { get { return entity.IsTradeAllowed; } }

        public void Subscribe(int depth = 1)
        {
            handler.Subscribe(Name, depth);
        }

        public void Unsubscribe()
        {
            handler.Unsubscribe(Name);
        }
    }

    public class NullSymbol : Api.Symbol
    {
        public NullSymbol(string code)
        {
            this.Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get { return -1; } }
        public double ContractSize { get { return double.NaN; } }
        public double MaxTradeVolume { get { return double.NaN; } }
        public double MinTradeVolume { get { return double.NaN; } }
        public double TradeVolumeStep { get { return double.NaN; } }
        public string BaseCurrency { get { return ""; } }
        public string CounterCurrency { get { return ""; } }
        public bool IsNull { get { return true; } }
        public double Point { get { return double.NaN; } }
        public double Bid { get { return double.NaN; } }
        public double Ask { get { return double.NaN; } }
        public bool IsTradeAllowed { get { return false; } }

        public void Subscribe(int depth = 1)
        {
        }

        public void Unsubscribe()
        {
        }
    }
}
