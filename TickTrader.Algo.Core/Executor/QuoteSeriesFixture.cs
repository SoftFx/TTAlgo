using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesFixture : FeedFixture, IFeedBuffer
    {
        private InputBuffer<Quote> buffer;

        public QuoteSeriesFixture(string symbolCode, IFixtureContext context, List<QuoteEntity> data = null) : base(symbolCode, context)
        {
            buffer = context.Builder.GetBuffer<Quote>(SymbolCode);
            if (data != null)
                AppendData(data);
        }

        public DateTime this[int index] => buffer[index].Time;
        public int Count { get { return buffer.Count; } }
        public bool IsLoaded { get; private set; }
        public int LastIndex => buffer.Count - 1;
        public DateTime OpenTime => buffer[0].Time;

        public event Action Appended { add { } remove { } }

        public DateTime? GetTimeAtIndex(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return buffer[index].Time;
        }

        public void LoadFeed(DateTime from, DateTime to)
        {
            var data = Context.FeedHistory.QueryTicks(SymbolCode, from, to, false);
            AppendData(data);
        }

        public void LoadFeed(int size)
        {
            var to = DateTime.Now + TimeSpan.FromDays(1);
            var data = Context.FeedHistory.QueryTicks(SymbolCode, to, -size, false);
            AppendData(data);
        }

        public void LoadFeed(DateTime from, int size)
        {
            var data = Context.FeedHistory.QueryTicks(SymbolCode, from, size, false);
            AppendData(data);
        }

        public void LoadFeedFrom(DateTime from)
        {
            throw new NotImplementedException();
        }

        public void SyncByTime()
        {
            throw new NotSupportedException("Synchronization by time is not supported for quote series!");
        }

        public BufferUpdateResult Update(Api.Quote quote)
        {
            buffer.Append(quote);
            return new BufferUpdateResult() { IsLastUpdated = true };
        }

        private void AppendData(List<QuoteEntity> data)
        {
            IsLoaded = true;
            buffer.AppendRange(data);
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
