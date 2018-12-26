﻿namespace TickTrader.Algo.Api.Indicators
{
    public interface ISlowAdaptiveTrendLine
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Satl { get; }
    }
}
