using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class DelayedEventsQueue: ITimeEventSeries
    {
        private C5.IPriorityQueue<TimeEvent> _delayedQueue = new C5.IntervalHeap<TimeEvent>();

        public DateTime NextOccurrance { get; private set; }
        public bool IsEmpty => _delayedQueue.Count == 0;

        public TimeEvent Take()
        {
            var ev = _delayedQueue.DeleteMin();
            UpdateNext();
            return ev;
        }

        public void Add(TimeEvent delayedEvent)
        {
            _delayedQueue.Add(delayedEvent);
            UpdateNext();
        }

        private void UpdateNext()
        {
            if (_delayedQueue.Count > 0)
                NextOccurrance = _delayedQueue.FindMin().Time;
            else
                NextOccurrance = DateTime.MaxValue;
        }
    }
}
