using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    /// <summary>
    /// Price update summary. May represent one or multiple quotes.
    /// May represent quite a few quotes in case your TradeBot is too slow or quote update rate is too high.
    /// Use LastQuotes to get most relevant quote.
    /// </summary>
    public interface RateUpdate
    {
        string Symbol { get; }
        DateTime Time { get; }
        double Ask { get; }
        double AskHigh { get; }
        double AskLow { get; }
        double AskOpen { get; }
        double Bid { get; }
        double BidHigh { get; }
        double BidLow { get; }
        double BidOpen { get; }
        int NumberOfQuotes { get; }

        Quote LastQuote { get; }
    }
}
