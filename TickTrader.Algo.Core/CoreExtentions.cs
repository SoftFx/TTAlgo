using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public static class CoreExtentions
    {
        public static double? NullableAsk(this IRateInfo rate)
        {
            return rate.HasAsk ? rate.Ask : (double?)null;
        }

        public static double? NullableBid(this IRateInfo rate)
        {
            return rate.HasBid ? rate.Bid : (double?)null;
        }

        public static double? DoubleNullableAsk(this IRateInfo rate)
        {
            return rate.Ask.GetNanAware();
        }

        public static double? DoubleNullableBid(this IRateInfo rate)
        {
            return rate.Bid.GetNanAware();
        }

        public static double? AsNullable(this double value)
        {
            return double.IsNaN(value) ? null : (double?)value;
        }

        public static decimal? NanAwareToDecimal(this double value)
        {
            return double.IsNaN(value) ? null : (decimal?)value;
        }

        public static double? GetNanAware(this double value)
        {
            return double.IsNaN(value) ? null : (double?)value;
        }

        public static decimal? NanAwareToDecimal(this double? value)
        {
            return value == null || double.IsNaN(value.Value) ? null : (decimal?)value;
        }

        public static decimal ToDecimal(this double price)
        {
            return (decimal)price;
        }

        public static bool IsTicks(this Feed.Types.Timeframe timeFrame)
        {
            return timeFrame == Feed.Types.Timeframe.Ticks || timeFrame == Feed.Types.Timeframe.TicksLevel2;
        }

        public static OrderInfo.Types.Side Revert(this OrderInfo.Types.Side side)
        {
            return side == OrderInfo.Types.Side.Sell ? OrderInfo.Types.Side.Buy : OrderInfo.Types.Side.Sell;
        }
    }
}
