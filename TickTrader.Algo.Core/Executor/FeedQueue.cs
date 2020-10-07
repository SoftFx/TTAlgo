using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class FeedQueue
    {
        private readonly Queue<RateUpdate> _queue = new Queue<RateUpdate>();
        private readonly Dictionary<string, RateUpdate> _lasts = new Dictionary<string, RateUpdate>();
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
            _lasts[bars.Symbol] = bars;
            _queue.Enqueue(bars);
        }

        public void Enqueue(QuoteEntity quote)
        {
            _lasts.TryGetValue(quote.Symbol, out var last);
            var newUpdate = _fStrategy.InvokeAggregate(last, quote);
            if (newUpdate != null)
            {
                _lasts[quote.Symbol] = newUpdate;
                _queue.Enqueue(newUpdate);
            }
        }

        public RateUpdate Dequeue()
        {
            var update = _queue.Dequeue();
            _lasts.TryGetValue(update.Symbol, out var last);
            if (update == last)
                _lasts.Remove(update.Symbol);
            return update;
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}
