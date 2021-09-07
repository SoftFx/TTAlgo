using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public abstract class BarSampler
    {
        private static readonly Dictionary<Feed.Types.Timeframe, BarSampler> CachedSamplers = new Dictionary<Feed.Types.Timeframe, BarSampler>();

        static BarSampler()
        {
            CachedSamplers.Add(Feed.Types.Timeframe.MN, new MonthSampler());
            CachedSamplers.Add(Feed.Types.Timeframe.W, new WeekSampler());
            CachedSamplers.Add(Feed.Types.Timeframe.D, new DurationSampler(TimeSpan.FromDays(1)));
            CachedSamplers.Add(Feed.Types.Timeframe.H4, new DurationSampler(TimeSpan.FromHours(4)));
            CachedSamplers.Add(Feed.Types.Timeframe.H1, new DurationSampler(TimeSpan.FromHours(1)));
            CachedSamplers.Add(Feed.Types.Timeframe.M30, new DurationSampler(TimeSpan.FromMinutes(30)));
            CachedSamplers.Add(Feed.Types.Timeframe.M15, new DurationSampler(TimeSpan.FromMinutes(15)));
            CachedSamplers.Add(Feed.Types.Timeframe.M5, new DurationSampler(TimeSpan.FromMinutes(5)));
            CachedSamplers.Add(Feed.Types.Timeframe.M1, new DurationSampler(TimeSpan.FromMinutes(1)));
            CachedSamplers.Add(Feed.Types.Timeframe.S10, new DurationSampler(TimeSpan.FromSeconds(10)));
            CachedSamplers.Add(Feed.Types.Timeframe.S1, new SecondSampler());
        }

        public static BarSampler Get(Feed.Types.Timeframe timeFrame)
        {
            if (CachedSamplers.TryGetValue(timeFrame, out var result))
                return result;
            throw new ArgumentException($"Timeframe '{timeFrame}' is not supported!");
        }

        public abstract BarBoundaries GetBar(Timestamp timepoint);
    }

    public struct BarBoundaries
    {
        public BarBoundaries(long openSeconds, long closeSeconds)
        {
            Open = new Timestamp { Seconds = openSeconds, Nanos = 0 };
            Close = new Timestamp { Seconds = closeSeconds, Nanos = 0 };
        }

        public Timestamp Open { get; }
        public Timestamp Close { get; }
    }

    internal class SecondSampler : BarSampler
    {
        public override BarBoundaries GetBar(Timestamp timepoint)
        {
            var s = timepoint.Seconds;
            return new BarBoundaries(s, s + 1);
        }
    }

    internal class DurationSampler : BarSampler
    {
        private readonly long _durationSeconds;

        public DurationSampler(TimeSpan duration)
        {
            _durationSeconds = (long)duration.TotalSeconds;
        }

        public override BarBoundaries GetBar(Timestamp timepoint)
        {
            var s = timepoint.Seconds;
            var delta = _durationSeconds - (s % _durationSeconds);
            var close = s + delta;
            var open = close - _durationSeconds;
            return new BarBoundaries(open, close);
        }
    }

    internal class WeekSampler : BarSampler
    {
        // 1970-01-01 - Thursday (timestamp zero)
        // 1970-01-04 - Sunday (start of week on forex)
        public static readonly long ShiftSeconds = (long)TimeSpan.FromDays(3).TotalSeconds;
        public static readonly long DurationSeconds = (long)TimeSpan.FromDays(7).TotalSeconds;

        public override BarBoundaries GetBar(Timestamp timepoint)
        {
            var s = timepoint.Seconds - ShiftSeconds;
            var weekDelta = DurationSeconds - (s % DurationSeconds);
            var weekEnd = s + weekDelta + ShiftSeconds;
            var weekStart = weekEnd - DurationSeconds;
            return new BarBoundaries(weekStart, weekEnd);
        }
    }

    internal class MonthSampler : BarSampler
    {
        public override BarBoundaries GetBar(Timestamp timepoint)
        {
            var t = timepoint.ToDateTime();
            var start = new DateTime(t.Year, t.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            return new BarBoundaries(start.ToTimestamp().Seconds, end.ToTimestamp().Seconds);
        }
    }
}
