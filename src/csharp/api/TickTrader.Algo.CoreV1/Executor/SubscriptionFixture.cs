using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
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
