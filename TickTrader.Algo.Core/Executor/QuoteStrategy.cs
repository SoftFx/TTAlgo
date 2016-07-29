using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class QuoteStrategy : FeedStrategy
    {
        private IFeedStrategyContext context;
        private QuoteSeriesFixture mainSeries;

        public override ITimeRef TimeRef { get { return mainSeries; } }
        public override int BufferSize { get { return mainSeries.Count; } }

        public override BufferUpdateResults UpdateBuffers(FeedUpdate update)
        {
            return mainSeries.Update(update.Quote);
        }

        private void ThrowIfNotTickType<TSrc>()
        {
            if (!typeof(TSrc).Equals(typeof(QuoteEntity)))
                throw new InvalidOperationException("Wrong data type! TickStrategy only works with QuoteEntity data!");
        }

        internal override void MapInput<TSrc, TVal>(string inputName, string symbolCode, Func<TSrc, TVal> selector)
        {
            ThrowIfNotTickType<TSrc>();

            if(symbolCode != mainSeries.SymbolCode)
                throw new InvalidOperationException("Wrong symbol! TickStrategy does only suppot main symbol inputs!");

            context.Builder.MapInput(inputName, symbolCode, selector);
        }

        internal override void Start(PluginExecutor executor)
        {
            this.context = executor;

            if (mainSeries != null)
                mainSeries.Dispose();

            mainSeries = new QuoteSeriesFixture(executor.MainSymbolCode, executor);
        }

        internal override void Stop()
        {
            mainSeries.Dispose();
        }
    }
}
