namespace TickTrader.Algo.Api.Indicators
{
    public interface IRangeBoundChannelIndex
    {
        int Std { get; }

        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Rbci { get; }

        DataSeries UpperBound { get; }

        DataSeries LowerBound { get; }

        DataSeries UpperBound2 { get; }

        DataSeries LowerBound2 { get; }

        int LastPositionChanged { get; }


        bool HasEnoughBars(int barsCount);
    }
}
