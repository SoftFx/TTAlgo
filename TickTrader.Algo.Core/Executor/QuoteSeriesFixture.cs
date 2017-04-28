using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesFixture : FeedFixture
    {
        private InputBuffer<Quote> buffer;

        public QuoteSeriesFixture(string symbolCode, IFeedFixtureContext context, List<QuoteEntity> data = null) : base(symbolCode, context)
        {
            var execContext = context.ExecContext;
            var feed = context.Feed;
            if (data != null)
                data = feed.QueryTicks(SymbolCode, execContext.TimePeriodStart, execContext.TimePeriodEnd, 1);

            buffer = execContext.Builder.GetBuffer<Quote>(SymbolCode);
            if (data != null)
                buffer.AppendRange(data);
        }

        public int Count { get { return buffer.Count; } }

        public DateTime? GetTimeAtIndex(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return buffer[index].Time;
        }

        public BufferUpdateResult Update(Api.Quote quote)
        {
            buffer.Append(quote);
            return new BufferUpdateResult() { IsLastUpdated = true };
        }
    }

    //internal class TickSeriesL2Fixture : FeedFixture, ITimeRef
    //{
    //    private InputBuffer<QuoteEntityL2> buffer;

    //    public TickSeriesL2Fixture(string symbolCode, IFeedStrategyContext context) : base(symbolCode, context)
    //    {
    //        var data = context.QueryTicksL2(SymbolCode, context.TimePeriodStart, context.TimePeriodEnd, context.TimeFrame);

    //        buffer = context.Builder.GetBuffer<QuoteEntityL2>(SymbolCode);
    //        if (data != null)
    //            buffer.Append(data);
    //    }

    //    public int Count { get { return buffer.Count; } }

    //    public DateTime? GetTimeAtIndex(int index)
    //    {
    //        if (index < 0 || index >= Count)
    //            return null;
    //        return buffer[index].Time;
    //    }

    //    public override BufferUpdateResults Update(QuoteEntity quote)
    //    {
    //        buffer.Append(quote);
    //        return BufferUpdateResults.Extended;
    //    }
    //}
}
