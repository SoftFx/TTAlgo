using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class SubscriptionFixture
    {
        private IFixtureContext context;

        public SubscriptionFixture(IFixtureContext context, string symbol, int depth)
        {
            this.context = context;
            this.Depth = depth;
            this.SymbolCode = symbol;
        }

        public int Depth { get; private set; }
        public string SymbolCode { get; private set; }

        public void OnUpdateEvent(Quote quote)
        {
            context.Builder.InvokeOnQuote(quote);
        }
    }
}
