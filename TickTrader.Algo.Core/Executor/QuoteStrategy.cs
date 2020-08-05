using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class QuoteStrategy : FeedStrategy
    {
        private QuoteSeriesFixture mainSeries;

        public override IFeedBuffer MainBuffer { get { return null; } }
        public override int BufferSize { get { return mainSeries.Count; } }
        public override IEnumerable<string> BufferedSymbols => mainSeries.SymbolCode.Yield();

        public QuoteStrategy()
        {
        }

        protected override BufferUpdateResult UpdateBuffers(IRateInfo update)
        {
            return mainSeries.Update(update.LastQuote);
        }

        protected override IRateInfo Aggregate(IRateInfo last, QuoteInfo quote)
        {
            return quote;
        }

        protected override FeedStrategy CreateClone()
        {
            return new QuoteStrategy();
        }

        public void MapInput<TVal>(string inputName, string symbolCode, Func<QuoteEntity, TVal> selector)
        {
            AddSetupAction(new MapAction<TVal>(inputName, symbolCode, selector));
        }

        internal override void OnInit()
        {
            //if (mainSeries != null)
            //    mainSeries.Dispose();

            mainSeries = new QuoteSeriesFixture(ExecContext.MainSymbolCode, ExecContext);
        }

        internal override void Stop()
        {
            base.Stop();
            //mainSeries.Dispose();
        }

        protected override BarSeries GetBarSeries(string symbol)
        {
            throw new NotImplementedException();
        }

        protected override BarSeries GetBarSeries(string symbol, BarPriceType side)
        {
            throw new NotImplementedException();
        }

        [Serializable]
        public class MapAction<TVal> : InputSetupAction
        {
            public MapAction(string inputName, string symbol, Func<QuoteEntity, TVal> selector) : base(inputName, symbol)
            {
                Selector = selector;
            }

            public Func<QuoteEntity, TVal> Selector { get; }

            public override void Apply(FeedStrategy fStartegy)
            {
                if (SymbolName != ((QuoteStrategy)fStartegy).mainSeries.SymbolCode)
                    throw new InvalidOperationException("Wrong symbol! TickStrategy does only suppot main symbol inputs!");

                fStartegy.ExecContext.Builder.MapInput(InputName, SymbolName, Selector);
            }
        }
    }
}
