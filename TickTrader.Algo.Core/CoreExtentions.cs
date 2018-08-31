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
            return rate.Ask.PriceToDecimal();
        }

        public static decimal? NullableBid(this RateUpdate rate)
        {
            return rate.Bid.PriceToDecimal();
        }

        public static double? AsNullable(this double value)
        {
            return double.IsNaN(value) ? null : (double?)value;
        }

        public static decimal? PriceToDecimal(this double price)
        {
            return double.IsNaN(price) ? null : (decimal?)price;
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
