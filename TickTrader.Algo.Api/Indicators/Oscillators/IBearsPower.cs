namespace TickTrader.Algo.Api.Indicators
{
    public interface IBearsPower
    {
        int Period { get; }

        AppliedPrice.Target TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Bears { get; }

        int LastPositionChanged { get; }
    }
}
