using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Lib
{
    public static class TimestampHelper
    {
        public const int NanosInMillisecond = 1_000_000;
        public const int NanosInSecond = 1_000_000_000;

        public static Timestamp ParseLocalDateTime(string dateTimeString)
        {
            return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.AssumeLocal).ToUniversalTime().ToTimestamp();
        }

        public static Timestamp ParseUtcDateTime(string dateTimeString)
        {
            return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime().ToTimestamp();
        }

        public static Timestamp FromDate(int year, int month, int day)
        {
            return FromDateAndTime(year, month, day, 0, 0, 0);
        }

        public static Timestamp FromDateAndTime(int year, int month, int day, int hour, int minute, int second)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            return dt.ToTimestamp();
        }

        public static Timestamp AddMilliseconds(this Timestamp time, int cnt)
        {
            long newNanos = time.Nanos + (long)NanosInMillisecond * cnt;
            var seconds = time.Seconds + System.Math.DivRem(newNanos, NanosInSecond, out var nanos);
            if (nanos < 0)
            {
                nanos += NanosInSecond;
                seconds -= 1;
            }
            return new Timestamp { Seconds = seconds, Nanos = (int)nanos };
        }
    }

    public static class TimeMsHelper
    {
        public static long ParseLocalDateTime(string dateTimeString)
        {
            return TimeMs.FromDateTime(DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.AssumeLocal));
        }

        public static long ParseUtcDateTime(string dateTimeString)
        {
            return TimeMs.FromDateTime(DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.AssumeUniversal));
        }

        public static long FromDate(int year, int month, int day)
        {
            return FromDateAndTime(year, month, day, 0, 0, 0);
        }

        public static long FromDateAndTime(int year, int month, int day, int hour, int minute, int second)
        {
            var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            return TimeMs.FromDateTime(dt);
        }
    }
}
