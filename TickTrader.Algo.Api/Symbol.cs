using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Symbol
    {
        string Name { get; }
        int Digits { get; }
        double Point { get; }
        double ContractSize { get; }
        double MaxTradeVolume { get; }
        double MinTradeVolume { get; }
        double TradeVolumeStep { get; }
        bool IsTradeAllowed { get; }
        bool IsNull { get; }
        string BaseCurrency { get; }
        string CounterCurrency { get; }
        double Bid { get; }
        double Ask { get; }
        Quote LastQuote { get; }

        void Subscribe(int depth = 1);
        void Unsubscribe();
    }

    public interface SymbolProvider
    {
        SymbolList List { get; }
        Symbol MainSymbol { get; }
    }

    public interface SymbolList : IEnumerable<Symbol>
    {
        Symbol this[string symbol] { get; }
    }
}
