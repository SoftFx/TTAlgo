using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class CoreExtentions
    {
        public static decimal? NullableAsk(this RateUpdate rate)
        {
            return rate.Ask.NanAwareToDecimal();
        }

        public static decimal? NullableBid(this RateUpdate rate)
        {
            return rate.Bid.NanAwareToDecimal();
        }

        public static double? AsNullable(this double value)
        {
            return double.IsNaN(value) ? null : (double?)value;
        }

        public static decimal? NanAwareToDecimal(this double value)
        {
            return double.IsNaN(value) ? null : (decimal?)value;
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
    }
}
