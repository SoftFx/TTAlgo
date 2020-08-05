using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesFixture : FeedFixture, IFeedBuffer
    {
        private InputBuffer<Quote> _buffer;

        public QuoteSeriesFixture(string symbolCode, IFixtureContext context) : base(symbolCode, context)
        {
            _buffer = context.Builder.GetBuffer<Quote>(SymbolCode);
        }

        public DateTime this[int index] => _buffer[index].Time;
        public int Count { get { return _buffer.Count; } }
        public bool IsLoaded { get; private set; }
        public int LastIndex => _buffer.Count - 1;
        public DateTime OpenTime => _buffer[0].Time;

        public event Action Appended { add { } remove { } }

        public DateTime? GetTimeAtIndex(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return _buffer[index].Time;
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

        public BufferUpdateResult Update(QuoteInfo quote)
        {
            _buffer.Append(new QuoteEntity(quote));
            return new BufferUpdateResult() { IsLastUpdated = true };
        }

        private void AppendData(List<QuoteInfo> data)
        {
            IsLoaded = true;
            _buffer.AppendRange(data.Select(q => new QuoteEntity(q)));
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
