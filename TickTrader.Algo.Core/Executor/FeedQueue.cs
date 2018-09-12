using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class FeedQueue
    {
        private Queue<RateUpdate> queue = new Queue<RateUpdate>();
        private Dictionary<string, RateUpdate> lasts = new Dictionary<string, RateUpdate>();
        private FeedStrategy _fStrategy;

        public FeedQueue(FeedStrategy fStrategy)
        {
            _fStrategy = fStrategy;
        }

        public int Count { get { return queue.Count; } }

        public void Enqueue(QuoteEntity quote)
        {
            RateUpdate last;
            lasts.TryGetValue(quote.Symbol, out last);
            RateUpdate newUpdate = _fStrategy.InvokeAggregate(last, quote);
            if (newUpdate != null)
            {
                lasts[quote.Symbol] = newUpdate;
                queue.Enqueue(newUpdate);
            }
        }

        public RateUpdate Dequeue()
        {
            var update = queue.Dequeue();
            var last = lasts[update.Symbol];
            if (update == last)
                lasts.Remove(update.Symbol);
            return update;
        }

        public void Clear()
        {
            queue.Clear();
        }
    }
}
