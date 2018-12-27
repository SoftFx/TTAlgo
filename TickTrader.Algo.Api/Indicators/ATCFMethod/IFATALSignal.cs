namespace TickTrader.Algo.Api.Indicators
{
    public interface IFATALSignal
    {
        int CountBars { get; }

        AppliedPrice.Target TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Up { get; }

        DataSeries Down { get; }

        int LastPositionChanged { get; }
    }
}
