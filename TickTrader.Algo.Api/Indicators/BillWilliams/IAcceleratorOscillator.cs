namespace TickTrader.Algo.Api.Indicators
{
    public interface IAcceleratorOscillator
    {
        int FastSmaPeriod { get; }

        int SlowSmaPeriod { get; }

        int DataLimit { get; }

        BarSeries Bars { get; }

        DataSeries ValueUp { get; }

        DataSeries ValueDown { get; }
    }
}
