using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class FeedStrategy
    {
        internal FeedStrategy()
        {
        }

        public abstract int BufferSize { get; }
        public abstract ITimeRef TimeRef { get; }
        internal abstract void Start(PluginExecutor executor);
        public abstract BufferUpdateResults UpdateBuffers(FeedUpdate update);
        internal abstract void MapInput<TSrc, TVal>(string inputName, string symbolCode, Func<TSrc, TVal> selector);
        internal abstract void Stop();
    }

    [Serializable]
    public sealed class BarStrategy : FeedStrategy
    {
        private IFeedStrategyContext context;
        private BarSeriesFixture mainSeries;
        private Dictionary<string, BarSeriesFixture> fixtures = new Dictionary<string, BarSeriesFixture>();

        public BarStrategy()
        {
        }

        public override ITimeRef TimeRef { get { return mainSeries; } }
        public override int BufferSize { get { return mainSeries.Count; } }

        internal override void Start(PluginExecutor executor)
        {
            this.context = executor;

            if (mainSeries != null)
                fixtures.Clear();

            mainSeries = new BarSeriesFixture(executor.MainSymbolCode, executor);

            fixtures.Add(executor.MainSymbolCode, mainSeries);
        }

        private void InitSymbol(string symbolCode)
        {
            BarSeriesFixture fixture;
            if (!fixtures.TryGetValue(symbolCode, out fixture))
            {
                fixture = new BarSeriesFixture(symbolCode, context);
                fixtures.Add(symbolCode, fixture);
            }
        }

        private BarSeriesFixture GetFixutre(string smbCode)
        {
            BarSeriesFixture fixture;
            fixtures.TryGetValue(smbCode, out fixture);
            return fixture;
        }

        public override BufferUpdateResults UpdateBuffers(FeedUpdate update)
        {
            var fixture = GetFixutre(update.SymbolCode);

            if (fixture == null)
                return BufferUpdateResults.NotUpdated;

            return fixture.Update(update.Quote);
        }

        private void ThrowIfNotbarType<TSrc>()
        {
            if (!typeof(TSrc).Equals(typeof(BarEntity)))
                throw new InvalidOperationException("Wrong data type! BarStrategy only works with BarEntity data!");
        }

        internal override void MapInput<TSrc, TVal>(string inputName, string symbolCode, Func<TSrc, TVal> selector)
        {
            ThrowIfNotbarType<TSrc>();
            InitSymbol(symbolCode);
            context.Builder.MapInput(inputName, symbolCode, selector);
        }

        internal override void Stop()
        {
            foreach (var fixture in fixtures.Values)
                fixture.Dispose();
        }
    }
}
