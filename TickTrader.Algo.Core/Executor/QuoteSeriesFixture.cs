using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesFixture : FeedFixture, ITimeRef
    {
        private InputBuffer<QuoteEntity> buffer;

        public QuoteSeriesFixture(string symbolCode, IFeedStrategyContext context) : base(symbolCode, context)
        {
            var data = context.QueryTicks(SymbolCode, context.TimePeriodStart, context.TimePeriodEnd);

            buffer = context.Builder.GetBuffer<QuoteEntity>(SymbolCode);
            if (data != null)
                buffer.Append(data);
        }

        public int Count { get { return buffer.Count; } }

        public DateTime? GetTimeAtIndex(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return buffer[index].Time;
        }

        public override BufferUpdateResults Update(QuoteEntity quote)
        {
            buffer.Append(quote);
            return BufferUpdateResults.Extended;
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
