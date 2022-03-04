using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class QuoteSeriesFixture : FeedFixture, IFeedBuffer
    {
        private InputBuffer<QuoteInfo> _buffer;
        private QuoteInfo _lastQuote;

        public QuoteSeriesFixture(string symbolCode, IFixtureContext context) : base(symbolCode, context)
        {
            _buffer = context.Builder.GetBuffer<QuoteInfo>(SymbolCode);
        }

        public UtcTicks this[int index] => _buffer[index].Time;
        public int Count { get { return _buffer.Count; } }
        public bool IsLoaded { get; private set; }
        public int LastIndex => _buffer.Count - 1;
        public UtcTicks OpenTime => _buffer[0].Time;

        public event Action Appended { add { } remove { } }

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
            var res = UpdateBuffer(quote);
            return res == null ? new BufferUpdateResult()
                : res.Value
                    ? new BufferUpdateResult { IsLastUpdated = true }
                    : new BufferUpdateResult { IsLastUpdated = false, ExtendedBy = 1 };
        }

        private void AppendData(List<QuoteInfo> data)
        {
            IsLoaded = true;
            foreach(var q in data)
            {
                UpdateBuffer(q);
            }
        }

        private bool? UpdateBuffer(QuoteInfo quote)
        {
            // ticks from past are not accepted
            if (_lastQuote?.Time > quote.Time)
                return true;

            // chart can't process output points with equal ticks
            var res = _lastQuote?.Time == quote.Time;
            _lastQuote = quote;
            if (res)
            {
                _buffer[LastIndex] = quote;
            }
            else
            {
                _buffer.Append(quote);
            }
            return res;
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
