namespace TickTrader.Algo.Api.Indicators
{
    public interface ISlowTrendLineMomentum
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Stlm { get; }
    }
}
