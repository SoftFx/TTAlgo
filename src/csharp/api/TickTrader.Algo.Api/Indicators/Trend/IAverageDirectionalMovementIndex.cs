namespace TickTrader.Algo.Api.Indicators
{
    public interface IAverageDirectionalMovementIndex
    {
        int Period { get; }

        AppliedPrice TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Price { get; }

        DataSeries Adx { get; }

        DataSeries PlusDmi { get; }

        DataSeries MinusDmi { get; }

        int LastPositionChanged { get; }
    }
}
