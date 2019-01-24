namespace TickTrader.Algo.Api.Indicators
{
    public interface IVolumes
    {
        BarSeries Bars { get; }

        DataSeries ValueUp { get; }

        DataSeries ValueDown { get; }

        int LastPositionChanged { get; }
    }
}
