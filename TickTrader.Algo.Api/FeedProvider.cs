using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface FeedProvider
    {
        BarSeries Bars { get; }
        CustomFeedProvider CustomCommds { get; }
        //QuoteSeries Quotes { get; }
        //QuoteSeries Level2 { get; }
    }

    public enum BarPriceType
    {
        Bid     = 1,
        Ask     = 2
    }

    public interface CustomFeedProvider
    {
        void Subscribe(string symbol, int depth = 1);
        void Unsubscribe(string symbol);
        BarSeries GetBarSeries(string symbol);
        BarSeries GetBarSeries(string symbol, BarPriceType side);
        IEnumerable<Bar> GetBars(string symbol, TimeFrames timeFrame, DateTime from, DateTime to, BarPriceType side, bool backwardOrder);
        IEnumerable<Quote> GetQuotes(string symbol, DateTime from, DateTime to, bool level2, bool backwardOrder);
    }

    public interface BarSeries : DataSeries<Bar>
    {
        TimeSeries OpenTime { get; }
        DataSeries Open { get; }
        DataSeries Close { get; }
        DataSeries High { get; }
        DataSeries Low { get; }
        DataSeries Median { get; }
        string Symbol { get; }
        DataSeries Volume { get; }
        DataSeries Typical { get; }
        DataSeries Weighted { get; }
        DataSeries Move { get; }
        DataSeries Range { get; }
        //TimeFrame TimeFrame { get; }
        //DataSeries Typical { get; }
        //DataSeries WeightedClose { get; }
    }

    public interface QuoteSeries : DataSeries<Quote>
    {
        TimeSeries Time { get; }
        DataSeries Ask { get; }
        DataSeries Bid { get; }
    }

    public interface QuoteL2Series : QuoteSeries
    {
    }
}
