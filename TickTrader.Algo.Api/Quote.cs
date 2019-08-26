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

        bool HasAsk { get; }
        bool HasBid { get; }

        bool IsAskIndicative { get; }
        bool IsBidIndicative { get; }

        BookEntry[] AskBook { get; }
        BookEntry[] BidBook { get; }
    }

    [Serializable]
    public struct BookEntry
    {
        public BookEntry(double price, double volume)
        {
            Price = price;
            Volume = volume;
        }

        public double Price { get; }
        public double Volume { get; }
    }
}
