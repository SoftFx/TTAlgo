namespace TickTrader.Algo.Api.Indicators
{
    public interface IFastAdaptiveTrendLine
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Fatl { get; }
    }
}
