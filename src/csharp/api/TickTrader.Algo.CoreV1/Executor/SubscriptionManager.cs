using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class SubscriptionFixtureManager
    {
        private readonly IFixtureContext _context;
        private readonly MarketStateFixture _marketFixture;
        private IQuoteSub _subscription;

        public SubscriptionFixtureManager(IFixtureContext context, MarketStateFixture marketFixture)
        {
            _context = context;
            _marketFixture = marketFixture;
        }

        public void Start()
        {
            _subscription = _context.FeedProvider.GetQuoteSub();
        }

        public void Stop()
        {
            _subscription.Dispose();
        }

        public void OnUpdateEvent(SymbolMarketNode node)
        {
            var sub = node?.UserSubscriptionInfo;

            if (sub != null)
            {
                var quote = node.SymbolInfo.LastQuote;
                _context.Builder.InvokeOnQuote(new QuoteEntity((QuoteInfo)quote));
            }
        }

        public void SetUserSubscription(string symbol, int depth)
        {
            var node = _context.MarketData.GetSymbolNodeOrNull(symbol);

            if (node != null)
            {
                var update = QuoteSubUpdate.Upsert(symbol, depth);
                node.UserSubscriptionInfo = update;
                _subscription.Modify(update);
            }
        }

        public void RemoveUserSubscription(string symbol)
        {
            var node = _context.MarketData.GetSymbolNodeOrNull(symbol);

            if (node != null && node.UserSubscriptionInfo != null)
            {
                node.UserSubscriptionInfo = null;
                _subscription.Modify(QuoteSubUpdate.Remove(symbol));
            }
        }

        public void ClearUserSubscriptions() => _context.MarketData.ClearUserSubscriptions();
    }
}
