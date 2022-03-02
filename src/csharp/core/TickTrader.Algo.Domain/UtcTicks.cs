using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Domain
{
    public readonly struct UtcTicks : IComparable<UtcTicks>, IEquatable<UtcTicks>
    {
        public static readonly UtcTicks Default = new UtcTicks(0);
        public static readonly UtcTicks MaxValue = new UtcTicks(DateTime.MaxValue.Ticks);

        public static UtcTicks Now => new UtcTicks(DateTime.UtcNow.Ticks);


        public long Value { get; }


        public UtcTicks(long value) => Value = value;

        public UtcTicks(DateTime time) => Value = time.ToUniversalTime().Ticks;

        public UtcTicks(Timestamp timestamp) => Value = TimeTicks.FromTimestamp(timestamp);


        public DateTime ToUtcDateTime() => new DateTime(Value, DateTimeKind.Utc);

        public DateTime ToLocalDateTime() => new DateTime(Value, DateTimeKind.Utc).ToLocalTime();

        public Timestamp ToTimestamp() => TimeTicks.ToTimestamp(Value);

        public UtcTicks RoundMs() => new UtcTicks(TimeTicks.FromMs(TimeMs.FromUtcTicks(Value)));

        public UtcTicks AddMs(long msCnt) => new UtcTicks(Value + TimeTicks.FromMs(msCnt));

        public UtcTicks AddTicks(long tickCnt) => new UtcTicks(Value + tickCnt);


        public override string ToString() => ToUtcDateTime().ToString();

        public int CompareTo(UtcTicks other) => Value.CompareTo(other.Value);

        public bool Equals(UtcTicks other) => Value == other.Value;

        public override bool Equals(object obj) => obj is UtcTicks other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public static UtcTicks operator +(UtcTicks time, TimeSpan period) => new UtcTicks(time.Value + period.Ticks);

        public static UtcTicks operator -(UtcTicks time, TimeSpan period) => new UtcTicks(time.Value - period.Ticks);

        public static TimeSpan operator -(UtcTicks t1, UtcTicks t2) => TimeSpan.FromTicks(t1.Value - t2.Value);

        public static TimeSpan operator -(DateTime t1, UtcTicks t2) => TimeSpan.FromTicks(t1.ToUniversalTime().Ticks - t2.Value);

        public static TimeSpan operator -(UtcTicks t1, DateTime t2) => TimeSpan.FromTicks(t1.Value - t2.ToUniversalTime().Ticks);

        public static bool operator ==(UtcTicks t1, UtcTicks t2) => t1.Value == t2.Value;

        public static bool operator !=(UtcTicks t1, UtcTicks t2) => t1.Value != t2.Value;

        public static bool operator >(UtcTicks t1, UtcTicks t2) => t1.Value > t2.Value;

        public static bool operator <(UtcTicks t1, UtcTicks t2) => t1.Value < t2.Value;

        public static bool operator >=(UtcTicks t1, UtcTicks t2) => t1.Value >= t2.Value;

        public static bool operator <=(UtcTicks t1, UtcTicks t2) => t1.Value <= t2.Value;
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
}
