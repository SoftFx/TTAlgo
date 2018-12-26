namespace TickTrader.Algo.Api.Indicators
{
    public interface IRelativeStrenghtIndex
    {
        int Period { get; }

        DataSeries Price { get; }

        DataSeries Rsi { get; }
    }
}
