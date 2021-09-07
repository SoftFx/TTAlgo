namespace TickTrader.Algo.Api.Indicators
{
    public interface IZigZag
    {
        int Depth { get; }

        int Deviation { get; }

        int Backstep { get; }

        BarSeries Bars { get; }

        DataSeries Zigzag { get; }

        DataSeries ZigzagLine { get; }

        int LastPositionChanged { get; }
    }
}
