namespace TickTrader.Algo.Api.Indicators
{
    public interface IWilliamsPercentRange
    {
        int Period { get; }

        BarSeries Bars { get; }

        DataSeries Wpr { get; }

        int LastPositionChanged { get; }
    }
}
