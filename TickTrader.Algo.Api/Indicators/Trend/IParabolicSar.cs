namespace TickTrader.Algo.Api.Indicators
{
    public interface IParabolicSar
    {
        double Step { get; }

        double Maximum { get; }

        BarSeries Bars { get; }

        DataSeries Sar { get; }
    }
}
