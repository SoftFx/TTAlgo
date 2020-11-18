using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public sealed class BarStrategy : FeedStrategy
    {
        [NonSerialized]
        private BarSampler _sampler;
        [NonSerialized]
        private BarSeriesFixture mainSeriesFixture;
        private List<BarEntity> mainSeries;
        [NonSerialized]
        private Dictionary<string, BarSeriesFixture[]> fixtures;

        public BarStrategy(BarPriceType mainPriceType)
        {
            this.MainPriceType = mainPriceType;
        }

        public BarPriceType MainPriceType { get; private set; }
        public override IFeedBuffer MainBuffer { get { return mainSeriesFixture; } }
        public override int BufferSize { get { return mainSeriesFixture.Count; } }
        public override IEnumerable<string> BufferedSymbols => fixtures.Keys;

        internal override void OnInit()
        {
            _sampler = BarSampler.Get(ExecContext.TimeFrame);
            fixtures = new Dictionary<string, BarSeriesFixture[]>();
            mainSeriesFixture = new BarSeriesFixture(ExecContext.MainSymbolCode, MainPriceType, ExecContext, mainSeries);
            ExecContext.Builder.MainBufferId = GetKey(ExecContext.MainSymbolCode, MainPriceType);

            AddFixture(ExecContext.MainSymbolCode, MainPriceType, mainSeriesFixture);
        }

        protected override FeedStrategy CreateClone()
        {
            return new BarStrategy(MainPriceType);
        }

        internal static Tuple<string, BarPriceType> GetKey(string symbol, BarPriceType priceType)
        {
            return new Tuple<string, BarPriceType>(symbol, priceType);
        }

        private void InitSymbol(string symbol, BarPriceType priceType, bool isSetup)
        {
            BarSeriesFixture fixture = GetFixture(symbol, priceType);
            if (fixture == null)
            {
                fixture = new BarSeriesFixture(symbol, priceType, ExecContext, null, mainSeriesFixture);
                AddFixture(symbol, priceType, fixture);
                BufferingStrategy.InitBuffer(fixture);
                if (!isSetup)
                    AddSubscription(symbol);
            }
        }

        private BarSeriesFixture GetFixture(string smbCode, BarPriceType priceType)
        {
            BarSeriesFixture[] fixturePair;
            if (fixtures.TryGetValue(smbCode, out fixturePair))
            {
                if (priceType == BarPriceType.Bid)
                    return fixturePair[0];
                else
                    return fixturePair[1];
            }
            return null;
        }

        private bool GetBothFixtures(string smbCode, out BarSeriesFixture bid, out BarSeriesFixture ask)
        {
            BarSeriesFixture[] fixturePair;
            if (fixtures.TryGetValue(smbCode, out fixturePair))
            {
                bid = fixturePair[0];
                ask = fixturePair[1];
                return true;
            }
            bid = null;
            ask = null;
            return false;
        }

        private void AddFixture(string smbCode, BarPriceType priceType, BarSeriesFixture fixture)
        {
            BarSeriesFixture[] fixturePair;
            if (!fixtures.TryGetValue(smbCode, out fixturePair))
            {
                fixturePair = new BarSeriesFixture[2];
                fixtures.Add(smbCode, fixturePair);
            }

            if (priceType == BarPriceType.Bid)
                fixturePair[0] = fixture;
            else
                fixturePair[1] = fixture;
        }

        protected override BufferUpdateResult UpdateBuffers(RateUpdate update)
        {
            var overallResult = new BufferUpdateResult();

            GetBothFixtures(update.Symbol, out var bidFixture, out var askFixture);

            if (askFixture != null)
            {
                var askResult = askFixture.Update(update);
                if (update.Symbol != mainSeriesFixture.SymbolCode || MainPriceType != BarPriceType.Ask)
                    askResult.ExtendedBy = 0;
                overallResult += askResult;
            }

            if (bidFixture != null)
            {
                var bidResult = bidFixture.Update(update);
                if (update.Symbol != mainSeriesFixture.SymbolCode || MainPriceType != BarPriceType.Bid)
                    bidResult.ExtendedBy = 0;
                overallResult += bidResult;
            }

            if (overallResult.ExtendedBy > 0)
                BufferingStrategy.OnBufferExtended();

            return overallResult;
        }

        protected override RateUpdate Aggregate(RateUpdate last, QuoteEntity quote)
        {
            var bounds = _sampler.GetBar(quote.Time);

            if (last != null && last.Time == bounds.Open)
            {
                ((BarRateUpdate)last).Append(quote);
                return null;
            }
            else
            {
                return new BarRateUpdate(bounds.Open, bounds.Close, quote);
            }
        }

        protected override BarSeries GetBarSeries(string symbol)
        {
            return GetBarSeries(symbol, MainPriceType);
        }

        protected override BarSeries GetBarSeries(string symbol, BarPriceType side)
        {
            InitSymbol(symbol, side, false);
            var fixture = GetFixture(symbol, side);
            var proxyBuffer = new ProxyBuffer<BarEntity, Api.Bar>(b => b) { SrcBuffer = fixture.Buffer };
            return new BarSeriesProxy() { Buffer = proxyBuffer };
        }

        #region Setup

        private void ThrowIfNotbarType<TSrc>()
        {
            if (!typeof(TSrc).Equals(typeof(BarEntity)))
                throw new InvalidOperationException("Wrong data type! BarStrategy only works with BarEntity data!");
        }

        public void MapInput<TVal>(string inputName, string symbolCode, BarPriceType priceType, Func<BarEntity, TVal> selector)
        {
            AddSetupAction(new MapBarAction<TVal>(inputName, symbolCode, priceType, selector));
        }

        public void MapInput<TVal>(string inputName, string symbolCode, Func<BarEntity, BarEntity, TVal> selector)
        {
            AddSetupAction(new MapDBarAction<TVal>(inputName, symbolCode, selector));
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

            //foreach (var fixture in fixtures.Values)
            //    fixture.Dispose();
        }

        [Serializable]
        public class MapBarAction<TVal> : InputSetupAction
        {
            public MapBarAction(string inputName, string symbol, BarPriceType priceType, Func<BarEntity, TVal> selector) : base(inputName, symbol)
            {
                Selector = selector;
                PriceType = priceType;
            }

            public BarPriceType PriceType { get; }
            public Func<BarEntity, TVal> Selector { get; }

            public override void Apply(FeedStrategy fStartegy)
            {
                ((BarStrategy)fStartegy).InitSymbol(SymbolName, PriceType, true);
                var key = GetKey(SymbolName, PriceType);
                fStartegy.ExecContext.Builder.MapInput(InputName, key, Selector);
            }
        }

        [Serializable]
        public class MapDBarAction<TVal> : InputSetupAction
        {
            public MapDBarAction(string inputName, string symbol, Func<BarEntity, BarEntity, TVal> selector) : base(inputName, symbol)
            {
                Selector = selector;
            }

            public Func<BarEntity, BarEntity, TVal> Selector { get; }

            public override void Apply(FeedStrategy fStartegy)
            {
                var key1 = GetKey(SymbolName, BarPriceType.Bid);
                var key2 = GetKey(SymbolName, BarPriceType.Ask);
                ((BarStrategy)fStartegy).InitSymbol(SymbolName, BarPriceType.Ask, true);
                ((BarStrategy)fStartegy).InitSymbol(SymbolName, BarPriceType.Bid, true);
                fStartegy.ExecContext.Builder.MapInput(InputName, key1, key2, Selector);
            }
        }
    }
}
