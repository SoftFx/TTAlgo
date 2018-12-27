namespace TickTrader.Algo.Api.Indicators
{
    public interface IIchimokuKinkoHyo
    {
        int TenkanSen { get; }

        int KijunSen { get; }

        int SenkouSpanB { get; }

        BarSeries Bars { get; }

        DataSeries Tenkan { get; }

        DataSeries Kijun { get; }

        DataSeries SenkouA { get; }

        DataSeries SenkouB { get; }

        DataSeries Chikou { get; }

        int LastPositionChanged { get; }
    }
}
