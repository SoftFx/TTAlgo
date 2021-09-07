namespace TickTrader.Algo.Api.Indicators
{
    public interface IFATLSignal
    {
        int CountBars { get; }

        AppliedPrice TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Up { get; }

        DataSeries Down { get; }

        int LastPositionChanged { get; }


        bool HasEnoughBars(int barsCount);
    }
}
