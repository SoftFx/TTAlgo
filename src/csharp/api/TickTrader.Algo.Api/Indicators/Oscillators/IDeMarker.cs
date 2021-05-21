namespace TickTrader.Algo.Api.Indicators
{
    public interface IDeMarker
    {
        int Period { get; }

        BarSeries Bars { get; }

        DataSeries DeMark { get; }

        int LastPositionChanged { get; }
    }
}
