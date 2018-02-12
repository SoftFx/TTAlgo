using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class DateTimeExt
    {
        public static int GetAbsoluteDay(this DateTime val)
        {
            return (int)(val - DateTime.MinValue).TotalDays;
        }

        public static DateTime FromTotalDays(int day)
        {
            return DateTime.MinValue + TimeSpan.FromDays(day);
        }
    }
}
