using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class SubscriptionFixtureManager : SubscriptionManager
    {
        private IFixtureContext _context;
        private Dictionary<string, IFeedSubscription> _userSubscriptions = new Dictionary<string, IFeedSubscription>();

        public SubscriptionFixtureManager(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            Start(_context.FeedProvider, _context.Builder.Symbols.Select(s => s.Name));
        }

        public void Stop()
        {
            base.Stop(true);
        }

        protected override void ModifySourceSubscription(List<FeedSubscriptionUpdate> updates)
        {
            var request = new CrossdomainRequest { Feed = _context.FeedProvider, Updates = updates };
            _context.FeedProvider.Sync.Invoke(request.Modify);
        }

        public void OnUpdateEvent(AlgoMarketNode node)
        {
            var sub = node.UserSubscriptionInfo;
            if (sub != null)
            {
                var quote = node.Rate.LastQuote;
                _context.Builder.InvokeOnQuote(quote);
                //if (quote.Time >= sub.LastQuoteTime) // old quotes from snapshot should not be sent as new quotes
                //  collection.OnUpdate(quote);
            }
        }

        public void SetUserSubscription(string symbol, int depth)
        {
            var node = _context.MarketData.GetSymbolNodeOrNull(symbol);

            if (node != null)
            {
                if (node.UserSubscriptionInfo == null)
                    node.UserSubscriptionInfo = AddSubscription(symbol, depth);
                else
                    node.UserSubscriptionInfo.AddOrModify(symbol, depth);
            }
        }

        public void RemoveUserSubscription(string symbol)
        {
            var node = _context.MarketData.GetSymbolNodeOrNull(symbol);

            if (node != null && node.UserSubscriptionInfo != null)
            {
                node.UserSubscriptionInfo.CancelAll();
                node.UserSubscriptionInfo = null;
            }
        }

        public void ClearUserSubscriptions()
        {
            foreach (var node in _context.MarketData.Nodes)
            {
                node.UserSubscriptionInfo?.CancelAll();
                node.UserSubscriptionInfo = null;
            }
        }

        [Serializable]
        private class CrossdomainRequest
        {
            public List<FeedSubscriptionUpdate> Updates { get; set; }
            public IFeedProvider Feed { get; set; }

            public void Modify()
            {
                Feed.Modify(Updates);
            }

            public void CancellAll()
            {
                Feed.CancelAll();
            }
        }
    }
}
