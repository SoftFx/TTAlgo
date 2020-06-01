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
    internal class SubscriptionFixtureManager : QuoteDistributor
    {
        private readonly IFixtureContext _context;
        private readonly Dictionary<string, IFeedSubscription> _userSubscriptions = new Dictionary<string, IFeedSubscription>();
        private readonly MarketStateFixture _marketFixture;

        public SubscriptionFixtureManager(IFixtureContext context, MarketStateFixture marketFixture)
        {
            _context = context;
            _marketFixture = marketFixture;
        }

        public bool IsSynchronized { get; set; }

        public void Start()
        {
            Start(_context.FeedProvider, _context.Builder.Symbols.Select(s => s.Name));
        }

        public void Stop()
        {
            base.Stop(true);
        }

        protected override List<QuoteEntity> ModifySourceSubscription(List<FeedSubscriptionUpdate> updates)
        {
            using (var request = new CrossdomainRequest { Feed = _context.FeedProvider, Updates = updates })
            {
                if (IsSynchronized)
                    request.Modify();
                else
                    _context.FeedProvider.Sync.Invoke(request.Modify);
                return request.Result;
            }
        }

        public void OnUpdateEvent(AlgoMarketNode node)
        {
            var sub = node?.UserSubscriptionInfo;
            if (sub != null)
            {
                var quote = node.Rate.LastQuote;
                _context.Builder.InvokeOnQuote(quote);
                //if (quote.Time >= sub.LastQuoteTime) // old quotes from snapshot should not be sent as new quotes
                //  collection.OnUpdate(quote);
            }
        }

        internal override SubscriptionGroup GetGroupOrDefault(string symbol)
        {
            var node = _marketFixture.Market.GetSymbolNodeOrNull(symbol);
            return node?.SubGroup;
        }

        internal override SubscriptionGroup GetOrAddGroup(string symbol)
        {
            var node = _marketFixture.Market.GetSymbolNodeOrNull(symbol);

            if (node == null)
                throw new AlgoException($"Symbol {symbol} has not been added for emulation");

            if (node.SubGroup == null)
                node.SubGroup = new SubscriptionGroup(symbol);
            return node.SubGroup;
        }

        public void SetUserSubscription(string symbol, int depth)
        {
            var node = _context.MarketData.GetSymbolNodeOrNull(symbol);

            if (node != null)
            {
                if (node.UserSubscriptionInfo == null)
                    node.UserSubscriptionInfo = AddSubscription(q => { }, symbol, depth);
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

        private class CrossdomainRequest : CrossDomainObject
        {
            public List<FeedSubscriptionUpdate> Updates { get; set; }
            public IFeedProvider Feed { get; set; }
            public List<QuoteEntity> Result { get; set; }

            public void Modify()
            {
                Result = Feed.Modify(Updates);
            }

            public void CancellAll()
            {
                Feed.CancelAll();
            }
        }
    }
}
