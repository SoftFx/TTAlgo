namespace TickTrader.Algo.Api.Indicators
{ 
    public interface IAverageTrueRange
    {
        int Period { get; }

        BarSeries Bars { get; }

        DataSeries Atr { get; }

        int LastPositionChanged { get; }
    }
}
