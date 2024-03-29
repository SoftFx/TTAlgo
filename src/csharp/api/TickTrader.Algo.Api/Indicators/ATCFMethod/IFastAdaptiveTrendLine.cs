﻿namespace TickTrader.Algo.Api.Indicators
{
    public interface IFastAdaptiveTrendLine
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Fatl { get; }

        int LastPositionChanged { get; }

        bool HasEnoughBars(int barsCount);
    }
}
