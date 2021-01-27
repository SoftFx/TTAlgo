using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class FeedCdProxy : CrossDomainObject, IFeedProvider, IFeedHistoryProvider
    {
        private BunchingBlock<QuoteInfo> _bBlock;
        private readonly IFeedProvider _feed;
        private readonly IFeedHistoryProvider _history;

        public FeedCdProxy(IFeedProvider feed, IFeedHistoryProvider history)
        {
            _feed = feed;
            _history = history;
        }

        public void Stop()
        {
            _feed.RateUpdated -= _feed_RateUpdated;
            _bBlock.Complete();
            _bBlock.Completion.Wait();
        }

        public ISyncContext Sync => _feed.Sync;

        public event Action<QuoteInfo> RateUpdated { add { } remove { } }
        public event Action<List<QuoteInfo>> RatesUpdated;

        public List<QuoteInfo> GetSnapshot()
        {
            return _feed.GetSnapshot();
        }

        public Task<List<QuoteInfo>> GetSnapshotAsync()
        {
            return _feed.GetSnapshotAsync();
        }

        //public QuoteEntity GetRate(string symbol)
        //{
        //    return _feed.GetRate(symbol);
        //}

        public List<BarData> QueryBars(string symbolCode, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return _history.QueryBars(symbolCode, marketSide, timeframe, from, to);
        }

        public List<BarData> QueryBars(string symbolCode, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            return _history.QueryBars(symbolCode, marketSide, timeframe, from, count);
        }

        public List<QuoteInfo> QueryQuotes(string symbolCode, Timestamp from, Timestamp to, bool level2)
        {
            return _history.QueryQuotes(symbolCode, from, to, level2);
        }

        public List<QuoteInfo> QueryQuotes(string symbolCode, Timestamp from, int count, bool level2)
        {
            return _history.QueryQuotes(symbolCode, from, count, level2);
        }

        public Task<List<BarData>> QueryBarsAsync(string symbolCode, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, Timestamp to)
        {
            return _history.QueryBarsAsync(symbolCode, marketSide, timeframe, from, to);
        }

        public Task<List<BarData>> QueryBarsAsync(string symbolCode, Feed.Types.MarketSide marketSide, Feed.Types.Timeframe timeframe, Timestamp from, int count)
        {
            return _history.QueryBarsAsync(symbolCode, marketSide, timeframe, from, count);
        }

        public Task<List<QuoteInfo>> QueryQuotesAsync(string symbolCode, Timestamp from, Timestamp to, bool level2)
        {
            return _history.QueryQuotesAsync(symbolCode, from, to, level2);
        }

        public Task<List<QuoteInfo>> QueryQuotesAsync(string symbolCode, Timestamp from, int count, bool level2)
        {
            return _history.QueryQuotesAsync(symbolCode, from, count, level2);
        }

        public List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates)
        {
            LazyInit();

            return _feed.Modify(updates);
        }

        public Task<List<QuoteInfo>> ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            LazyInit();

            return _feed.ModifyAsync(updates);
        }

        public void CancelAll()
        {
            _feed.CancelAll();
        }

        public Task CancelAllAsync()
        {
            return _feed.CancelAllAsync();
        }

        private void LazyInit()
        {
            if (_bBlock == null)
            {
                _bBlock = new BunchingBlock<QuoteInfo>(TransferToCore, 30);
                _feed.RateUpdated += _feed_RateUpdated;
                _feed.RatesUpdated += _feed_RatesUpdated;
            }
        }

        private void TransferToCore(List<QuoteInfo> page)
        {
            RatesUpdated?.Invoke(page);
        }

        private void _feed_RatesUpdated(List<QuoteInfo> quotes)
        {
            foreach (var q in quotes)
                _feed_RateUpdated(q);
        }

        private void _feed_RateUpdated(QuoteInfo quote)
        {
            _bBlock?.Enqueue(quote);
        }
    }
}
