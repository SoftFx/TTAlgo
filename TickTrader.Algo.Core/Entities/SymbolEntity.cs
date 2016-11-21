using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class SymbolEntity
    {
        public SymbolEntity(string code)
        {
            this.Name = code;
        }

        public string Name { get; private set; }
        public int Digits { get; set; }
        public double LotSize { get; set; }
        public double MaxAmount { get; set; }
        public double MinAmount { get; set; }
        public double AmountStep { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string CounterCurrencyCode { get; set; }
        public bool IsTradeAllowed { get; set; }
    }
}
