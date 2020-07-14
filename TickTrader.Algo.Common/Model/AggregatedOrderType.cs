using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, BuyStopLimit, Sell, SellLimit, SellStop, SellStopLimit }

    public static class OrderCommonExtensions
    {
        public static AggregatedOrderType Aggregate(this Domain.OrderInfo.Types.Side side, Domain.OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.OrderInfo.Types.Type.Market:
                case Domain.OrderInfo.Types.Type.Position:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case Domain.OrderInfo.Types.Type.Limit:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case Domain.OrderInfo.Types.Type.Stop:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                case Domain.OrderInfo.Types.Type.StopLimit:
                    return side == Domain.OrderInfo.Types.Side.Buy ? AggregatedOrderType.BuyStopLimit : AggregatedOrderType.SellStopLimit;
                default: return AggregatedOrderType.Unknown;
            }
        }
    }
}
