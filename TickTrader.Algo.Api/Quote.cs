using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Quote
    {
        string SymbolCode { get; }
        DateTime Time { get; }
        double Ask { get; }
        double Bid { get; }
    }

    public interface QuoteL2 : Quote
    {
        IReadOnlyList<BookEntry> AskBook { get; }
        IReadOnlyList<BookEntry> BidBook { get; }
    }

    public interface BookEntry
    {
        double Price { get; }
        double Value { get; }
    }
}
