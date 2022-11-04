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

        private class BarCacheController
        {
            private readonly ActorLock _asynclock;
            private readonly CircularItemCache<BarData> _askBars, _bidBars;
            private readonly CircularList<BarData> _futureAsks, _futureBids;

            private bool _freezeCache;


            public BufferKey Key { get; }


            public BarCacheController(BufferKey key, ActorLock asyncLock, int cacheSize)
            {
                Key = key;
                _asynclock = asyncLock;

                _askBars = new CircularItemCache<BarData>(cacheSize);
                _bidBars = new CircularItemCache<BarData>(cacheSize);

                var futureCacheSize = key.Timeframe == Feed.Types.Timeframe.S1 ? 128 : 16;
                _futureAsks = new CircularList<BarData>(futureCacheSize);
                _futureBids = new CircularList<BarData>(futureCacheSize);
            }


            public async Task<BarData[]> GetBars(SyncFeedProviderModel.BarListRequest req, FeedHistoryProviderModel.Handler history)
            {
                var cache = req.MarketSide == Feed.Types.MarketSide.Ask ? _askBars : _bidBars;

                if (req.Count.HasValue)
                {
                    var cnt = req.Count.Value;
                    var isBackward = cnt < 0;
                    cnt = Math.Abs(cnt);

                    var fromIndex = cache.BinarySearchBy(d => d.OpenTime, req.From, BinarySearchTypes.NearestLower);
                    var cacheFrom = cache[fromIndex].OpenTime;
                    if (!isBackward && cacheFrom <= req.From)
                    {
                        // Copy cache[fromIndex..Count)
                    }
                }

                using (await _asynclock.GetLock())
                {
                    _freezeCache = true;
                    try
                    {
                        return null;
                    }
                    finally
                    {
                        _freezeCache = false;
                        ApplyPendingUpdates();
                    }
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
        }
    }
}
