using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, BuyStopLimit, Sell, SellLimit, SellStop, SellStopLimit }

    public static class OrderCommonExtensions
    {
        public static AggregatedOrderType Aggregate(this Domain.OrderInfo.Types.Side side, OrderType type)
        {
            switch (type)
            {
                case OrderType.Market:
                case OrderType.Position:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case OrderType.Limit:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case OrderType.Stop:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                case OrderType.StopLimit:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyStopLimit : AggregatedOrderType.SellStopLimit;
                default: return AggregatedOrderType.Unknown;
            }
        }

        public static AggregatedOrderType Aggregate(this OrderSide type, OrderSide side)
        {
            return Aggregate(side, type);
        }
    }
}
