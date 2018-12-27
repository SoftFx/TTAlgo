namespace TickTrader.Algo.Api.Indicators
{
    public interface IForceIndex
    {
        int Period { get; }

        MovingAverageMethod TargetMethod { get; }

        AppliedPrice.Target TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Force { get; }

        int LastPositionChanged { get; }
    }
}
