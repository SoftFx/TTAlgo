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
        private IFixtureContext _context;
        //private Dictionary<string, SubscriptionFixture> userSubscriptions = new Dictionary<string, SubscriptionFixture>();
        //private HashSet<IAllRatesSubscription> allSymbolsSubscribers = new HashSet<IAllRatesSubscription>();
        private Dictionary<string, SubscriptionCollection> subscribersBySymbol = new Dictionary<string, SubscriptionCollection>();
        //private DateTime _startTime;

        public SubscriptionManager(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            foreach (var entry in subscribersBySymbol)
            {
                if (entry.Value.GetMaxDepth() != 1)
                    UpdateSubscription(entry.Key, entry.Value);
            }
        }

        public void Stop()
        {
        }

        public void OnUpdateEvent(Quote quote)
        {
            var collection = GetListOrNull(quote.Symbol);
            if (collection != null)
            {
                if (quote.Time > collection.LastQuoteTime) // old quotes from snapshot should not be sent as new quotes
                    collection.OnUpdate(quote);
            }
        }

        public void SetUserSubscription(string symbol, int depth)
        {
            var list = GetOrAddList(symbol);
            if (list.UserSubscription != null && list.UserSubscription.Depth == depth)
                return;
            list.UserSubscription = new SubscriptionFixture(_context, symbol, depth);
            UpdateSubscription(symbol, list);
        }

        public void RemoveUserSubscription(string symbol)
        {
            var list = GetOrAddList(symbol);
            list.UserSubscription = null;
            UpdateSubscription(symbol, list);
        }

        public void ClearUserSubscriptions()
        {
            foreach (var collection in subscribersBySymbol.Values)
            {
                collection.UserSubscription = null;
                collection.CurrentDepth = 1;
            }
        }

        private void UpdateSubscription(string symbol, SubscriptionCollection subscribers)
        {
            if (_context == null)
                return;

            int newDepth = subscribers.GetMaxDepth();
            if (newDepth != subscribers.CurrentDepth)
            {
                var request = new CrossdomainRequest() { Feed = _context.FeedProvider, Symbol = symbol, Depth = newDepth };
                _context.FeedProvider.Sync.Invoke(request.SetDepth);
                subscribers.CurrentDepth = newDepth;
            }
        }

        private SubscriptionCollection GetOrAddList(string symbolCode)
        {
            SubscriptionCollection list;
            if (!subscribersBySymbol.TryGetValue(symbolCode, out list))
            {
                list = new SubscriptionCollection();
                subscribersBySymbol.Add(symbolCode, list);
            }
            return list;
        }

        private SubscriptionCollection GetListOrNull(string symbolCode)
        {
            SubscriptionCollection list;
            subscribersBySymbol.TryGetValue(symbolCode, out list);
            return list;
        }

        //private static int GetMaxDepth(List<IRateSubscription> subscribersList)
        //{
        //    int max = 1;

        //    foreach (var s in subscribersList)
        //    {
        //        if (s.Depth == 0)
        //            return 0;
        //        if (s.Depth > max)
        //            max = s.Depth;
        //    }

        //    return max;
        //}

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

        private class SubscriptionCollection
        {
            public SubscriptionCollection()
            {
                CurrentDepth = 1;
            }

            public SubscriptionFixture UserSubscription { get; set; }
            public int CurrentDepth { get; set; }
            public DateTime LastQuoteTime { get; set; }

            public void OnUpdate(Quote quote)
            {
                LastQuoteTime = quote.Time;
                UserSubscription?.OnUpdateEvent(quote);
            }

            public int GetMaxDepth()
            {
                //int max = 1;

                //foreach (var s in UseSubscriptions)
                //{
                //    if (s.Depth == 0)
                //        return 0;
                //    if (s.Depth > max)
                //        max = s.Depth;
                //}

                //return max;

                var result = UserSubscription?.Depth;

                if (result == null || result < 0)
                    return 1;

                return result.Value;
            }
        }
    }
}
