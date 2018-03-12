using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Ext
{
    // Bar open/close time internally set to quote.Time

    [Reduction("Bid")]
    public class QuoteBidBarReduction : QuoteToBarReduction
    {
        public void Reduce(Quote quote, IBarWriter result)
        {
            result.Open = quote.Bid;
            result.Close = quote.Bid;
            result.High = quote.Bid;
            result.Low = quote.Bid;
            result.Volume = 1;
        }
    }


    [Reduction("Ask")]
    public class QuoteAskBarReduction : QuoteToBarReduction
    {
        public void Reduce(Quote quote, IBarWriter result)
        {
            result.Open = quote.Ask;
            result.Close = quote.Ask;
            result.High = quote.Ask;
            result.Low = quote.Ask;
            result.Volume = 1;
        }
    }


    [Reduction("Median")]
    public class QuoteMedianBarReduction : QuoteToBarReduction
    {
        public void Reduce(Quote quote, IBarWriter result)
        {
            result.Open = (quote.Ask + quote.Bid) / 2;
            result.Close = (quote.Ask + quote.Bid) / 2;
            result.High = (quote.Ask + quote.Bid) / 2;
            result.Low = (quote.Ask + quote.Bid) / 2;
            result.Volume = 1;
        }
    }
}
