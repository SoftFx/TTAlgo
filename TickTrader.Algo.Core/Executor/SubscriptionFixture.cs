using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class SubscriptionFixture : IFeedFixture
    {
        private IFeedStrategyContext context;

        public SubscriptionFixture(IFeedStrategyContext context, string symbol, int depth)
        {
            this.context = context;
            this.Depth = depth;
            this.SymbolCode = symbol;
        }

        public int Depth { get; private set; }
        public string SymbolCode { get; private set; }

        public void OnBufferUpdated(QuoteEntity quote)
        {
        }

        public void OnUpdateEvent(QuoteEntity quote)
        {
            context.InvokeUpdateOnCustomSubscription(quote);   
        }
    }
}
