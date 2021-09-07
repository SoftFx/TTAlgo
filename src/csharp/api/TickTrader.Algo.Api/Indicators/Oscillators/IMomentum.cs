namespace TickTrader.Algo.Api.Indicators
{
    public interface IMomentum
    {
        int Period { get; }

        DataSeries Price { get; }

        DataSeries Moment { get; }

        int LastPositionChanged { get; }
    }
}
