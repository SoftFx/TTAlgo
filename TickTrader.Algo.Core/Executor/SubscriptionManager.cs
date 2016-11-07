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
        private Dictionary<string, List<IFeedFixture>> subscribers = new Dictionary<string, List<IFeedFixture>>();

        public SubscriptionManager(IPluginFeedProvider feed)
        {
            this.feed = feed;
        }

        //public void OnBufferUpdated(Quote quote)
        //{
        //    var list =  GetOrAddList(quote.Symbol);
        //    list.ForEach(s => s.OnBufferUpdated(quote));
        //}

        public void OnUpdateEvent(Quote quote)
        {
            var list = GetOrAddList(quote.Symbol);
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

        [Serializable]
        private class CrossdomainRequest
        {
            public string Symbol { get; set; }
            public int Depth { get; set; }
            public IPluginFeedProvider Feed { get; set; }

            public void Subscribe()
            {
                Feed.Subscribe(Symbol, Depth);
            }

            public void Unsubscribe()
            {
                Feed.Unsubscribe(Symbol);
            }
        }

        private void UpdateSubscription(string symbol, List<IFeedFixture> subscribers)
        {
            var request = new CrossdomainRequest() { Feed = feed, Symbol = symbol, Depth = GetMaxDepth(subscribers) };

            if (subscribers.Count > 0)
                feed.Sync.Invoke(request.Subscribe);
            else
                feed.Sync.Invoke(request.Unsubscribe);
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
    }
}
