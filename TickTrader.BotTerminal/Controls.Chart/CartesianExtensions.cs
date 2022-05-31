using LiveChartsCore.Defaults;
using System;
using System.Text;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal static class CartesianExtensions
    {
        internal static string ToCandelTooltipInfo(this FinancialPoint bar, int precision)
        {
            var sb = new StringBuilder(1 << 5);
            var format = $"F{precision}";

            sb.AppendLine($"O: {bar.Open.ToString(format)}")
              .AppendLine($"H: {bar.High.ToString(format)}")
              .AppendLine($"L: {bar.Low.ToString(format)}")
              .Append($"C: {bar.Close.ToString(format)}");

            return sb.ToString();
        }

        internal static string ToLineTooltipInfo(this FinancialPoint bar, int precision)
        {
            return $"C: {bar.Close.ToString($"F{precision}")}";
        }

        internal static FinancialPoint ToPoint(this BarData data)
        {
            return new FinancialPoint(data.OpenTime.ToUtcDateTime(), data.High, data.Open, data.Close, data.Low);
        }

        internal static FinancialPoint ApplyTick(this FinancialPoint point, double tick)
        {
            point.High = Math.Max(point.High, tick);
            point.Low = Math.Min(point.Low, tick);
            point.Close = tick;

            return point;
        }
    }
}
