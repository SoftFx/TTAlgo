using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, BuyStopLimit, Sell, SellLimit, SellStop, SellStopLimit }

    public static class OrderCommonExtensions
    {
        public static AggregatedOrderType Aggregate(this OrderSide side, OrderType type)
        {
            switch (type)
            {
                case OrderType.Market:
                case OrderType.Position:
                    return side == OrderSide.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case OrderType.Limit:
                    return side == OrderSide.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case OrderType.Stop:
                    return side == OrderSide.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                case OrderType.StopLimit:
                    return side == OrderSide.Buy ? AggregatedOrderType.BuyStopLimit : AggregatedOrderType.SellStopLimit;
                default: return AggregatedOrderType.Unknown;
            }
        }

        public static AggregatedOrderType Aggregate(this OrderSide type, OrderSide side)
        {
            return Aggregate(side, type);
        }
    }
}
