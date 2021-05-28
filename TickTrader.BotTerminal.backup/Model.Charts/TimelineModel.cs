using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Model.Charts
{
    internal class TimelineModel
    {
        public TimelineModel(ITimeSampler sampler)
        {
        }
    }

    interface ITimeSampler
    {
        DateTime RoundUp(DateTime point);
    }

    internal class MathSampler : ITimeSampler
    {
        private long spTicks;

        public MathSampler(TimeSpan sampleSize)
        {
            spTicks = sampleSize.Ticks;
        }

        public DateTime RoundUp(DateTime point)
        {
            var delta = (spTicks - (point.Ticks % spTicks)) % spTicks;
            return new DateTime(point.Ticks + delta, point.Kind);
        }
    }
}
