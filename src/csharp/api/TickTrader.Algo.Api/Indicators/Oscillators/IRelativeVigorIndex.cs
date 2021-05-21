namespace TickTrader.Algo.Api.Indicators
{
    public interface IRelativeVigorIndex
    {
        int Period { get; }

        BarSeries Bars { get; }

        DataSeries RviAverage { get; }

        DataSeries Signal { get; }

        int LastPositionChanged { get; }
    }
}
