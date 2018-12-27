namespace TickTrader.Algo.Api.Indicators
{
    public interface IStandardDeviation
    {
        int Period { get; }

        int Shift { get; }

        MovingAverageMethod TargetMethod { get; }

        DataSeries Price { get; }

        DataSeries StdDev { get; }

        int LastPositionChanged { get; }
    }
}
