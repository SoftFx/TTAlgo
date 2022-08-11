using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    public class ChartSettings
    {
        internal int Precision { get; set; }

        internal Feed.Types.Timeframe Period { get; set; } = Feed.Types.Timeframe.M1;


        internal string PriceFormat => $"F{Precision}";

        internal string DateFormat => Period switch
        {
            Feed.Types.Timeframe.MN => "MMM yyyy",
            Feed.Types.Timeframe.W or Feed.Types.Timeframe.D => "d MMM yyyy",
            Feed.Types.Timeframe.H4 or Feed.Types.Timeframe.H1 or
            Feed.Types.Timeframe.M30 or Feed.Types.Timeframe.M15 or
            Feed.Types.Timeframe.M5 or Feed.Types.Timeframe.M1 => "d MMM yyyy HH:mm",
            _ => "d MMM yyyy HH:mm:ss",
        };
    }


    public sealed class TradeChartSettings : ChartSettings
    {
        internal ChartTypes ChartType { get; set; }
    }
}