using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class FeedUpdateSummary
    {
        public BarRateUpdate[] BarUpdates { get; }

        public QuoteInfo[] NewQuotes { get; }


        public FeedUpdateSummary(BarRateUpdate[] barUpdates, QuoteInfo[] newQuotes)
        {
            BarUpdates = barUpdates;
            NewQuotes = newQuotes;
        }
    }

    public interface IFeedQueue
    {
        FeedUpdateSummary GetFeedUpdate();
    }


    public class FeedQueue2 : IFeedQueue
    {
        private readonly Queue<BarRateUpdate> _queue = new Queue<BarRateUpdate>();
        private readonly Dictionary<string, BarRateUpdate> _lastBars = new Dictionary<string, BarRateUpdate>();
        private readonly Dictionary<string, QuoteInfo> _newQuotes = new Dictionary<string, QuoteInfo>();


        public int Count => _queue.Count + _newQuotes.Count;

        public IReadOnlyDictionary<string, QuoteInfo> PendingQuotes => _newQuotes;


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

        public FeedUpdateSummary GetFeedUpdate()
        {
            var barUpdates = _queue.ToArray();
            var quoteUpdates = _newQuotes.Values.ToArray();
            Clear();
            return new FeedUpdateSummary(barUpdates, quoteUpdates);
        }

        public void Clear()
        {
            _queue.Clear();
            _lastBars.Clear();
            _newQuotes.Clear();
        }
    }
}
