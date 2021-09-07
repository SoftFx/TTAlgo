namespace TickTrader.Algo.Api.Indicators
{
    public interface ICommodityChannelIndex
    {
        int Period { get; }

        DataSeries Price { get; }

        DataSeries Cci { get; }

        int LastPositionChanged { get; }
    }
}
