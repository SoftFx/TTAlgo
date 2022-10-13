using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface IFeedBuffer
    {
        UtcTicks this[int index] { get; }

        int Count { get; }
    }

    internal interface ILoadableFeedBuffer : IFeedBuffer
    {
        bool IsLoaded { get; }


        Task LoadFeed(UtcTicks from, int count);

        Task LoadFeed(UtcTicks from, UtcTicks to);
    }

    internal interface IFeedLoadContext
    {
        IBarSub BarSub { get; }

        IQuoteSub QuoteSub { get; }

        IFeedHistoryProvider FeedHistory { get; }
    }


    public class FeedBufferController : IFeedLoadContext
    {
        private readonly Dictionary<BufferKey, ILoadableFeedBuffer> _buffers = new Dictionary<BufferKey, ILoadableFeedBuffer>();
        private readonly IBarSub _barSub;
        private readonly IQuoteSub _quoteSub;
        private readonly IDisposable _barHandler, _quoteHandler;
        private readonly IFeedProvider _feedProvider;
        private readonly IFeedHistoryProvider _feedHistoryProvider;
        private readonly Channel<object> _feedQueue;


        public FeedBufferController(IFeedProvider feedProvider, IFeedHistoryProvider feedHistoryProvider, string mainSymbol, Feed.Types.Timeframe mainTimeframe)
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


        public async Task Load(UtcTicks from, int count)
        {
            await Task.WhenAll(_buffers.Values.Where(b => b.IsLoaded).Select(b => b.LoadFeed(from, count)));
        }

        public async Task Load(UtcTicks from, UtcTicks to)
        {
            await Task.WhenAll(_buffers.Values.Where(b => b.IsLoaded).Select(b => b.LoadFeed(from, to)));
        }

        public IFeedBuffer GetBarBuffer(string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side)
        {
            return GetBuffer(BufferKey.ForBar(symbol, timeframe, side));
        }

        public IFeedBuffer GetQuoteBuffer(string symbol, bool level2)
        {
            return GetBuffer(BufferKey.ForQuote(symbol, level2));
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


        IBarSub IFeedLoadContext.BarSub => _barSub;

        IQuoteSub IFeedLoadContext.QuoteSub => _quoteSub;

        IFeedHistoryProvider IFeedLoadContext.FeedHistory => _feedHistoryProvider;


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
