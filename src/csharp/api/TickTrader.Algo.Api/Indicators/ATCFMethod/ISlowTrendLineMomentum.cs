namespace TickTrader.Algo.Api.Indicators
{
    public interface ISlowTrendLineMomentum
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Stlm { get; }

        int LastPositionChanged { get; }


        bool HasEnoughBars(int barsCount);
    }
}
