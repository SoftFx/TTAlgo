using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Math
{
    public abstract class BarSampler
    {
        private static readonly Dictionary<Api.TimeFrames, BarSampler> cachedSamplers = new Dictionary<Api.TimeFrames, BarSampler>();

        static BarSampler()
        {
            cachedSamplers.Add(Api.TimeFrames.MN, new MonthSampler());
            cachedSamplers.Add(Api.TimeFrames.W, new WeekSampler());
            cachedSamplers.Add(Api.TimeFrames.D, new FormulaSampler(TimeSpan.FromDays(1)));
            cachedSamplers.Add(Api.TimeFrames.H4, new FormulaSampler(TimeSpan.FromHours(4)));
            cachedSamplers.Add(Api.TimeFrames.H1, new FormulaSampler(TimeSpan.FromHours(1)));
            cachedSamplers.Add(Api.TimeFrames.M30, new FormulaSampler(TimeSpan.FromMinutes(30)));
            cachedSamplers.Add(Api.TimeFrames.M15, new FormulaSampler(TimeSpan.FromMinutes(15)));
            cachedSamplers.Add(Api.TimeFrames.M5, new FormulaSampler(TimeSpan.FromMinutes(5)));
            cachedSamplers.Add(Api.TimeFrames.M1, new FormulaSampler(TimeSpan.FromMinutes(1)));
            cachedSamplers.Add(Api.TimeFrames.S10, new FormulaSampler(TimeSpan.FromSeconds(10)));
            cachedSamplers.Add(Api.TimeFrames.S1, new FormulaSampler(TimeSpan.FromSeconds(1)));
        }

        public static BarSampler Get(Api.TimeFrames timeFrame)
        {
            BarSampler result;
            if (cachedSamplers.TryGetValue(timeFrame, out result))
                return result;
            throw new ArgumentException("Time frame '" + timeFrame + "' is not supported!");
        }

        internal BarSampler()
        {
        }

        public abstract BarBoundaries GetBar(DateTime timepoint);
    }

    public struct BarBoundaries
    {
        public BarBoundaries(DateTime start, DateTime end)
        {
            this.Open = start;
            this.Close = end;
        }

        public DateTime Open { get; private set; }
        public DateTime Close { get; private set; }
    }

    internal class FormulaSampler : BarSampler
    {
        private long spTicks;

        public FormulaSampler(TimeSpan sampleSize)
        {
            spTicks = sampleSize.Ticks;
        }

        public override BarBoundaries GetBar(DateTime timepoint)
        {
            var delta = spTicks - (timepoint.Ticks % spTicks);
            var endTicks = timepoint.Ticks + delta;
            var startTicks = endTicks - spTicks;
            return new BarBoundaries(new DateTime(startTicks, timepoint.Kind), new DateTime(endTicks, timepoint.Kind));
        }

        public DateTime RoundUp(DateTime point)
        {
            var delta = (spTicks - (point.Ticks % spTicks)) % spTicks;
            return new DateTime(point.Ticks + delta, point.Kind);
        }
    }

    internal class WeekSampler : BarSampler
    {
        public override BarBoundaries GetBar(DateTime timepoint)
        {
            int diff = timepoint.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
                diff += 7;
            var weekStart =  timepoint.AddDays(-diff).Date;
            var weekEnd = weekStart.AddDays(7).Date;
            return new BarBoundaries(weekStart, weekEnd);
        }
    }

    internal class MonthSampler : BarSampler
    {
        public override BarBoundaries GetBar(DateTime timepoint)
        {
            var start = new DateTime(timepoint.Year, timepoint.Month, 1);
            var end = start.AddMonths(1);
            return new BarBoundaries(start, end);
        }
    }
}
