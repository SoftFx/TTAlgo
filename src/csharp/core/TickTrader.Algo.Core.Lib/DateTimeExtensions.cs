using System;

namespace TickTrader.Algo.Core.Lib
{
    public static class DateTimeExtensions
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
