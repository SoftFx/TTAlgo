using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Domain
{
    public static class TimeMs
    {
        private static readonly long UnixEpochOffset = FromUtcTicks(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks);


        public static DateTime ToUtc(long utcMs) => new DateTime(utcMs * 10_000, DateTimeKind.Utc);

        public static Timestamp ToTimestamp(long utcMs)
        {
            var seconds = Math.DivRem(utcMs - UnixEpochOffset, 1_000, out var secPart);
            return new Timestamp { Seconds = seconds, Nanos = (int)secPart * 1_000_000 };
        }

        public static long FromDateTime(DateTime time) => FromUtcTicks(time.ToUniversalTime().Ticks);

        public static long FromUtcTicks(long utcTicks)
        {
            // correct near millisecond bound
            return (utcTicks / 625 + 1) >> 4;
        }

        public static long FromTimestamp(Timestamp timestamp)
        {
            var ms = (timestamp.Nanos / 62500 + 1) >> 4;
            return UnixEpochOffset + timestamp.Seconds * 1000 + ms;
        }
    }

    public static class TimeTicks
    {
        private static readonly long UnixEpochOffset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;


        public static DateTime ToUtc(long utcTicks) => new DateTime(utcTicks, DateTimeKind.Utc);

        public static Timestamp ToTimestamp(long utcTicks)
        {
            var seconds = Math.DivRem(utcTicks - UnixEpochOffset, 10_000_000, out var secPart);
            return new Timestamp { Seconds = seconds, Nanos = (int)secPart * 100 };
        }

        public static long FromDateTime(DateTime time) => time.ToUniversalTime().Ticks;

        public static long FromTimestamp(Timestamp timestamp) => UnixEpochOffset + timestamp.Seconds * 10_000_000 + timestamp.Nanos / 100;

        public static long FromMs(long utcMs) => utcMs * 10_000;
    }
}
