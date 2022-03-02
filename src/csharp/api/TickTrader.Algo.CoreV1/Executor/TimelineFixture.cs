using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class TimelineFixture
    {
        private List<BarBoundaries> _timeline = new List<BarBoundaries>();
        private BarSampler _sampler;


        public BarBoundaries this[int index] => _timeline[index];

        public int LastIndex => _timeline.Count - 1;

        public BarBoundaries LastBounds => _timeline[LastIndex];

        public bool IsRealTime { get; }


        public event Action Appended;


        public TimelineFixture(Feed.Types.Timeframe timeFrame)
        {
            IsRealTime = timeFrame.IsTicks();
            if (!IsRealTime)
            {
                _timeline = new List<BarBoundaries>(1000);
                _sampler = BarSampler.Get(timeFrame);
            }
        }


        public BufferUpdateResult Update(UtcTicks time)
        {
            var barBoundaries = _sampler.GetBar(time);
            return Update(barBoundaries);
        }


        public BufferUpdateResult Update(BarBoundaries barBoundaries)
        {
            if (IsRealTime)
                return new BufferUpdateResult();

            var barOpenTime = barBoundaries.Open;

            if (_timeline.Count > 0)
            {
                var lastOpenTime = _timeline.Count == 0 ? UtcTicks.Default : _timeline[_timeline.Count - 1].Open;

                if (barOpenTime < lastOpenTime)
                    return new BufferUpdateResult();
                if (barOpenTime == lastOpenTime)
                    return new BufferUpdateResult { IsLastUpdated = true };
            }

            _timeline.Add(barBoundaries);
            Appended?.Invoke();
            return new BufferUpdateResult { ExtendedBy = 1 };
        }
    }
}
