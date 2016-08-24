using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    internal class SubscriptionManager
    {
        private IPluginFeedProvider feed;
        private Dictionary<string, List<IFeedFixture>> subscribers = new Dictionary<string, List<IFeedFixture>>();

        public SubscriptionManager(IPluginFeedProvider feed)
        {
            this.feed = feed;
        }

        public void OnBufferUpdated(QuoteEntity quote)
        {
            var list =  GetOrAddList(quote.SymbolCode);
            list.ForEach(s => s.OnBufferUpdated(quote));
        }

        public void OnUpdateEvent(QuoteEntity quote)
        {
            var list = GetOrAddList(quote.SymbolCode);
            list.ForEach(s => s.OnUpdateEvent(quote));
        }

        public void Add(IFeedFixture fixture)
        {
            var list = GetOrAddList(fixture.SymbolCode);
            list.Add(fixture);
            UpdateSubscription(fixture.SymbolCode, list);
        }

        public void Remove(IFeedFixture fixture)
        {
            var list = GetOrAddList(fixture.SymbolCode);
            list.Remove(fixture);
            UpdateSubscription(fixture.SymbolCode, list);
        }

        private void UpdateSubscription(string symbol, List<IFeedFixture> subscribers)
        {
            if (subscribers.Count == 0)
                feed.Unsubscribe(symbol);
            else
                feed.Subscribe(symbol, GetMaxDepth(subscribers));
        }

        private List<IFeedFixture> GetOrAddList(string symbolCode)
        {
            List<IFeedFixture> list;
            if (!subscribers.TryGetValue(symbolCode, out list))
            {
                list = new List<IFeedFixture>();
                subscribers.Add(symbolCode, list);
            }
            return list;
        }

        private int GetMaxDepth(List<IFeedFixture> subscribersList)
        {
            int max = 1;

            foreach (var s in subscribersList)
            {
                if (s.Depth == 0)
                    return 0;
                if (s.Depth > max)
                    max = s.Depth;
            }

            return max;
        }
    }
}
