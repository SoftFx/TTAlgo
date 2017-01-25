using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class SubscriptionManager
    {
        private IPluginFeedProvider feed;
        private Dictionary<string, SubscriptionSummary> subscribers = new Dictionary<string, SubscriptionSummary>();

        public SubscriptionManager(IPluginFeedProvider feed)
        {
            this.feed = feed;
        }

        public void Reset()
        {
            subscribers.Clear();
        }

        public void OnUpdateEvent(Quote quote)
        {
            GetListOrNull(quote.Symbol)?.OnUpdate(quote);
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

        [Serializable]
        private class CrossdomainRequest
        {
            public string Symbol { get; set; }
            public int Depth { get; set; }
            public IPluginFeedProvider Feed { get; set; }

            public void SetDepth()
            {
                Feed.SetSymbolDepth(Symbol, Depth);
            }

            //public void Unsubscribe()
            //{
            //    Feed.Unsubscribe(Symbol);
            //}
        }

        private void UpdateSubscription(string symbol, SubscriptionSummary subscribers)
        {
            int newDepth = subscribers.GetMaxDepth();
            if (newDepth != subscribers.CurrentDepth)
            {
                var request = new CrossdomainRequest() { Feed = feed, Symbol = symbol, Depth = newDepth };
                feed.Sync.Invoke(request.SetDepth);
            }
        }

        private SubscriptionSummary GetOrAddList(string symbolCode)
        {
            SubscriptionSummary list;
            if (!subscribers.TryGetValue(symbolCode, out list))
            {
                list = new SubscriptionSummary();
                subscribers.Add(symbolCode, list);
            }
            return list;
        }

        private SubscriptionSummary GetListOrNull(string symbolCode)
        {
            SubscriptionSummary list;
            subscribers.TryGetValue(symbolCode, out list);
            return list;
        }

        private static int GetMaxDepth(List<IFeedFixture> subscribersList)
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

        private class SubscriptionSummary
        {
            public SubscriptionSummary()
            {
                Subscribers = new List<IFeedFixture>();
                CurrentDepth = 1;
            }

            public List<IFeedFixture> Subscribers { get; private set; }
            public int CurrentDepth { get; private set; }

            public void Add(IFeedFixture fixture)
            {
                Subscribers.Add(fixture);
            }

            public void Remove(IFeedFixture fixture)
            {
                Subscribers.Remove(fixture);
            }

            public void OnUpdate(Quote quote)
            {
                Subscribers.ForEach(s => s.OnUpdateEvent(quote));
            }

            public int GetMaxDepth()
            {
                int max = 1;

                foreach (var s in Subscribers)
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
}
