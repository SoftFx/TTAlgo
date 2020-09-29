using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Core
{
    internal class TimeRefFixture : ITimeRef
    {
        private List<DateTime> _timeline = new List<DateTime>();
        private BarSampler _sampler;
        private IFixtureContext _context;


        public DateTime this[int index] => _timeline[index];

        public int LastIndex => _timeline.Count - 1;

        public DateTime LastTime => _timeline[LastIndex];

        public bool IsRealTime { get; }


        public event Action Appended;


        public TimeRefFixture(IFixtureContext context)
        {
            _context = context;

            IsRealTime = context.ModelTimeFrame.IsTicks();
            if (!IsRealTime)
                _sampler = BarSampler.Get(context.ModelTimeFrame);
        }


        public BufferUpdateResult Update(DateTime time)
        {
            var barBoundaries = _sampler.GetBar(time);
            var barOpenTime = barBoundaries.Open;

            if (!_context.BufferingStrategy.InBoundaries(barOpenTime))
                return new BufferUpdateResult();

            if (_timeline.Count > 0)
            {
                var lastOpenTime = _timeline[_timeline.Count - 1];

                if (barOpenTime < lastOpenTime)
                    return new BufferUpdateResult();
                if (barOpenTime == lastOpenTime)
                    return new BufferUpdateResult { IsLastUpdated = true };
            }

            _timeline.Add(barOpenTime);
            Appended?.Invoke();
            return new BufferUpdateResult { ExtendedBy = 1 };
        }

        public void InitTimeline(List<BarEntity> bars)
        {
            if (bars != null)
            {
                var cnt = bars.Count;
                _timeline = new List<DateTime>(2 * cnt);
                for (var i = 0; i < cnt; i++)
                {
                    _timeline.Add(bars[i].OpenTime);
                }
            }
        }
    }
}
