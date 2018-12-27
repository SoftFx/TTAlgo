namespace TickTrader.Algo.Api.Indicators
{
    public interface IPerfectCommodityChannelIndex
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Pcci { get; }

        int LastPositionChanged { get; }
    }
}
