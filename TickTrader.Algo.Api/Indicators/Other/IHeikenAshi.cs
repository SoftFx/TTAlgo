namespace TickTrader.Algo.Api.Indicators
{
    public interface IHeikenAshi
    {
        BarSeries Bars { get; }

        DataSeries HaLowHigh { get; }

        DataSeries HaHighLow { get; }

        DataSeries HaOpen { get; }

        DataSeries HaClose { get; }

        int LastPositionChanged { get; }
    }
}
