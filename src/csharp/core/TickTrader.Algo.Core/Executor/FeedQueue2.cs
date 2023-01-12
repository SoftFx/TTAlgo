using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class FeedUpdateSummary
    {
        public List<IRateInfo> RateUpdates { get; } = new List<IRateInfo>();

        public List<QuoteInfo> NewQuotes { get; } = new List<QuoteInfo>();
    }


    public interface IFeedQueue
    {
        void GetFeedUpdate(FeedUpdateSummary update);
    }


    public class FeedQueue2 : IFeedQueue
    {
        private readonly Queue<IRateInfo> _queue = new Queue<IRateInfo>();
        private readonly Dictionary<string, BarRateUpdate> _lastBars = new Dictionary<string, BarRateUpdate>();
        private readonly Dictionary<string, QuoteInfo> _newQuotes = new Dictionary<string, QuoteInfo>();


        public int Count => _queue.Count + _newQuotes.Count;


        public void Enqueue(BarUpdate bar)
        {
            var hasLastBar = _lastBars.TryGetValue(bar.Symbol, out var lastBar);
            if (hasLastBar && lastBar.OpenTime == bar.OpenTime)
                lastBar.Replace(bar);
            else if (hasLastBar && bar.OpenTime < lastBar.OpenTime)
                return; // Invalid time sequence
            else // if (!hasLastBar || (hasLastBar && bar.OpenTime > lastBar.OpenTime))
            {
                var update = new BarRateUpdate(bar);
                _lastBars[bar.Symbol] = update;
                _queue.Enqueue(update);
            }
        }

        public void Enqueue(QuoteInfo quote)
        {
            _newQuotes[quote.Symbol] = quote;
        }

        public void Enqueue(IRateInfo rate)
        {
            _queue.Enqueue(rate);
            _newQuotes[rate.Symbol] = rate.LastQuote;
        }

        public void GetFeedUpdate(FeedUpdateSummary update)
        {
            update.RateUpdates.Clear();
            update.NewQuotes.Clear();

            update.RateUpdates.AddRange(_queue);
            update.NewQuotes.AddRange(_newQuotes.Values);

            Clear();
        }

        public void Clear()
        {
            _queue.Clear();
            _lastBars.Clear();
            _newQuotes.Clear();
        }
    }
}
