using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
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
        public int Depth { get { return 1; } }

        //public abstract BufferUpdateResults Update(Quote quote);

        //private void UpdateOverallSbscription(string symbol, List<IFeedConsumer> subscribers)
        //{
        //    Context.Subscribe(symbol, GetMaxDepth(subscribers));
        //}

        public void OnBufferUpdated(Quote quote)
        {
        }

        public void OnUpdateEvent(Quote quote)
        {
        }
    }
}
