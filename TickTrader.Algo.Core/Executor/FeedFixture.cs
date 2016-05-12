using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal abstract class FeedFixture : IFeedFixture
    {
        public FeedFixture(string symbolCode, IFeedStrategyContext context)
        {
            this.SymbolCode = symbolCode;
            this.Context = context;

            context.Subscribe(symbolCode, 1);
        }

        protected IFeedStrategyContext Context { get; private set; }
        protected string SymbolCode { get; private set; }

        public void Add(IInternalFeedConsumer consumer)
        {
        }

        public void Remove(IInternalFeedConsumer consumer)
        {
        }

        public abstract BufferUpdateResults Update(QuoteEntity quote);

        private void UpdateOverallSbscription(string symbol, List<IInternalFeedConsumer> subscribers)
        {
            Context.Subscribe(symbol, GetMaxDepth(subscribers));
        }

        private int GetMaxDepth(List<IInternalFeedConsumer> subscribers)
        {
            int max = 1;

            foreach (var s in subscribers)
            {
                if (s.Depth == 0)
                    return 0;
                if (s.Depth > max)
                    max = s.Depth;
            }

            return max;
        }

        public void Dispose()
        {
            Context.Unsubscribe(SymbolCode);
        }
    }
}
