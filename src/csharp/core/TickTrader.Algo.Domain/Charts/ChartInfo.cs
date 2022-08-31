namespace TickTrader.Algo.Domain
{
    public interface IChartInfo
    {
        int Id { get; }

        string Symbol { get; }

        Feed.Types.Timeframe Timeframe { get; }

        Feed.Types.MarketSide MarketSide { get; }
    }


    public class ChartInfo : IChartInfo
    {
        public int Id { get; set; }

        public string Symbol { get; set; }

        public Feed.Types.Timeframe Timeframe { get; set; }

        public Feed.Types.MarketSide MarketSide { get; set; }
    }
}
