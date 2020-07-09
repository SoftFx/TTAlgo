using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public static class CoreExtentions
    {
        public static double? NullableAsk(this RateUpdate rate)
        {
            return rate.HasAsk ? rate.Ask : (double?)null;
        }

        public static double? NullableBid(this RateUpdate rate)
        {
            return rate.HasBid ? rate.Bid : (double?)null;
        }

        public static double? DoubleNullableAsk(this RateUpdate rate)
        {
            return rate.Ask.GetNanAware();
        }

        public static double? DoubleNullableBid(this RateUpdate rate)
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

        public static bool IsTicks(this TimeFrames timeFrame)
        {
            return timeFrame == TimeFrames.Ticks || timeFrame == TimeFrames.TicksLevel2;
        }

        internal static Currency GetOrStub(this Dictionary<string, Currency> currencies, string key)
        {
            return currencies.GetOrDefault(key) ?? new CurrencyEntity(null);
        }

        internal static Currency GetOrStub(this Dictionary<string, CurrencyEntity> currencies, string key)
        {
            return currencies.GetOrDefault(key) ?? new CurrencyEntity(null);
        }

        public static OrderSide Revert(this OrderSide side)
        {
            return side == OrderSide.Sell ? OrderSide.Buy : OrderSide.Sell;
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
