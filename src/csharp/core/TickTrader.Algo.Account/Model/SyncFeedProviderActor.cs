using System;
using System.Buffers;
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
        public static Task Reset(IActorRef actor) => actor.Ask(ResetCmd.Instance);

        public static void NotifyBarSubChanged(IActorRef actor, List<BarSubUpdate> updates) => actor.Tell(new BarSubChangedMsg(updates));

        public static Task<BarData[]> GetBarList(IActorRef actor, string symbol, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, UtcTicks from, UtcTicks to, int? count)
            => actor.Ask<BarData[]>(new BarListRequest(symbol, timeframe, marketSide, from, to, count));

        public static Task<QuoteInfo[]> GetQuoteList(IActorRef actor, string symbol, bool level2, UtcTicks from, UtcTicks to, int? count)
            => actor.Ask<QuoteInfo[]>(new QuoteListRequest(symbol, level2, from, to, count));


        internal record BarListRequest(string Symbol, Feed.Types.Timeframe Timeframe, Feed.Types.MarketSide MarketSide, UtcTicks From, UtcTicks To, int? Count);

        internal record QuoteListRequest(string Symbol, bool Level2, UtcTicks From, UtcTicks To, int? Count);

        internal class ResetCmd : Singleton<ResetCmd> { }

        internal record BarSubChangedMsg(List<BarSubUpdate> Updates);
    }


    internal class SyncFeedProviderActor : Actor
    {
        public const int MaxBarsInCache = 4096;

        private readonly Dictionary<BufferKey, BarCacheController> _barCaches = new();

        private readonly FeedHistoryProviderModel.Handler _history;
        private readonly IBarSub _barSub;
        private readonly IDisposable _barSubHandler;

        private IAlgoLogger _logger;


        private SyncFeedProviderActor(FeedHistoryProviderModel.Handler history, IBarSub barSub)
        {
            _history = history;
            _barSub = barSub;

            _barSubHandler = _barSub.AddHandler(update => Self.Tell(update));

            Receive<SyncFeedProviderModel.ResetCmd>(Reset);
            Receive<SyncFeedProviderModel.BarSubChangedMsg>(BarSubChanged);
            Receive<BarUpdate>(ApplyBarUpdate);
            Receive<SyncFeedProviderModel.BarListRequest, BarData[]>(GetBars);
            Receive<SyncFeedProviderModel.QuoteListRequest, QuoteInfo[]>(GetQuotes);
        }


        public static IActorRef Create(FeedHistoryProviderModel.Handler history, IBarSub barSub, string loggerId)
        {
            return ActorSystem.SpawnLocal(() => new SyncFeedProviderActor(history, barSub), $"{nameof(SyncFeedProviderActor)} {loggerId}");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private void Reset(SyncFeedProviderModel.ResetCmd cmd)
        {
            // Reset called on disconnect. Sub handler should release only on complete shutdown
            //_barSubHandler.Dispose();

            foreach (var barCache in _barCaches.Values)
                barCache.Dispose();

            _barCaches.Clear();
        }

        private void BarSubChanged(SyncFeedProviderModel.BarSubChangedMsg msg)
        {
            foreach (var update in msg.Updates)
            {
                var key = new BufferKey(update.Entry.Symbol, update.Entry.Timeframe);
                var hasCache = _barCaches.TryGetValue(key, out var barCache);
                if (update.IsUpsertAction && !hasCache)
                {
                    barCache = new BarCacheController(key, CreateLock(), MaxBarsInCache);
                    _barCaches[key] = barCache;

                    _logger.Debug($"BarSubChanged: Enabled bar cache for {key.Symbol}.{key.Timeframe}");
                }
                else if (update.IsRemoveAction)
                {
                    _barCaches.Remove(key);
                    barCache.Dispose();

                    _logger.Debug($"BarSubchanged: Disabled bar cache for {key.Symbol}.{key.Timeframe}");
                }
            }
        }

        private void ApplyBarUpdate(BarUpdate update)
        {
            var key = new BufferKey(update.Symbol, update.Timeframe);

            if (_barCaches.TryGetValue(key, out var barCache))
                barCache.ApplyUpdate(update);
        }

        private async Task<BarData[]> GetBars(SyncFeedProviderModel.BarListRequest req)
        {
            if (req.Count == 0 || (!req.Count.HasValue && req.From > req.To))
                return Array.Empty<BarData>();

            var key = new BufferKey(req.Symbol, req.Timeframe);

            if (!_barCaches.TryGetValue(key, out var barCache))
            {
                _logger.Debug($"Bar cache disabled for {key.Symbol}.{key.Timeframe}; Using direct feed history query...");

                // No cache if there is no subscription to updates 
                return req.Count.HasValue
                    ? await _history.GetBarPage(req.Symbol, req.MarketSide, req.Timeframe, req.From, req.Count.Value)
                    : (await _history.GetBarList(req.Symbol, req.MarketSide, req.Timeframe, req.From, req.To)).ToArray();
            }
            else
            {
                _logger.Debug($"Bar cache enabled for {key.Symbol}.{key.Timeframe}; Searching cache first...");

                return await barCache.GetBars(req, _history);
            }
        }

        private async Task<QuoteInfo[]> GetQuotes(SyncFeedProviderModel.QuoteListRequest req)
        {
            if (req.Count == 0 || (!req.Count.HasValue && req.From > req.To))
                return Array.Empty<QuoteInfo>();

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
            private bool _disposed;


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


            public void Dispose()
            {
                _disposed = true;

                _askBars.Clear();
                _bidBars.Clear();
                _futureAsks.Clear();
                _futureBids.Clear();
            }


            public async Task<BarData[]> GetBars(SyncFeedProviderModel.BarListRequest req, FeedHistoryProviderModel.Handler history)
            {
                var cache = req.MarketSide == Feed.Types.MarketSide.Ask ? _askBars : _bidBars;

                var searchRes = SearchCache(cache, req.From, req.To, req.Count);
                if (!searchRes.NeedServerRequest) // all data already in cache
                    return CreateCacheResult(cache, searchRes);

                using (await _requestlock.GetLock())
                {
                    if (_disposed)
                        return Array.Empty<BarData>();

                    // Now we have no pending requests
                    // Cache state might be modified since last check. Search again
                    searchRes = SearchCache(cache, req.From, req.To, req.Count);
                    if (!searchRes.NeedServerRequest) // all data already in cache
                        return CreateCacheResult(cache, searchRes);

                    _freezeCache = true;
                    BarData[] barsRes = null;
                    try
                    {
                        var requestRes = searchRes.ReqCount.HasValue
                            ? await history.GetBarPage(req.Symbol, req.MarketSide, req.Timeframe, searchRes.ReqFrom, searchRes.ReqCount.Value)
                            : (await history.GetBarList(req.Symbol, req.MarketSide, req.Timeframe, req.From, req.To)).ToArray();

                        var futureCache = req.MarketSide == Feed.Types.MarketSide.Ask ? _futureAsks : _futureBids;
                        barsRes = searchRes.CheckFutureUpdates
                            ? MergeBarsResult(requestRes, cache, futureCache, req.From, req.To, req.Count)
                            : MergeBarsResult(requestRes, cache, searchRes);

                        MergeCacheWithRequest(requestRes, cache);
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
                _futureAsks.Clear();

                foreach (var upd in _futureBids)
                    ApplyUpdate(_bidBars, upd);
                _futureBids.Clear();
            }

            private void ApplyPendingLastUpdates()
            {
                if (_askBars.Count > 0 && _futureAsks.Count > 0)
                {
                    var update = _futureAsks[0];
                    var lastIndex = _askBars.Count - 1;
                    if (_askBars[lastIndex].OpenTime == update.OpenTime)
                        _askBars[lastIndex] = update;

                    _futureAsks.Dequeue();
                }
                if (_bidBars.Count > 0 && _futureBids.Count > 0)
                {
                    var update = _futureBids[0];
                    var lastIndex = _bidBars.Count - 1;
                    if (_bidBars[lastIndex].OpenTime == update.OpenTime)
                        _bidBars[lastIndex] = update;

                    _futureBids.Dequeue();
                }
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

                    return searchRes;
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
                var fromIndex = cache.BinarySearchBy(d => d.OpenTime, reqFrom, BinarySearchTypes.NearestHigher);
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

            private void MergeCacheWithRequest(BarData[] requestRes, CircularItemCache<BarData> cache)
            {
                var cacheSize = cache.Count;
                if (cacheSize >= MaxBarsInCache)
                    return; // cache full

                if (cacheSize != 0 && requestRes.Length != 0 && cache[0].OpenTime <= requestRes[^1].OpenTime)
                    return; // requestRes end is expected to be before cache begin

                if (cacheSize == 0)
                {
                    cache.AddRange(requestRes.AsSpan());
                }
                else
                {
                    var buffer = ArrayPool<BarData>.Shared.Rent(cacheSize);
                    try
                    {
                        cache.CopyTo(buffer, 0);
                        cache.Clear();
                        cache.AddRange(requestRes.AsSpan());
                        cache.AddRange(buffer.AsSpan(0, cacheSize));
                    }
                    finally
                    {
                        ArrayPool<BarData>.Shared.Return(buffer);
                    }
                }
            }

            private BarData[] MergeBarsResult(BarData[] requestRes, CircularItemCache<BarData> cache, CacheSearchResult searchRes)
            {
                var res = new BarData[requestRes.Length + searchRes.Cnt];
                requestRes.CopyTo(res, 0);
                CopyCache(cache, searchRes.Offset, searchRes.Cnt, res, requestRes.Length);
                return res;
            }

            private BarData[] MergeBarsResult(BarData[] requestRes, CircularItemCache<BarData> cache, CircularList<BarData> futureCache, UtcTicks reqFrom, UtcTicks reqTo, int? reqCount)
            {
                // The fact that we made a request and need future updates mean that full cache range is involved
                // So we need to check what part of future updates are needed

                // update last bars to simplify merge calculations
                // this doesn't change cache range so search result should be still relevant
                ApplyPendingLastUpdates();

                BarData[] res = Array.Empty<BarData>();
                if (reqCount < 0)
                {
                    var reqCnt = Math.Abs(reqCount.Value);

                    var futureIndex = futureCache.BinarySearchBy(d => d.OpenTime, reqFrom, BinarySearchTypes.NearestHigher);
                    if (futureIndex != -1 && futureCache[futureIndex].OpenTime > reqFrom)
                        futureIndex--;

                    var futureCnt = futureIndex + 1;
                    var availableSize = requestRes.Length + cache.Count + futureCnt;
                    // future updates may have shifted the window we requested initially
                    var offset = availableSize > reqCnt ? availableSize - reqCnt : 0;
                    res = new BarData[offset > 0 ? reqCnt : availableSize];
                    var i = 0;
                    if (offset < requestRes.Length)
                    {
                        requestRes.AsSpan(offset).CopyTo(res.AsSpan(i));
                        i += requestRes.Length - offset;
                        offset = 0;
                    }
                    else
                    {
                        offset -= requestRes.Length;
                    }

                    if (offset < cache.Count)
                    {
                        var copySize = cache.Count - offset;
                        CopyCache(cache, offset, copySize, res, i);
                        i += copySize;
                        offset = 0;
                    }
                    else
                    {
                        offset -= cache.Count;
                    }

                    if (offset < futureCnt)
                    {
                        for (var j = 0; j < futureCnt; j++)
                            res[i + j] = futureCache[j];
                    }
                }
                else
                {
                    var futureCnt = 0;
                    if (reqCount.HasValue)
                    {
                        var spaceLeft = reqCount.Value - requestRes.Length - cache.Count;
                        if (spaceLeft > 0)
                            futureCnt = Math.Min(spaceLeft, futureCnt);
                    }
                    else
                    {
                        var futureIndex = futureCache.BinarySearchBy(d => d.OpenTime, reqTo, BinarySearchTypes.NearestHigher);
                        if (futureIndex != -1 && futureCache[futureIndex].OpenTime > reqTo)
                            futureIndex--;

                        futureCnt = futureIndex + 1;
                    }

                    res = new BarData[requestRes.Length + cache.Count + futureCnt];
                    var i = 0;
                    requestRes.CopyTo(res, i); i += requestRes.Length;
                    cache.CopyTo(res, i); i += cache.Count;
                    if (i < res.Length)
                    {
                        for (var j = 0; j < futureCnt; j++)
                            res[i + j] = futureCache[j];
                    }
                }

                return res;
            }
        }
    }
}
