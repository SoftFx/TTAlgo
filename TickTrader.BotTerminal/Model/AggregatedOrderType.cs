using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, BuyStopLimit, Sell, SellLimit, SellStop, SellStopLimit }

    public static class OrderCommonExtensions
    {
        public static AggregatedOrderType Aggregate(this OrderInfo.Types.Side side, OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case OrderInfo.Types.Type.Market:
                case OrderInfo.Types.Type.Position:
                    return side == OrderInfo.Types.Side.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case OrderInfo.Types.Type.Limit:
                    return side == OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case OrderInfo.Types.Type.Stop:
                    return side == OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                case OrderInfo.Types.Type.StopLimit:
                    return side == OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyStopLimit : AggregatedOrderType.SellStopLimit;
                default: return AggregatedOrderType.Unknown;
            }
        }
    }
}
