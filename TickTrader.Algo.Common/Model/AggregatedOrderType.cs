using SoftFX.Extended;

namespace TickTrader.Algo.Common.Model
{
    public enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, BuyStopLimit, Sell, SellLimit, SellStop, SellStopLimit }

    public static class OrderCommonExtensions
    {
        public static AggregatedOrderType Aggregate(this TradeRecordSide side, TradeRecordType type)
        {
            switch (type)
            {
                case TradeRecordType.Market:
                case TradeRecordType.Position:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case TradeRecordType.Limit:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case TradeRecordType.Stop:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                case TradeRecordType.StopLimit:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyStopLimit : AggregatedOrderType.SellStopLimit;
                default: return AggregatedOrderType.Unknown;
            }
        }

        public static AggregatedOrderType Aggregate(this TradeRecordType type, TradeRecordSide side)
        {
            return Aggregate(side, type);
        }
    }
}
