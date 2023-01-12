using System;

namespace TickTrader.Algo.Domain
{
    public static class UtcTicksHelper
    {
        public static UtcTicks ParseLocalDateTime(string dateTimeString)
        {
            return new UtcTicks(DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.AssumeLocal));
        }

        public static UtcTicks ParseUtcDateTime(string dateTimeString)
        {
            return new UtcTicks(DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.AssumeUniversal));
        }

        public static UtcTicks FromDate(int year, int month, int day)
        {
            return FromDateAndTime(year, month, day, 0, 0, 0);
        }

        public static UtcTicks FromDateAndTime(int year, int month, int day, int hour, int minute, int second)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            return new UtcTicks(dt);
        }
    }
}
