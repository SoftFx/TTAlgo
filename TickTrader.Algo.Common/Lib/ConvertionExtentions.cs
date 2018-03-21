using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class ConvertionExtentions
    {
        public static decimal? ToDecimalSafe(this double d)
        {
            if (double.IsNaN(d))
                return null;

            return (decimal)d;
        }

        public static decimal? ToDecimalSafe(this double? d)
        {
            if (d == null)
                return null;

            return ToDecimalSafe(d.Value);
        }
    }
}
