namespace TickTrader.Algo.Api.Indicators
{
    public interface IFractals
    {
        BarSeries Bars { get; }

        DataSeries<Marker> FractalsUp { get; }

        DataSeries<Marker> FractalsDown { get; }
    }
}
