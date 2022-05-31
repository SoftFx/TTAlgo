using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed class ChartTradeSettings
    {
        internal int SymbolDigits { get; set; }

        internal Feed.Types.Timeframe Period { get; set; }


        internal string PriceFormat => $"F{SymbolDigits}";

        internal string DateFormat => Customizer.GetPeriodDateFormat(Period);
    }
}
