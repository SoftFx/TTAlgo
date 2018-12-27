namespace TickTrader.Algo.Api.Indicators
{
    public interface IFastTrendLineMomentum
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Ftlm { get; }

        int LastPositionChanged { get; }
    }
}
