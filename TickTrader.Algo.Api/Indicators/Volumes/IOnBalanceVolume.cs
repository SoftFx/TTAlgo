namespace TickTrader.Algo.Api.Indicators
{
    public interface IOnBalanceVolume
    {
        AppliedPrice TargetPrice { get; }

        BarSeries Bars { get; }

        DataSeries Obv { get; }

        int LastPositionChanged { get; }
    }
}
