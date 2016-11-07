using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    /// <summary>
    /// Price update summary. May represent one or multiple quotes.
    /// May represent quite a few quotes in case  your TradeBot is too slow or quote update rate is too high.
    /// Note: LastQuotes array contains limmited number of quotes and its size may be less than NumberOfQuotes.
    /// Use LastQuotes[0] to get most relevant quote.
    /// </summary>
    public interface RateUpdate
    {
        string Symbol { get; }
        DateTime Time { get; }
        double Ask { get; }
        double AskHigh { get; }
        double AskLow { get; }
        double Bid { get; }
        double BidHigh { get; }
        double BidLow { get; }
        double NumberOfQuotes { get; }

        Quote[] LastQuotes { get; }
    }
}
