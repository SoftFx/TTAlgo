namespace TickTrader.Algo.Api.Indicators
{
    public interface IMoneyFlowIndex
    {
        int Period { get; }

        BarSeries Bars { get; }

        DataSeries Mfi { get; }

        int LastPositionChanged { get; }
    }
}
