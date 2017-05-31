using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Model
{
    public static class FdkExtension
    {
        public static double? GetNullableAsk(this Quote q)
        {
            return q.HasAsk ? (double?)q.Ask : null;
        }

        public static double? GetNullableBid(this Quote q)
        {
            return q.HasBid ? (double?)q.Bid : null;
        }
    }
}
