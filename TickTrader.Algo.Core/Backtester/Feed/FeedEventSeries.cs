using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class FeedEventSeries : ITimeEventSeries, IDisposable
    {
        private IEnumerator<RateUpdate> _e;
        private RateUpdate _nextRate;

        public FeedEventSeries(FeedEmulator emulator)
        {
            _e = emulator.GetFeedStream().GetEnumerator();
            MoveNext();
        }

        public DateTime NextOccurrance { get; private set; }
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
            if (IsCompeted)
                NextOccurrance = DateTime.MaxValue;
            else
            {
                _nextRate = _e.Current;
                NextOccurrance = _nextRate.Time;
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
