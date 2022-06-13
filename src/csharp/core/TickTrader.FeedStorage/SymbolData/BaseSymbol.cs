using ActorSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.FeedStorage.StorageBase;

namespace TickTrader.FeedStorage
{
    internal abstract class BaseSymbol : ISymbolData, ISymbolKey
    {
        private readonly ConcurrentDictionary<ISeriesKey, IStorageSeries> _series;
        private protected readonly FeedStorageBase.FeedHandler _storage;


        public ISymbolKey Key => this;

        public string Name => Info.Name;

        public bool IsCustom => Origin == SymbolConfig.Types.SymbolOrigin.Custom;


        public virtual bool IsDownloadAvailable => true;


        public ISymbolInfo Info { get; private set; }

        public abstract SymbolConfig.Types.SymbolOrigin Origin { get; }


        public IReadOnlyDictionary<ISeriesKey, IStorageSeries> Series => _series;


        public event Action<IStorageSeries> SeriesAdded, SeriesRemoved, SeriesUpdated;


        public BaseSymbol(ISymbolInfo info, FeedStorageBase.FeedHandler storage)
        {
            _series = new ConcurrentDictionary<ISeriesKey, IStorageSeries>();
            _storage = storage;

            UpdateInfo(info);
        }


        internal void UpdateInfo(ISymbolInfo info)
        {
            Info = info;
        }

        public abstract Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        public abstract Task<ActorChannel<ISliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to);

        public abstract Task<ActorChannel<ISliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to);


        public Task<ActorChannel<ISliceInfo>> ImportBarSeriesToStorage(IImportSeriesSettings settings, Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide)
        {
            return _storage?.ImportSeriesWithFile(GetKey(timeframe, marketSide), settings);
        }

        public Task<ActorChannel<ISliceInfo>> ImportTickSeriesToStorage(IImportSeriesSettings settings, Feed.Types.Timeframe timeframe)
        {
            return _storage?.ImportSeriesWithFile(GetKey(timeframe), settings);
        }


        public async Task<IEnumerable<BarData>> GetBarStream(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side, DateTime from, DateTime to, bool reversed = false)
        {
            var feedKey = GetKey(timeframe, side);
            var series = await _storage.GetSeries<BarData>(feedKey);

            return series?.Iterate(from, to, reversed);
        }

        public async Task<IEnumerable<QuoteInfo>> GetTickStream(Feed.Types.Timeframe timeframe, DateTime from, DateTime to, bool reversed = false)
        {
            var feedKey = GetKey(timeframe);
            var series = await _storage.GetSeries<QuoteInfo>(feedKey);

            return series?.Iterate(from, to, reversed);
        }


        internal void AddSeries(FeedCacheKey key)
        {
            if (_series.ContainsKey(key))
                return;

            var newSeries = new SymbolStorageSeries(key, _storage);

            _series.TryAdd(key, newSeries);
            SeriesAdded?.Invoke(newSeries);
        }

        internal void RemoveSeries(FeedCacheKey key)
        {
            if (_series.TryRemove(key, out var series))
                SeriesRemoved?.Invoke(series);
        }

        internal void UpdateSeries(FeedCacheKey key, FeedSeriesUpdate update)
        {
            if (!_series.TryGetValue(key, out var series))
                return;

            ((SymbolStorageSeries)series).Update(update);
            SeriesUpdated?.Invoke(series);
        }

        protected FeedCacheKey GetKey(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide? side = null)
        {
            return new FeedCacheKey(Name, timeframe, Origin, side);
        }


        string ISymbolKey.Name => Name; // protection for correct object serialization

        public override int GetHashCode() => ((ISymbolKey)this).GetHashCode();
    }
}