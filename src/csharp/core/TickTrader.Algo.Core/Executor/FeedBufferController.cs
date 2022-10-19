using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal interface IFeedControllerContext
    {
        IFeedHistoryProvider FeedHistory { get; }
    }


    public class FeedBufferController : IFeedControllerContext
    {
        private readonly Dictionary<BufferKey, ILoadableFeedBuffer> _buffers = new Dictionary<BufferKey, ILoadableFeedBuffer>();
        private readonly Channel<object> _feedQueue = DefaultChannelFactory.CreateForManyToOne<object>();
        private readonly IBarSub _barSub;
        private readonly IQuoteSub _quoteSub;
        private readonly IDisposable _barHandler, _quoteHandler;
        private readonly IFeedProvider _feedProvider;
        private readonly IFeedHistoryProvider _feedHistoryProvider;


        public FeedBufferController(IFeedProvider feedProvider, IFeedHistoryProvider feedHistoryProvider)
        {
            _feedProvider = feedProvider;
            _feedHistoryProvider = feedHistoryProvider;

            _barSub = feedProvider.GetBarSub();
            _quoteSub = feedProvider.GetQuoteSub();

            _barHandler = _barSub.AddHandler(b => _feedQueue.Writer.TryWrite(b));
            _quoteHandler = _quoteSub.AddHandler(q => _feedQueue.Writer.TryWrite(q));
        }


        public void Dispose()
        {
            _barHandler.Dispose();
            _quoteHandler.Dispose();
            _barSub.Dispose();
            _quoteSub.Dispose();
        }


        public async Task Load(int count)
        {
            InitSubs();

            var mainBuffer = GetMainBuffer();

            var to = DateTime.UtcNow.AddHours(1).ToUtcTicks();
            await mainBuffer.LoadFeed(to, -count);

            var from = mainBuffer.Timeline[0];

            await LoadBuffersInternal(from, to);
        }

        public async Task Load(UtcTicks from)
        {
            InitSubs();

            var to = DateTime.UtcNow.AddHours(1).ToUtcTicks();
            await LoadBuffersInternal(from, to);
        }

        public IFeedBuffer<BarData> GetBarBuffer(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side, bool markMain = false)
        {
            var buffer = GetBuffer(BufferKey.ForBar(symbol, timeframe, side));
            if (markMain)
                buffer.IsMain = true;
            return (IFeedBuffer<BarData>)buffer;
        }

        public IFeedBuffer<QuoteInfo> GetQuoteBuffer(string symbol, bool level2, bool markMain = false)
        {
            var buffer = GetBuffer(BufferKey.ForQuote(symbol, level2));
            if (markMain)
                buffer.IsMain = true;
            return (IFeedBuffer<QuoteInfo>)buffer;
        }

        public void ApplyUpdates()
        {
            var reader = _feedQueue.Reader;
            while (reader.TryRead(out var update))
            {
                if (update is BarInfo bar)
                    ApplyBarUpdate(bar);
                else if (update is QuoteInfo quote)
                    ApplyQuoteUpdate(quote);
            }
        }


        private ILoadableFeedBuffer GetMainBuffer()
        {
            var mainBuffers = _buffers.Values.Where(b => b.IsMain);
            var cnt = mainBuffers.Count();
            if (cnt == 0)
                throw new AlgoException("No main buffer specified");
            if (cnt > 1)
                throw new AlgoException($"Found {cnt} main buffers. Only 1 allowed");

            return mainBuffers.First();
        }

        private Task LoadBuffersInternal(UtcTicks from, UtcTicks to) => Task.WhenAll(_buffers.Values.Where(b => b.IsLoaded).Select(b => b.LoadFeed(from, to)));

        private void InitSubs()
        {
            var barSubUpdates = _buffers.Keys.Where(k => (int)k.Side != -1)
                .Select(k => BarSubUpdate.Upsert(new BarSubEntry(k.Symbol, k.Timeframe)))
                .ToList();
            if (barSubUpdates.Count > 0)
                _barSub.Modify(barSubUpdates);

            var quoteSubUpdates = _buffers.Keys.Where(k => (int)k.Side == -1)
                .Select(k => QuoteSubUpdate.Upsert(k.Symbol, 1))
                .ToList();
            if (quoteSubUpdates.Count > 0)
                _quoteSub.Modify(quoteSubUpdates);
        }

        private ILoadableFeedBuffer GetBuffer(BufferKey key)
        {
            if (!_buffers.TryGetValue(key, out var buffer))
            {
                buffer = (int)key.Side == -1
                    ? new BarSeriesBuffer(key.Symbol, key.Timeframe, key.Side, this)
                    : (ILoadableFeedBuffer)new QuoteSeriesBuffer(key.Symbol, key.Level2, this);

                _buffers[key] = buffer;
            }

            return buffer;
        }

        private void ApplyBarUpdate(BarInfo bar)
        {
            var key = BufferKey.ForBar(bar.Symbol, bar.Timeframe, bar.MarketSide);
            if (_buffers.TryGetValue(key, out var buffer) && buffer is IWritableFeedBuffer<BarData> barBuffer)
                barBuffer.ApplyUpdate(bar.Data);
        }

        private void ApplyQuoteUpdate(QuoteInfo quote)
        {
            var key = BufferKey.ForQuote(quote.Symbol, false);
            if (_buffers.TryGetValue(key, out var buffer1) && buffer1 is IWritableFeedBuffer<QuoteInfo> quoteBuffer1)
                quoteBuffer1.ApplyUpdate(quote);

            key = BufferKey.ForQuote(quote.Symbol, true);
            if (_buffers.TryGetValue(key, out var buffer2) && buffer2 is IWritableFeedBuffer<QuoteInfo> quoteBuffer2)
                quoteBuffer2.ApplyUpdate(quote);
        }



        IFeedHistoryProvider IFeedControllerContext.FeedHistory => _feedHistoryProvider;


        private readonly struct BufferKey
        {
            public string Symbol { get; }

            public Feed.Types.Timeframe Timeframe { get; }

            public Feed.Types.MarketSide Side { get; }

            public bool Level2 { get; }


            public BufferKey(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side, bool level2)
            {
                Symbol = symbol;
                Timeframe = timeframe;
                Side = side;
                Level2 = level2;
            }


            public static BufferKey ForBar(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side) => new BufferKey(symbol, timeframe, side, false);

            public static BufferKey ForQuote(string symbol, bool level2) => new BufferKey(symbol, Feed.Types.Timeframe.Ticks, (Feed.Types.MarketSide)(-1), level2);
        }
    }
}
