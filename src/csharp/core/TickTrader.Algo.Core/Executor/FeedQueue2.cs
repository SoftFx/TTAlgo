using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class FeedUpdateSummary
    {
        public List<BarRateUpdate> BarUpdates { get; } = new List<BarRateUpdate>();

        public List<QuoteInfo> NewQuotes { get; } = new List<QuoteInfo>();
    }


    public interface IFeedQueue
    {
        void GetFeedUpdate(FeedUpdateSummary update);
    }


    public class FeedQueue2 : IFeedQueue
    {
        private readonly Queue<BarRateUpdate> _queue = new Queue<BarRateUpdate>();
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
                var update = new BarRateUpdate(bar.BidData, bar.AskData, bar.Symbol);
                _lastBars[bar.Symbol] = update;
                _queue.Enqueue(update);
            }
        }

        public void Enqueue(QuoteInfo quote)
        {
            _newQuotes[quote.Symbol] = quote;
        }

        public void GetFeedUpdate(FeedUpdateSummary update)
        {
            update.BarUpdates.Clear();
            update.NewQuotes.Clear();

            update.BarUpdates.AddRange(_queue);
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
