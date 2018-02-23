using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Ext
{
    [Reduction("BestBid")]
    public class QuoteBestBidReduction : QuoteToDoubleReduction
    {
        public double Reduce(Quote quote)
        {
            return quote.Bid;
        }
    }


    [Reduction("BestAsk")]
    public class QuoteBestAskReduction : QuoteToDoubleReduction
    {
        public double Reduce(Quote quote)
        {
            return quote.Ask;
        }
    }


    [Reduction("Median")]
    public class QuoteMedianReduction : QuoteToDoubleReduction
    {
        public double Reduce(Quote quote)
        {
            return (quote.Ask + quote.Bid) / 2;
        }
    }
}
