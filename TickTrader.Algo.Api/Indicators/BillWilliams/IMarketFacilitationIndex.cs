namespace TickTrader.Algo.Api.Indicators
{
    public interface IMarketFacilitationIndex
    {
        double PointSize { get; }

        BarSeries Bars { get; }

        DataSeries MfiUpVolumeUp { get; }

        DataSeries MfiUpVolumeDown { get; }

        DataSeries MfiDownVolumeUp { get; }

        DataSeries MfiDownVolumeDown { get; }
    }
}
