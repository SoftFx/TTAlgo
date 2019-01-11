namespace TickTrader.Algo.Api.Indicators
{
    public interface IForceIndex
    {
        int Period { get; }

        MovingAverageMethod TargetMethod { get; }

        AppliedPrice TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Force { get; }

        int LastPositionChanged { get; }
    }
}
