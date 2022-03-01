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
            CachedSamplers.Add(Feed.Types.Timeframe.S1, new DurationSampler(TimeSpan.FromSeconds(1)));
        }

        public static BarSampler Get(Feed.Types.Timeframe timeFrame)
        {
            if (CachedSamplers.TryGetValue(timeFrame, out var result))
                return result;
            throw new ArgumentException($"Timeframe '{timeFrame}' is not supported!");
        }

        public abstract BarBoundaries GetBar(long timepoint);
    }

    public readonly struct BarBoundaries
    {
        public BarBoundaries(long openMs, long closeMs)
        {
            Open = openMs;
            Close = closeMs;
        }

        public long Open { get; }
        public long Close { get; }
    }

    internal class DurationSampler : BarSampler
    {
        private readonly long _durationMs;

        public DurationSampler(TimeSpan duration)
        {
            _durationMs = duration.Ticks / 10_000;
        }

        public override BarBoundaries GetBar(long timepoint)
        {
            var delta = _durationMs - (timepoint % _durationMs);
            var close = timepoint + delta;
            var open = close - _durationMs;
            return new BarBoundaries(open, close);
        }
    }

    internal class WeekSampler : BarSampler
    {
        // DateTime.MinValue - Monday
        // Sunday - start of week on forex
        public static readonly long ShiftMs = TimeSpan.FromDays(1).Ticks / 10_000;
        public static readonly long DurationMs = TimeSpan.FromDays(7).Ticks / 10_000;

        public override BarBoundaries GetBar(long timepoint)
        {
            var s = timepoint + ShiftMs;
            var weekDelta = DurationMs - (s % DurationMs);
            var weekEnd = s + weekDelta - ShiftMs;
            var weekStart = weekEnd - DurationMs;
            return new BarBoundaries(weekStart, weekEnd);
        }
    }

    internal class MonthSampler : BarSampler
    {
        public override BarBoundaries GetBar(long timepoint)
        {
            var t = TimeMs.ToUtc(timepoint);
            var start = new DateTime(t.Year, t.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);
            return new BarBoundaries(TimeMs.FromUtcTicks(start.Ticks), TimeMs.FromUtcTicks(end.Ticks));
        }
    }
}
