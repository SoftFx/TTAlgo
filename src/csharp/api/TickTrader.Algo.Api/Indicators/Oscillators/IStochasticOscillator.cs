namespace TickTrader.Algo.Api.Indicators
{
    public enum PriceField
    {
        LowHigh = 0,
        CloseClose = 1,
    }

    public interface IStochasticOscillator
    {
        int KPeriod { get; }

        int Slowing { get; }

        int DPeriod { get; }

        MovingAverageMethod TargetMethod { get; }

        PriceField TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Stoch { get; }

        DataSeries Signal { get; }

        int LastPositionChanged { get; }
    }
}
