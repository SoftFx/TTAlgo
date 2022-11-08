using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public static class SyncFeedProviderModel
    {
        internal record BarListRequest(string Symbol, Feed.Types.Timeframe Timeframe, Feed.Types.MarketSide MarketSide, UtcTicks From, UtcTicks To, int? Count);

        internal record QuoteListRequest(string Symbol, bool Level2, UtcTicks From, UtcTicks To, int? Count);
    }


    internal class SyncFeedProviderActor : Actor
    {
        public const int MaxBarsInCache = 4096;

        private readonly Dictionary<BufferKey, BarCacheController> _barCaches;

        private readonly FeedHistoryProviderModel.Handler _history;
        private readonly IBarSub _barSub;
        private readonly IDisposable _barSubHandler;


        private SyncFeedProviderActor(FeedHistoryProviderModel.Handler history, IBarSub barSub)
        {
            _history = history;
            _barSub = barSub;

            _barSubHandler = _barSub.AddHandler(update => Self.Tell(update));

            Receive<BarUpdate>(ApplyBarUpdate);
            Receive<SyncFeedProviderModel.BarListRequest>(GetBars);
            Receive<SyncFeedProviderModel.QuoteListRequest>(GetQuotes);
        }


        public static IActorRef Create(FeedHistoryProviderModel.Handler history, IBarSub barSub)
        {
            return ActorSystem.SpawnLocal(() => new SyncFeedProviderActor(history, barSub), $"{nameof(SyncFeedProviderActor)}");
        }


        private void ApplyBarUpdate(BarUpdate update)
        {
            var key = new BufferKey(update.Symbol, update.Timeframe);

            if (!_barCaches.TryGetValue(key, out var barCache))
            {
                barCache = new BarCacheController(key, CreateLock(), MaxBarsInCache);
                _barCaches[key] = barCache;
            }

            barCache.ApplyUpdate(update);
        }

        private async Task<BarData[]> GetBars(SyncFeedProviderModel.BarListRequest req)
        {
            if (req.Count == 0 || req.From >= req.To)
                return new BarData[0];

            var key = new BufferKey(req.Symbol, req.Timeframe);

            if (!_barCaches.TryGetValue(key, out var barCache))
            {
                // No cache if there is no subscription to updates 
                return req.Count.HasValue
                    ? await _history.GetBarPage(req.Symbol, req.MarketSide, req.Timeframe, req.From, req.Count.Value)
                    : (await _history.GetBarList(req.Symbol, req.MarketSide, req.Timeframe, req.From, req.To)).ToArray();
            }
            else
            {
                return await barCache.GetBars(req, _history);
            }
        }

        private async Task<QuoteInfo[]> GetQuotes(SyncFeedProviderModel.QuoteListRequest req)
        {
            if (req.Count == 0 || req.From >= req.To)
                return new QuoteInfo[0];

            return req.Count.HasValue
                ? await _history.GetQuotePage(req.Symbol, req.From, req.Count.Value, req.Level2)
                : (await _history.GetQuoteList(req.Symbol, req.From, req.To, req.Level2)).ToArray();
        }


        private readonly struct BufferKey
        {
            public string Symbol { get; }

            public Feed.Types.Timeframe Timeframe { get; }


            public BufferKey(string symbol, Feed.Types.Timeframe timeframe)
            {
                Symbol = symbol;
                Timeframe = timeframe;
            }
        }

        private class CacheSearchResult
        {
            public int Offset { get; set; }

            public int Cnt { get; set; }

            public bool CheckFutureUpdates { get; set; }

            public bool NeedServerRequest { get; set; }

            public UtcTicks ReqFrom { get; set; }

            public UtcTicks ReqTo { get; set; }

            public int? ReqCount { get; set; }
        }

        private class BarCacheController
        {
            private readonly ActorLock _requestlock;
            private readonly CircularItemCache<BarData> _askBars, _bidBars;
            private readonly CircularList<BarData> _futureAsks, _futureBids;

            private bool _freezeCache;


            public BufferKey Key { get; }


            public BarCacheController(BufferKey key, ActorLock asyncLock, int cacheSize)
            {
                Key = key;
                _requestlock = asyncLock;

                _askBars = new CircularItemCache<BarData>(cacheSize);
                _bidBars = new CircularItemCache<BarData>(cacheSize);

                var futureCacheSize = key.Timeframe == Feed.Types.Timeframe.S1 ? 128 : 16;
                _futureAsks = new CircularList<BarData>(futureCacheSize);
                _futureBids = new CircularList<BarData>(futureCacheSize);
            }


            public async Task<BarData[]> GetBars(SyncFeedProviderModel.BarListRequest req, FeedHistoryProviderModel.Handler history)
            {
                var cache = req.MarketSide == Feed.Types.MarketSide.Ask ? _askBars : _bidBars;

                if (req.Count == 0 || (!req.Count.HasValue && req.From > req.To))
                    return Array.Empty<BarData>();

                var searchRes = SearchCache(cache, req.From, req.To, req.Count);
                if (!searchRes.NeedServerRequest) // all data already in cache
                    CreateCacheResult(cache, searchRes);

                using (await _requestlock.GetLock())
                {
                    // Now we have no pending requests
                    // Cache state might be modified since last check. Search again
                    searchRes = SearchCache(cache, req.From, req.To, req.Count);
                    if (!searchRes.NeedServerRequest) // all data already in cache
                        CreateCacheResult(cache, searchRes);

                    _freezeCache = true;
                    BarData[] barsRes = null;
                    try
                    {
                        var requestRes = searchRes.ReqCount.HasValue
                            ? await history.GetBarPage(req.Symbol, req.MarketSide, req.Timeframe, searchRes.ReqFrom, searchRes.ReqCount.Value)
                            : (await history.GetBarList(req.Symbol, req.MarketSide, req.Timeframe, req.From, req.To)).ToArray();

                        // merge requestRes and cache into barsRes

                        // merge cache with requestRes if cache not full
                    }
                    finally
                    {
                        _freezeCache = false;
                        ApplyPendingUpdates();
                    }

                    return barsRes;
                }
            }

            public void ApplyUpdate(BarUpdate update)
            {
                if (_freezeCache)
                {
                    ApplyUpdate(_futureAsks, update.AskData);
                    ApplyUpdate(_futureBids, update.BidData);
                }
                else
                {
                    ApplyUpdate(_askBars, update.AskData);
                    ApplyUpdate(_bidBars, update.BidData);
                }
            }


            private void ApplyUpdate(CircularList<BarData> buffer, BarData update)
            {
                var lastIndex = buffer.Count - 1;
                if (lastIndex == -1)
                {
                    buffer.Add(update);
                }
                else
                {
                    if (buffer[lastIndex].OpenTime == update.OpenTime)
                        buffer[lastIndex] = update;
                    else if (buffer[lastIndex].OpenTime < update.OpenTime)
                        buffer.Add(update);
                }
            }

            private void ApplyPendingUpdates()
            {
                foreach (var upd in _futureAsks)
                    ApplyUpdate(_askBars, upd);
                foreach (var upd in _futureBids)
                    ApplyUpdate(_bidBars, upd);
            }

            private CacheSearchResult SearchCache(CircularItemCache<BarData> cache, UtcTicks reqFrom, UtcTicks reqTo, int? reqCount)
            {
                var searchRes = new CacheSearchResult();

                if (cache.Count == 0)
                {
                    searchRes.Offset = 0;
                    searchRes.Cnt = 0;
                    searchRes.CheckFutureUpdates = true;
                    searchRes.NeedServerRequest = true;
                    searchRes.ReqFrom = reqFrom;
                    searchRes.ReqTo = reqTo;
                    searchRes.ReqCount = reqCount;
                }

                if (reqCount < 0)
                {
                    SearchCacheBackward(cache, reqFrom, -reqCount.Value, searchRes);
                }
                else
                {
                    SearchCacheForward(cache, reqFrom, reqTo, reqCount, searchRes);
                }

                return searchRes;
            }

            private void SearchCacheForward(CircularItemCache<BarData> cache, UtcTicks reqFrom, UtcTicks reqTo, int? reqCount, CacheSearchResult searchRes)
            {
                var fromIndex = cache.BinarySearchBy(d => d.OpenTime, reqFrom, BinarySearchTypes.NearestLower);
                var cacheFrom = cache[fromIndex].OpenTime;
                if (cacheFrom < reqFrom)
                    fromIndex++;

                searchRes.Offset = fromIndex;
                if (cacheFrom > reqFrom)
                {
                    // cache miss: extra date range from cache begin
                    searchRes.NeedServerRequest = true;
                    searchRes.ReqFrom = reqFrom;
                    searchRes.ReqTo = cache[0].OpenTime.AddMs(-1);
                }
                else
                {
                    // cache hit
                    searchRes.NeedServerRequest = false;
                }

                if (reqCount.HasValue)
                {
                    var reqCnt = reqCount.Value; // expected to be positive
                    var cacheSize = cache.Count - fromIndex;
                    searchRes.Cnt = Math.Min(cacheSize, reqCnt);
                    if (reqCnt >= cacheSize)
                    {
                        searchRes.Cnt = cacheSize;
                        searchRes.CheckFutureUpdates = true; // During server request can update last bar or add new
                    }
                    else
                    {
                        searchRes.Cnt = reqCnt;
                        searchRes.CheckFutureUpdates = false;
                    }
                }
                else
                {
                    var toIndex = cache.BinarySearchBy(d => d.OpenTime, reqTo, BinarySearchTypes.NearestHigher);
                    var cacheTo = cache[toIndex].OpenTime;
                    if (cacheTo > reqTo)
                        toIndex--;

                    searchRes.Cnt = toIndex - fromIndex + 1;
                    searchRes.CheckFutureUpdates = cacheTo <= reqTo; // During server request can update last bar or add new
                }
            }

            private void SearchCacheBackward(CircularItemCache<BarData> cache, UtcTicks reqFrom, int reqCount, CacheSearchResult searchRes)
            {
                var fromIndex = cache.BinarySearchBy(d => d.OpenTime, reqFrom, BinarySearchTypes.NearestLower);
                var cacheFrom = cache[fromIndex].OpenTime;

                if (cacheFrom > reqFrom)
                    fromIndex--;

                searchRes.CheckFutureUpdates = cacheFrom <= reqFrom; // future can update last bar or add new

                var cacheSize = fromIndex + 1;
                if (reqCount > cacheSize)
                {
                    // cache miss: extra cnt from cache begin
                    searchRes.Offset = 0;
                    searchRes.Cnt = cacheSize;
                    searchRes.NeedServerRequest = true;
                    searchRes.ReqFrom = cache[0].OpenTime.AddMs(-1);
                    searchRes.ReqCount = -(reqCount - cacheSize);
                }
                else
                {
                    // cache hit
                    searchRes.Offset = cacheSize - reqCount;
                    searchRes.Cnt = reqCount;
                    searchRes.NeedServerRequest = false;
                }
            }

            private void CopyCache(CircularItemCache<BarData> cache, int offset, int cnt, BarData[] dst, int dstOffset)
            {
                for (var i = 0; i < cnt; i++)
                {
                    dst[dstOffset + i] = cache[offset + i];
                }
            }

            private BarData[] CreateCacheResult(CircularItemCache<BarData> cache, CacheSearchResult searchRes)
            {
                var cnt = searchRes.Cnt;
                if (cnt == 0)
                    return Array.Empty<BarData>();

                var res = new BarData[cnt];
                CopyCache(cache, searchRes.Offset, cnt, res, 0);
                return res;
            }
        }
    }
}
