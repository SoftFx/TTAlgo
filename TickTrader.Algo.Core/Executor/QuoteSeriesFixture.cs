using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class QuoteSeriesFixture : FeedFixture, IFeedBuffer
    {
        private InputBuffer<QuoteInfo> _buffer;

        public QuoteSeriesFixture(string symbolCode, IFixtureContext context) : base(symbolCode, context)
        {
            _buffer = context.Builder.GetBuffer<QuoteInfo>(SymbolCode);
        }

        public Timestamp this[int index] => _buffer[index].Timestamp;
        public int Count { get { return _buffer.Count; } }
        public bool IsLoaded { get; private set; }
        public int LastIndex => _buffer.Count - 1;
        public Timestamp OpenTime => _buffer[0].Timestamp;

        public event Action Appended { add { } remove { } }

        public Timestamp GetTimeAtIndex(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return _buffer[index].Timestamp;
        }

        public void LoadFeed(Timestamp from, Timestamp to)
        {
            var data = Context.FeedHistory.QueryQuotes(SymbolCode, from, to, false);
            AppendData(data);
        }

        public void LoadFeed(int count)
        {
            var to = (DateTime.Now + TimeSpan.FromDays(1)).ToTimestamp();
            var data = Context.FeedHistory.QueryQuotes(SymbolCode, to, -count, false);
            AppendData(data);
        }

        public void LoadFeed(Timestamp from, int count)
        {
            var data = Context.FeedHistory.QueryQuotes(SymbolCode, from, count, false);
            AppendData(data);
        }

        public void LoadFeedFrom(Timestamp from)
        {
            throw new NotImplementedException();
        }

        public void SyncByTime()
        {
            throw new NotSupportedException("Synchronization by time is not supported for quote series!");
        }

        public BufferUpdateResult Update(QuoteInfo quote)
        {
            _buffer.Append(quote);
            return new BufferUpdateResult() { IsLastUpdated = true };
        }

        private void AppendData(List<QuoteInfo> data)
        {
            IsLoaded = true;
            _buffer.AppendRange(data);
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
