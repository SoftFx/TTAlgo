using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal sealed class FeedEventSeries : ITimeEventSeries, IDisposable
    {
        private IEnumerator<IRateInfo> _e;


        public IRateInfo NextRate { get; private set; }

        public DateTime NextOccurrance { get; private set; }

        public bool IsCompeted { get; private set; }


        public FeedEventSeries(FeedEmulator emulator)
        {
            _e = emulator.GetFeedStream().GetEnumerator();
            MoveNext();
        }


        public IRateInfo Take()
        {
            var item = NextRate;
            MoveNext();
            return item;
        }

        TimeEvent ITimeEventSeries.Take()
        {
            var item = NextRate;
            MoveNext();
            return new TimeEvent(item.TimeUtc, false, item);
        }

        private void MoveNext()
        {
            IsCompeted = !_e.MoveNext();
            if (IsCompeted)
            {
                NextRate = null;
                NextOccurrance = DateTime.MaxValue;
            }
            else
            {
                NextRate = _e.Current;
                NextOccurrance = NextRate.TimeUtc;
            }
        }

        public void Dispose()
        {
            if (_e != null)
            {
                _e.Dispose();
                _e = null;
            }
        }
    }
}
