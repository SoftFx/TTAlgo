using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, BuyStopLimit, Sell, SellLimit, SellStop, SellStopLimit }

    public static class OrderCommonExtensions
    {
        public static AggregatedOrderType Aggregate(this OrderInfo.Types.Side side, OrderInfo.Types.Type type)
        {
            return type switch
            {
                OrderInfo.Types.Type.Market or OrderInfo.Types.Type.Position => side.IsBuy() ? AggregatedOrderType.Buy : AggregatedOrderType.Sell,
                OrderInfo.Types.Type.Limit => side.IsBuy() ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit,
                OrderInfo.Types.Type.Stop => side.IsBuy() ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop,
                OrderInfo.Types.Type.StopLimit => side.IsBuy() ? AggregatedOrderType.BuyStopLimit : AggregatedOrderType.SellStopLimit,
                _ => AggregatedOrderType.Unknown,
            };
        }
    }
}
