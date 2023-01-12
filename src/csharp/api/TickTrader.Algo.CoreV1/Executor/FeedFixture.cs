using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    internal abstract class FeedFixture
    {
        public FeedFixture(string symbolCode, IFixtureContext context)
        {
            this.SymbolCode = symbolCode;
            this.Context = context;
        }

        protected IFixtureContext Context { get; private set; }
        public string SymbolCode { get; private set; }
    }
}
