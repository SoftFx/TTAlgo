using System;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal static class TradePointsFactory
    {
        internal static EventPoint GetOpenMarker(BaseTransactionModel report)
        {
            var price = report.OpenPrice;

            return new EventPoint
            {
                Price = price,
                Time = report.OpenTime.Ticks,
                ToolTip = BuildTooltip("Open", report, report.OpenQuantity, price),
            };
        }

        internal static EventPoint GetCloseMarker(BaseTransactionModel report)
        {
            var side = report.Side == BaseTransactionModel.TransactionSide.Sell ? "Buy" : "Sell";

            return new EventPoint
            {
                Price = report.ClosePrice,
                Time = report.CloseTime.Ticks,
                ToolTip = BuildTooltip("Close", report, report.CloseQuantity, report.ClosePrice, side),
            };
        }

        internal static EventPoint GetFillMarker(BaseTransactionModel report)
        {
            var price = report.OpenPrice;

            return new EventPoint
            {
                Price = price,
                Time = report.OpenTime.Ticks,
                ToolTip = BuildTooltip("Fill", report, report.OpenQuantity, price),
            };
        }


        private static string BuildTooltip(string action, BaseTransactionModel report, double? volume, double? price, string side = null)
        {
            side ??= report.Side.ToString();

            if (volume is not null)
                volume = Math.Round(volume.Value, report.PriceDigits);

            return $"{action} #{report.OrderNum} {side} {volume} at price {price?.ToString($"F{report.PriceDigits}")}";
        }
    }


    internal static class IndicatorPointsFactory
    {
        internal static IndicatorPoint GetDefaultPoint(OutputPoint point)
        {
            return new IndicatorPoint
            {
                Value = double.IsNaN(point.Value) ? 0.0 : point.Value,
                Time = new DateTime(point.Time.Value),
            };
        }
    }
}