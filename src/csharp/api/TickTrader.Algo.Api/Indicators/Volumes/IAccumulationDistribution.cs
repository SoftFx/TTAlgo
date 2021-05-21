namespace TickTrader.Algo.Api.Indicators
{
    public interface IAccumulationDistribution
    {
        BarSeries Bars { get; }

        DataSeries Ad { get; }

        int LastPositionChanged { get; }
    }
}
