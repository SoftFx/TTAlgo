namespace TickTrader.Algo.Api.Indicators
{
    public interface IReferenceSlowTrendLine
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Rstl { get; }
    }
}
