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
            cachedSamplers.Add(Api.TimeFrames.W, new WeekSampler());
            cachedSamplers.Add(Api.TimeFrames.D, new FormulaSampler(TimeSpan.FromDays(1)));
            cachedSamplers.Add(Api.TimeFrames.H4, new FormulaSampler(TimeSpan.FromHours(4)));
            cachedSamplers.Add(Api.TimeFrames.H1, new FormulaSampler(TimeSpan.FromHours(1)));
            cachedSamplers.Add(Api.TimeFrames.M30, new FormulaSampler(TimeSpan.FromMinutes(1)));
            cachedSamplers.Add(Api.TimeFrames.M1, new FormulaSampler(TimeSpan.FromMinutes(1)));
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

        public abstract DateTime GetBarOpenTime(DateTime timepoint);
    }

    internal class FormulaSampler : BarSampler
    {
        private long spTicks;

        public FormulaSampler(TimeSpan sampleSize)
        {
            spTicks = sampleSize.Ticks;
        }

        public override DateTime GetBarOpenTime(DateTime timepoint)
        {
            var delta = (spTicks - (timepoint.Ticks % spTicks)) % spTicks;
            return new DateTime(timepoint.Ticks + delta, timepoint.Kind);
        }

        public DateTime RoundUp(DateTime point)
        {
            var delta = (spTicks - (point.Ticks % spTicks)) % spTicks;
            return new DateTime(point.Ticks + delta, point.Kind);
        }
    }

    internal class WeekSampler : BarSampler
    {
        public override DateTime GetBarOpenTime(DateTime timepoint)
        {
            int diff = timepoint.DayOfWeek - DayOfWeek.Sunday;
            if (diff < 0)
                diff += 7;
            return timepoint.AddDays(-diff).Date;
        }
    }
}
