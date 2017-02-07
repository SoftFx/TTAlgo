using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Quote
    {
        string Symbol { get; }
        DateTime Time { get; }
        double Ask { get; }
        double Bid { get; }

        BookEntry[] AskBook { get; }
        BookEntry[] BidBook { get; }
    }

    public interface BookEntry
    {
        double Price { get; }
        double Volume { get; }
    }
}
