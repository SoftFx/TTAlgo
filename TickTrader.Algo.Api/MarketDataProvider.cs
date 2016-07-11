using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface MarketDataProvider
    {
        BarSeries Bars { get; }
        QuoteSeries Quotes { get; }
        QuoteSeries Level2 { get; }
        BarSeries GetBars(string symbolCode);
        BarSeries GetBars(string symbolCode, TimeFrames timeFrame);
        BarSeries GetBars(string symbolCode, TimeFrames timeFrame, DateTime from, DateTime to);
        QuoteSeries GetQuotes(string symbolCode);
        QuoteSeries GetQuotes(string symbolCode, DateTime from, DateTime to);
        QuoteSeries GetLevel2(string symbolCode);
        QuoteSeries GetLevel2(string symbolCode, DateTime from, DateTime to);
    }

    public interface BarSeries : DataSeries<Bar>
    {
        TimeSeries OpenTime { get; }
        DataSeries Open { get; }
        DataSeries Close { get; }
        DataSeries High { get; }
        DataSeries Low { get; }
        DataSeries Median { get; }
        string SymbolCode { get; }
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
}
