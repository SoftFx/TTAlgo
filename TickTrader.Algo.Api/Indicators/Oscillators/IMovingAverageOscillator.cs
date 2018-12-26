namespace TickTrader.Algo.Api.Indicators
{
    public interface IMovingAverageOscillator
    {
        int FastEma { get; }

        int SlowEma { get; }

        int MacdSma { get; }

        DataSeries Price { get; }

        DataSeries OsMa { get; }
    }
}
