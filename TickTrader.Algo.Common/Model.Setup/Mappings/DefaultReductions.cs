using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Common.Model.Setup
{
    internal class FullBarToBidCloseReduction : FullBarToDoubleReduction
    {
        public double Reduce(Bar bidBar, Bar askBar)
        {
            return bidBar.Close;
        }
    }


    internal class BarToCloseReduction : BarToDoubleReduction
    {
        public double Reduce(Bar bar)
        {
            return bar.Close;
        }
    }


    internal class QuoteToBestBidReduction : QuoteToDoubleReduction
    {
        public double Reduce(Quote quote)
        {
            return quote.Bid;
        }
    }


    internal class QuoteToBidBarReduction : QuoteToBarReduction
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
}
