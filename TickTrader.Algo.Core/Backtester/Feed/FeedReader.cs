using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class FeedReader : ITimeEventSeries, IDisposable
    {
        private IEnumerator<RateUpdate> _e;
        private RateUpdate _nextRate;

        public FeedReader(FeedEmulator emulator)
        {
            _e = emulator.GetFeedStream().GetEnumerator();
            MoveNext();
        }

        public DateTime NextOccurrance => _nextRate.Time;
        public bool IsCompeted { get; private set; }

        public RateUpdate Take()
        {
            var item = _nextRate;
            MoveNext();
            return item;
        }

        TimeEvent ITimeEventSeries.Take()
        {
            var item = _nextRate;
            MoveNext();
            return new TimeEvent(item.Time, false, item);
        }

        private void MoveNext()
        {
            IsCompeted = !_e.MoveNext();
            _nextRate = _e.Current;
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
