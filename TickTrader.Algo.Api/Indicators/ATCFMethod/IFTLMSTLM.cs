namespace TickTrader.Algo.Api.Indicators
{
    public interface IFTLMSTLM
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Ftlm { get; }

        DataSeries Stlm { get; }
    }
}
