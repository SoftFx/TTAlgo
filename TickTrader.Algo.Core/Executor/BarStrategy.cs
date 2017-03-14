﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public sealed class BarStrategy : FeedStrategy
    {
        private BarSeriesFixture mainSeriesFixture;
        private List<BarEntity> mainSeries;
        private Dictionary<Tuple<string, BarPriceType>, BarSeriesFixture> fixtures;

        public BarStrategy(IPluginFeedProvider feed, BarPriceType mainPirceTipe)
            : base(feed)
        {
            this.MainPriceType = mainPirceTipe;
        }

        public BarPriceType MainPriceType { get; private set; }
        public override ITimeRef TimeRef { get { return mainSeriesFixture; } }
        public override int BufferSize { get { return mainSeriesFixture.Count; } }

        internal override void OnInit()
        {
            fixtures = new Dictionary<Tuple<string, BarPriceType>, BarSeriesFixture>();
            mainSeriesFixture = new BarSeriesFixture(ExecContext.MainSymbolCode, MainPriceType, this, mainSeries);

            fixtures.Add(GetKey(ExecContext.MainSymbolCode, MainPriceType), mainSeriesFixture);
        }

        internal static Tuple<string, BarPriceType> GetKey(string symbol, BarPriceType priceType)
        {
            return new Tuple<string, BarPriceType>(symbol, priceType);
        }

        private void InitSymbol(Tuple<string, BarPriceType> key)
        {
            BarSeriesFixture fixture;
            if (!fixtures.TryGetValue(key, out fixture))
            {
                fixture = new BarSeriesFixture(key.Item1, key.Item2, this, null, mainSeriesFixture);
                fixtures.Add(key, fixture);
            }
        }

        private BarSeriesFixture GetFixutre(string smbCode, BarPriceType priceType)
        {
            BarSeriesFixture fixture;
            fixtures.TryGetValue(GetKey(smbCode, priceType), out fixture);
            return fixture;
        }

        protected override BufferUpdateResult UpdateBuffers(RateUpdate update)
        {
            var overallResult = new BufferUpdateResult();

            var askFixture = GetFixutre(update.Symbol, BarPriceType.Ask);
            var bidFixture = GetFixutre(update.Symbol, BarPriceType.Bid);

            if (askFixture != null)
            {
                foreach (var quote in update.LastQuotes)
                {
                    var result = askFixture.Update(quote);
                    if (askFixture == mainSeriesFixture)
                        overallResult = result;
                }
            }

            if (bidFixture != null)
            {
                foreach (var quote in update.LastQuotes)
                {
                    var result = bidFixture.Update(quote);
                    if (bidFixture == mainSeriesFixture)
                        overallResult = result;
                }
            }

            return overallResult;
        }

        #region Setup

        private void ThrowIfNotbarType<TSrc>()
        {
            if (!typeof(TSrc).Equals(typeof(BarEntity)))
                throw new InvalidOperationException("Wrong data type! BarStrategy only works with BarEntity data!");
        }

        public void MapInput<TVal>(string inputName, string symbolCode, BarPriceType priceType, Func<BarEntity, TVal> selector)
        {
            AddSetupAction(() =>
            {
                var key = GetKey(symbolCode, priceType);
                InitSymbol(key);
                ExecContext.Builder.MapInput(inputName, key, selector);
            });
        }

        public void MapInput<TVal>(string inputName, string symbolCode, Func<BarEntity, BarEntity, TVal> selector)
        {
            AddSetupAction(() =>
            {
                var key1 = GetKey(symbolCode, BarPriceType.Bid);
                var key2 = GetKey(symbolCode, BarPriceType.Ask);
                InitSymbol(key1);
                InitSymbol(key2);
                ExecContext.Builder.MapInput(inputName, key1, key2, selector);
            });
        }

        public void MapInput(string inputName, string symbolCode, BarPriceType priceType)
        {
            MapInput<Api.Bar>(inputName, symbolCode, priceType, b => b);
        }

        public void SetMainSeries(List<BarEntity> mainSeries)
        {
            this.mainSeries = mainSeries;
        }

        #endregion Setup

        internal override void Stop()
        {
            base.Stop();

            foreach (var fixture in fixtures.Values)
                fixture.Dispose();
        }
    }
}