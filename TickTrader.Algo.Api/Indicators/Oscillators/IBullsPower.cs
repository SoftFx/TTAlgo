namespace TickTrader.Algo.Api.Indicators
{
    public interface IBullsPower
    {
        int Period { get; }

        AppliedPrice TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Bulls { get; }

        int LastPositionChanged { get; }
    }
}
