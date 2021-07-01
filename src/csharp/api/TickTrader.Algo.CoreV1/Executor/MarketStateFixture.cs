using System.Linq;
using TickTrader.Algo.Calculator.AlgoMarket;

namespace TickTrader.Algo.CoreV1
{
    internal class MarketStateFixture
    {
        private readonly IFixtureContext _context;
        //private readonly SubscriptionManager _subscriptions;

        public MarketStateFixture(IFixtureContext context)
        {
            _context = context;
            //_subscriptions = subscriptions;
            Market = new AlgoMarketState();
        }

        public AlgoMarketState Market { get; }

        public void Init()
        {
            var builder = _context.Builder;
            Market.Init(builder.Account, builder.Symbols.Values.Select(u => u.Info), builder.Currencies.Values.Select(u => u.Info));
        }
    }
}
