using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;

namespace TickTrader.Algo.Core
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
            Market.Init(builder.Symbols, builder.Currencies);
        }

        public AlgoMarketNode UpdateRate(RateUpdate newRate)
        {
            var node = Market.GetSymbolNodeOrNull(newRate.Symbol);
            if (node != null)
            {
                node.SymbolInfo.UpdateRate(newRate.LastQuote);
                node.Update(newRate);
                //_subscriptions.OnUpdateEvent(node);
            }
            return node;
        }
    }
}
