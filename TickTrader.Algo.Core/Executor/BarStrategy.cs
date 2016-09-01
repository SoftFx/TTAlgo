﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    internal sealed class BarStrategy : FeedStrategy
    {
        private BarSeriesFixture mainSeries;
        private Dictionary<string, BarSeriesFixture> fixtures;

        public BarStrategy(IPluginFeedProvider feed)
            : base(feed)
        {
        }

        public override ITimeRef TimeRef { get { return mainSeries; } }
        public override int BufferSize { get { return mainSeries.Count; } }

        internal override void OnInit()
        {
            fixtures = new Dictionary<string, BarSeriesFixture>();
            mainSeries = new BarSeriesFixture(ExecContext.MainSymbolCode, this);
            fixtures.Add(ExecContext.MainSymbolCode, mainSeries);
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

        protected override BufferUpdateResults UpdateBuffers(FeedUpdate update)
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
            ExecContext.Builder.MapInput(inputName, symbolCode, selector);
        }

        internal override void Stop()
        {
            foreach (var fixture in fixtures.Values)
                fixture.Dispose();
        }
    }
}
