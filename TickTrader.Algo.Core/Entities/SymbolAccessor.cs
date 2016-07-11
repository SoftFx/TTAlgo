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
        }

        public string Code { get { return entity.Code; } }
        public int Digits { get { return entity.Digits; } }
        public double LotSize { get { return entity.LotSize; } }
        public double MaxAmount { get { return entity.MaxAmount; } }
        public double MinAmount { get { return entity.MinAmount; } }
        public string BaseCurrencyCode { get { return entity.BaseCurrencyCode; } }
        public string CounterCurrencyCode { get { return entity.CounterCurrencyCode; } }
        public bool IsNull { get { return false; } }

        public void Subscribe(int depth = 1)
        {
            handler.Subscribe(Code, depth);
        }

        public void Unsubscribe()
        {
            handler.Unsubscribe(Code);
        }
    }

    public class NullSymbol : Api.Symbol
    {
        public NullSymbol(string code)
        {
            this.Code = code;
        }

        public string Code { get; private set; }
        public int Digits { get { return -1; } }
        public double LotSize { get { return double.NaN; } }
        public double MaxAmount { get { return double.NaN; } }
        public double MinAmount { get { return double.NaN; } }
        public string BaseCurrencyCode { get { return ""; } }
        public string CounterCurrencyCode { get { return ""; } }
        public bool IsNull { get { return true; } }

        public void Subscribe(int depth = 1)
        {
        }

        public void Unsubscribe()
        {
        }
    }
}
