namespace TickTrader.Algo.Api.Indicators
{
    public interface IMacd
    {
        int FastEma { get; }

        int SlowEma { get; }

        int MacdSma { get; }

        DataSeries Price { get; }

        DataSeries MacdSeries { get; }

        DataSeries Signal { get; }

        int LastPositionChanged { get; }
    }
}
