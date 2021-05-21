namespace TickTrader.Algo.Api.Indicators
{
    public interface IAwesomeOscillator
    {
        int FastSmaPeriod { get; }

        int SlowSmaPeriod { get; }

        int DataLimit { get; }

        BarSeries Bars { get; }

        DataSeries ValueUp { get; }

        DataSeries ValueDown { get; }

        int LastPositionChanged { get; }
    }
}
