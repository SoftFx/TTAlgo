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

        public static DateTime DayFloor(this DateTime val)
        {
            return val.Date.Ticks == val.Ticks ? val : val.Date.AddDays(1);
        }
    }
}
