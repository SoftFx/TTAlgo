namespace TickTrader.Algo.Api.Indicators
{
    public interface IRangeBoundChannelIndex
    {
        int DeviationPeriod { get; }

        double DeviationCoeff { get; }

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
