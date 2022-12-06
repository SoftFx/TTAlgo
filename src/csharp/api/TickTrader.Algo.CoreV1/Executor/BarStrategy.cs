using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public sealed class BarStrategy : FeedStrategy
    {
        private BarSampler _sampler;
        private BarSeriesFixture mainSeriesFixture;
        private List<BarData> mainSeries;
        private Dictionary<string, BarSeriesFixture[]> fixtures;

        public BarStrategy(Feed.Types.MarketSide mainMarketSide)
        {
            MainMarketSide = mainMarketSide;
        }

        public Feed.Types.MarketSide MainMarketSide { get; private set; }
        public override IFeedBuffer MainBuffer => mainSeriesFixture;
        public override int BufferSize => mainSeriesFixture.Count;
        public override IEnumerable<string> BufferedSymbols => fixtures.Keys;

        internal override void OnInit()
        {
            _sampler = BarSampler.Get(ExecContext.TimeFrame);
            fixtures = new Dictionary<string, BarSeriesFixture[]>();
            mainSeriesFixture = new BarSeriesFixture(ExecContext.MainSymbolCode, MainMarketSide, ExecContext, mainSeries);
            ExecContext.Builder.MainBufferId = GetKey(ExecContext.MainSymbolCode, MainMarketSide);

            AddFixture(ExecContext.MainSymbolCode, MainMarketSide, mainSeriesFixture);
        }

        protected override FeedStrategy CreateClone()
        {
            return new BarStrategy(MainMarketSide);
        }

        internal static Tuple<string, Feed.Types.MarketSide> GetKey(string symbol, Feed.Types.MarketSide marketSide)
        {
            return new Tuple<string, Feed.Types.MarketSide>(symbol, marketSide);
        }

        private void InitSymbol(string symbol, Feed.Types.MarketSide marketSide, bool isSetup)
        {
            BarSeriesFixture fixture = GetFixture(symbol, marketSide);
            if (fixture == null)
            {
                fixture = new BarSeriesFixture(symbol, marketSide, ExecContext, null, mainSeriesFixture);
                AddFixture(symbol, marketSide, fixture);
                if (!isSetup)
                    AddSubscription(symbol);
                BufferingStrategy.InitBuffer(fixture);
            }
        }

        private BarSeriesFixture GetFixture(string smbCode, Feed.Types.MarketSide marketSide)
        {
            if (fixtures.TryGetValue(smbCode, out var fixturePair))
            {
                if (marketSide == Feed.Types.MarketSide.Bid)
                    return fixturePair[0];
                else
                    return fixturePair[1];
            }
            return null;
        }

        private bool GetBothFixtures(string smbCode, out BarSeriesFixture bid, out BarSeriesFixture ask)
        {
            if (fixtures.TryGetValue(smbCode, out var fixturePair))
            {
                bid = fixturePair[0];
                ask = fixturePair[1];
                return true;
            }
            bid = null;
            ask = null;
            return false;
        }

        private void AddFixture(string smbCode, Feed.Types.MarketSide marketSide, BarSeriesFixture fixture)
        {
            if (!fixtures.TryGetValue(smbCode, out var fixturePair))
            {
                fixturePair = new BarSeriesFixture[2];
                fixtures.Add(smbCode, fixturePair);
            }

            if (marketSide == Feed.Types.MarketSide.Bid)
                fixturePair[0] = fixture;
            else
                fixturePair[1] = fixture;
        }

        protected override BufferUpdateResult UpdateBuffers(IRateInfo update)
        {
            var overallResult = new BufferUpdateResult();

            GetBothFixtures(update.Symbol, out var bidFixture, out var askFixture);

            if (askFixture != null)
            {
                var askResult =  askFixture.Update(update);
                if (update.Symbol != mainSeriesFixture.SymbolCode || MainMarketSide != Feed.Types.MarketSide.Ask)
                    askResult.ExtendedBy = 0;
                overallResult += askResult;
            }

            if (bidFixture != null)
            {
                var bidResult = bidFixture.Update(update);
                if (update.Symbol != mainSeriesFixture.SymbolCode || MainMarketSide != Feed.Types.MarketSide.Bid)
                    bidResult.ExtendedBy = 0;
                overallResult += bidResult;
            }

            if (overallResult.ExtendedBy > 0)
                BufferingStrategy.OnBufferExtended();

            return overallResult;
        }

        protected override BarSeries GetBarSeries(string symbol)
        {
            return GetBarSeries(symbol, MainMarketSide);
        }

        protected override BarSeries GetBarSeries(string symbol, Feed.Types.MarketSide side)
        {
            InitSymbol(symbol, side, false);
            var fixture = GetFixture(symbol, side);
            var proxyBuffer = new ProxyBuffer<BarData, Api.Bar>(b => new BarEntity(b)) { SrcBuffer = fixture.Buffer };
            return new BarSeriesProxy() { Buffer = proxyBuffer };
        }

        #region Setup

        public void MapInput<TVal>(string inputName, string symbolCode, Feed.Types.MarketSide marketSide, Func<BarData, TVal> selector)
        {
            AddSetupAction(new MapBarAction<TVal>(inputName, symbolCode, marketSide, selector));
        }

        public void MapInput<TVal>(string inputName, string symbolCode, Func<BarData, BarData, TVal> selector)
        {
            AddSetupAction(new MapDBarAction<TVal>(inputName, symbolCode, selector));
        }

        public void MapInput(string inputName, string symbolCode, Feed.Types.MarketSide marketSide)
        {
            MapInput<Api.Bar>(inputName, symbolCode, marketSide, b => new BarEntity(b));
        }

        #endregion Setup

        internal override void Stop()
        {
            base.Stop();

            //foreach (var fixture in fixtures.Values)
            //    fixture.Dispose();
        }


        public class MapBarAction<TVal> : InputSetupAction
        {
            public MapBarAction(string inputName, string symbol, Feed.Types.MarketSide marketSide, Func<BarData, TVal> selector) : base(inputName, symbol)
            {
                Selector = selector;
                MarketSide = marketSide;
            }

            public Feed.Types.MarketSide MarketSide { get; }
            public Func<BarData, TVal> Selector { get; }

            public override void Apply(FeedStrategy fStartegy)
            {
                ((BarStrategy)fStartegy).InitSymbol(SymbolName, MarketSide, true);
                var key = GetKey(SymbolName, MarketSide);
                fStartegy.ExecContext.Builder.MapInput(InputName, key, Selector);
            }
        }


        public class MapDBarAction<TVal> : InputSetupAction
        {
            public MapDBarAction(string inputName, string symbol, Func<BarData, BarData, TVal> selector) : base(inputName, symbol)
            {
                Selector = selector;
            }

            public Func<BarData, BarData, TVal> Selector { get; }

            public override void Apply(FeedStrategy fStartegy)
            {
                var key1 = GetKey(SymbolName, Feed.Types.MarketSide.Bid);
                var key2 = GetKey(SymbolName, Feed.Types.MarketSide.Ask);
                ((BarStrategy)fStartegy).InitSymbol(SymbolName, Feed.Types.MarketSide.Ask, true);
                ((BarStrategy)fStartegy).InitSymbol(SymbolName, Feed.Types.MarketSide.Bid, true);
                fStartegy.ExecContext.Builder.MapInput(InputName, key1, key2, Selector);
            }
        }
    }
}
