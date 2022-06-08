namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed record TradeEventPoint
    {
        internal double? Price { get; init; }

        internal double Time { get; init; }

        internal string ToolTip { get; init; }
    }


    internal static class TradePointsFactory
    {
        internal static TradeEventPoint GetOpenMarker(BaseTransactionModel report)
        {
            var price = report.OpenPrice;

            return new TradeEventPoint
            {
                Price = price,
                Time = report.OpenTime.Ticks,
                ToolTip = $"Open #{report.OrderId} {report.Side} {report.OpenQuantity} at price {price}"
            };
        }

        internal static TradeEventPoint GetCloseMarker(BaseTransactionModel report)
        {
            var side = report.Side == BaseTransactionModel.TransactionSide.Sell ? "Buy" : "Sell";

            return new TradeEventPoint
            {
                Price = report.ClosePrice,
                Time = report.CloseTime.Ticks,
                ToolTip = $"Close #{report.OrderId} {side} {report.CloseQuantity} at price {report.ClosePrice}"
            };
        }

        internal static TradeEventPoint GetFillMarker(BaseTransactionModel report)
        {
            var price = report.OpenPrice;

            return new TradeEventPoint
            {
                Price = price,
                Time = report.OpenTime.Ticks,
                ToolTip = $"Fill #{report.OrderId} {report.Side} {report.OpenQuantity} at price {price}"
            };
        }
    }
}