using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Symbol
    {
        string Code { get; }
        int Digits { get; }
        double LotSize { get; }
        double MaxAmount { get; }
        double MinAmount { get; }
        bool IsNull { get; }
        string BaseCurrencyCode { get; }
        string CounterCurrencyCode { get; }

        void Subscribe(int depth = 1);
        void Unsubscribe();
    }

    public interface SymbolProvider : IEnumerable<Symbol>
    {
        Symbol this[string symbolCode] { get; }
        Symbol Current { get; }
    }
}
