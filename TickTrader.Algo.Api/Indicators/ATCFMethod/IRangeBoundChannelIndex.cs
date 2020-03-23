namespace TickTrader.Algo.Api.Indicators
{
    public interface IRangeBoundChannelIndexAvg
    {
        int Std { get; }

        int CountBars { get; }

        CalcFrequency Frequency { get; set; }

        DataSeries Price { get; }

        DataSeries Rbci { get; }

        DataSeries UpperBound { get; }

        DataSeries LowerBound { get; }

        DataSeries UpperBound2 { get; }

        DataSeries LowerBound2 { get; }

        int LastPositionChanged { get; }


        bool HasEnoughBars(int barsCount);
    }

    public interface IRangeBoundChannelIndexBBands
    {
        int DeviationPeriod { get; }

        double DeviationCoeff { get; }

        DataSeries Price { get; }

        DataSeries Rbci { get; }

        DataSeries Plus2Sigma { get; }

        DataSeries PlusSigma { get; }

        DataSeries Middle { get; }

        DataSeries MinusSigma { get; }

        DataSeries Minus2Sigma { get; }

        int LastPositionChanged { get; }


        bool HasEnoughBars(int barsCount);
    }
}
