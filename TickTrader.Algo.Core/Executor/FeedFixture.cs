using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal abstract class FeedFixture : IFeedFixture
    {
        public FeedFixture(string symbolCode, IFeedFixtureContext context)
        {
            this.SymbolCode = symbolCode;
            this.Context = context;

            context.Add(this);
        }

        protected IFeedFixtureContext Context { get; private set; }
        public string SymbolCode { get; private set; }
        public int Depth { get { return 1; } }

        public abstract BufferUpdateResults Update(QuoteEntity quote);

        //private void UpdateOverallSbscription(string symbol, List<IFeedConsumer> subscribers)
        //{
        //    Context.Subscribe(symbol, GetMaxDepth(subscribers));
        //}

        public void Dispose()
        {
            Context.Remove(this);
        }

        public void OnBufferUpdated(QuoteEntity quote)
        {
        }

        public void OnUpdateEvent(QuoteEntity quote)
        {
        }
    }
}
