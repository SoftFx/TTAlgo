using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    internal sealed class BarStrategy : FeedStrategy
    {
        private BarSeriesFixture mainSeriesFixture;
        private Dictionary<string, BarSeriesFixture> fixtures;

        public BarStrategy(IBarBasedFeed feed)
            : base(feed)
        {
        }

        public override ITimeRef TimeRef { get { return mainSeriesFixture; } }
        public override int BufferSize { get { return mainSeriesFixture.Count; } }

        internal override void OnInit()
        {
            fixtures = new Dictionary<string, BarSeriesFixture>();
            var mainSeries = ((IBarBasedFeed)Feed).GetMainSeries();
            mainSeriesFixture = new BarSeriesFixture(ExecContext.MainSymbolCode, this, mainSeries);

            fixtures.Add(ExecContext.MainSymbolCode, mainSeriesFixture);
        }

        private void InitSymbol(string symbolCode)
        {
            BarSeriesFixture fixture;
            if (!fixtures.TryGetValue(symbolCode, out fixture))
            {
                fixture = new BarSeriesFixture(symbolCode, this);
                fixtures.Add(symbolCode, fixture);
            }
        }

        private BarSeriesFixture GetFixutre(string smbCode)
        {
            BarSeriesFixture fixture;
            fixtures.TryGetValue(smbCode, out fixture);
            return fixture;
        }

        protected override BufferUpdateResult UpdateBuffers(RateUpdate update)
        {
            var overallResult = new BufferUpdateResult();
            var fixture = GetFixutre(update.Symbol);

            if (fixture != null)
            {
                foreach (var quote in update.LastQuotes)
                    overallResult += fixture.Update(quote);
            }
            return overallResult;
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
            ExecContext.Builder.MapInput(inputName, symbolCode, selector);
        }

        internal override void Stop()
        {
            base.Stop();

            foreach (var fixture in fixtures.Values)
                fixture.Dispose();
        }
    }
}
