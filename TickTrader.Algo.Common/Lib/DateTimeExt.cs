using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class DateTimeExt
    {
        public static double GetAbsoluteDay(this DateTime val)
        {
            return (val - DateTime.MinValue).TotalDays;
        }

        public static DateTime FromTotalDays(double day)
        {
            return DateTime.MinValue + TimeSpan.FromDays(day);
        }
    }
}
