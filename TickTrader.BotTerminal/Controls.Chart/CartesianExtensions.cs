using LiveChartsCore.Defaults;
using System;
using System.Text;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal static class CartesianExtensions
    {
        internal static string ToCandleTooltipInfo(this FinancialPoint bar, ChartSettings settings)
        {
            var sb = new StringBuilder(1 << 5);
            var format = settings.PriceFormat;

            sb.AppendLine($"{bar.Date.ToString(settings.DateFormat)}")
              .AppendLine($"O: {bar.Open.ToString(format)}")
              .AppendLine($"H: {bar.High.ToString(format)}")
              .AppendLine($"L: {bar.Low.ToString(format)}")
              .Append($"C: {bar.Close.ToString(format)}");

            return sb.ToString();
        }

        internal static string ToLineTooltipInfo(this FinancialPoint bar, ChartSettings settings)
        {
            var sb = new StringBuilder(1 << 5);

            sb.AppendLine($"{bar.Date.ToString(settings.DateFormat)}")
              .Append($"C: {bar.Close.ToString(settings.PriceFormat)}");

            return sb.ToString();
        }

        internal static string ToPointTooltipInfo(this IndicatorPoint point, IndicatorChartSettings settings)
        {
            var sb = new StringBuilder(1 << 5);

            sb.AppendLine($"{point.Time.ToString(settings.DateFormat)}")
              .Append($"{settings.Name}: {point.Value.ToString(settings.PriceFormat)}");

            return sb.ToString();
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
