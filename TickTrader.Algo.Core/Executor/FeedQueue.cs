using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class FeedQueue
    {
        private Queue<RateUpdate> _queue = new Queue<RateUpdate>();
        private FeedStrategy _fStrategy;

        public FeedQueue(FeedStrategy fStrategy)
        {
            _fStrategy = fStrategy;
        }

        public int Count { get { return _queue.Count; } }

        public void Enqueue(RateUpdate rate)
        {
            if (rate is QuoteEntity)
                Enqueue((QuoteEntity)rate);
            else if (rate is BarRateUpdate)
                Enqueue((BarRateUpdate)rate);
            else
                throw new Exception("Unsupported implementation of RateUpdate!");
        }

        public void Enqueue(BarRateUpdate bars)
        {
            RateUpdate newUpdate = _fStrategy.InvokeAggregate(bars);
            if (newUpdate != null)
            {
                _queue.Enqueue(bars);
            }
        }

        public void Enqueue(QuoteEntity quote)
        {
            RateUpdate newUpdate = _fStrategy.InvokeAggregate(quote);
            if (newUpdate != null)
            {
                _queue.Enqueue(newUpdate);
            }
        }

        public RateUpdate Dequeue()
        {
            var update = _queue.Dequeue();
            return update;
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}
