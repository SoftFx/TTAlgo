using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface MarketSeries : DataSeries<Bar>
    {
        TimeSeries OpenTime { get; }
        DataSeries Open { get; }
        DataSeries Close { get; }
        DataSeries High { get; }
        DataSeries Low { get; }
        //DataSeries Median { get; }
        string SymbolCode { get; }
        DataSeries Volume { get; }
        //TimeFrame TimeFrame { get; }
        //DataSeries Typical { get; }
        //DataSeries WeightedClose { get; }
    }

    public interface Leve2Series
    {
        TimeSeries Time { get; }
        DataSeries BestAsk { get; }
        DataSeries BestBid { get; }
        BookSeries AskBook { get; }
        BookSeries BidBook { get; }
    }
}
