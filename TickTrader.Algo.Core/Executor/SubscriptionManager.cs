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

        public SubscriptionManager(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            foreach (var node in _context.MarketData.Nodes)
            {
                var entry = node.UserSubscriptions;
                if (entry != null && entry.GetMaxDepth() != 1)
                    UpdateSubscription(node.SymbolInfo.Name, entry);
            }
        }

        public void Stop()
        {
        }

        public void OnUpdateEvent(AlgoMarketNode node)
        {
            var collection = node.UserSubscriptions;
            if (collection != null)
            {
                var quote = node.Rate.LastQuote;
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
            foreach (var node in _context.MarketData.Nodes)
            {
                var collection = node.UserSubscriptions;
                if (collection != null)
                {
                    collection.UserSubscription = null;
                    collection.CurrentDepth = 1;
                }
            }
        }

        private void UpdateSubscription(string symbol, Collection subscribers)
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

        private Collection GetOrAddList(string symbolCode)
        {
            var node = _context.MarketData.GetSymbolNodeOrNull(symbolCode);

            if (node.UserSubscriptions == null)
                node.UserSubscriptions = new Collection();

            return node.UserSubscriptions;
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

        internal class Collection
        {
            public Collection()
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
