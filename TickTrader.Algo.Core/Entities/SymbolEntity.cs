using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class SymbolEntity : Api.Symbol
    {
        public SymbolEntity(string code)
        {
            this.Code = code;
        }

        public string Code { get; private set; }
        public int Digits { get; set; }
        public double LotSize { get; set; }
        public double MaxAmount { get; set; }
        public double MinAmount { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string CounterCurrencyCode { get; set; }
        public bool IsNull { get { return false; } }
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
    }
}
