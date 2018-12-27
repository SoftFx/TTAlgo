namespace TickTrader.Algo.Api.Indicators
{
    public interface IReferenceFastTrendLine
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Rftl { get; }

        int LastPositionChanged { get; }
    }
}
