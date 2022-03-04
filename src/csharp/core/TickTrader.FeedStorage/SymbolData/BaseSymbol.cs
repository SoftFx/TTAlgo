using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal abstract class BaseSymbol : ISymbolData, ISymbolKey
    {
        private readonly ConcurrentDictionary<FeedCacheKey, IStorageSeries> _serieses;
        private protected readonly FeedStorageBase.FeedHandler _storage;


        public ISymbolKey Key => this;

        public string Name => Info.Name;

        public bool IsCustom => Origin == SymbolConfig.Types.SymbolOrigin.Custom;


        public virtual bool IsDownloadAvailable => true;


        public ISymbolInfo Info { get; }

        public abstract SymbolConfig.Types.SymbolOrigin Origin { get; }


        public List<IStorageSeries> SeriesCollection => _serieses.Values.ToList();


        public event Action<IStorageSeries> SeriesAdded, SeriesRemoved, SeriesUpdated;


        public BaseSymbol(ISymbolInfo info, FeedStorageBase.FeedHandler storage)
        {
            _serieses = new ConcurrentDictionary<FeedCacheKey, IStorageSeries>();
            _storage = storage;

            Info = info;
        }


        public abstract Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        public abstract Task<ActorChannel<ISliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to);

        public abstract Task<ActorChannel<ISliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to);


        internal void AddSeries(FeedCacheKey key, double size = 0)
        {
            if (_serieses.ContainsKey(key))
                return;

            var newSeries = new SymbolStorageSeries(key, _storage, size);

            _serieses.TryAdd(key, newSeries);
            SeriesAdded?.Invoke(newSeries);
        }

        internal void RemoveSeries(FeedCacheKey key)
        {
            if (_serieses.TryRemove(key, out var series))
                SeriesRemoved?.Invoke(series);
        }

        internal void UpdateSeries(FeedCacheKey key, double newSize)
        {
            if (!_serieses.TryGetValue(key, out var series))
                return;

            ((SymbolStorageSeries)series).Size = newSize;
            SeriesUpdated?.Invoke(series);
        }


        public void WriteSlice(Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, Timestamp from, Timestamp to, BarData[] values)
        {
            _storage.Put(Name, frame, priceType, from.ToDateTime(), to.ToDateTime(), values).Wait();
        }

        public void WriteSlice(Feed.Types.Timeframe timeFrame, Timestamp from, Timestamp to, QuoteInfo[] values)
        {
            //_storage.Put(Name, timeFrame, from.ToDateTime(), to.ToDateTime(), values).Wait();
        }

        //public BlockingChannel<Slice<DateTime, BarData>> ReadCachedBars(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        //{
        //    var seriesKey = new FeedCacheKey(Name, timeFrame, priceType);
        //    return _storage.IterateBarCache(seriesKey, from, to);
        //}

        [Conditional("DEBUG")]
        public void PrintCacheData(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType)
        {
            var seriesKey = new FeedCacheKey(Name, timeFrame, priceType);
            _storage.PrintSlices(seriesKey);
        }

        //public virtual void OnRemoved()
        //{
        //    _keys.Dispose();
        //    //SeriesCollection.Dispose();
        //}


        string ISymbolKey.Name => Name; // protection for correct object serialization

        public override int GetHashCode()
        {
            return HashCode.GetComposite(Name, Origin);
        }

        int IComparable<ISymbolKey>.CompareTo(ISymbolKey other)
        {
            if (Origin == other.Origin)
                return Name.CompareTo(other.Name);
            else
                return Origin.CompareTo(other.Origin);
        }

        public bool Equals(ISymbolKey other)
        {
            return Origin == other.Origin && Name == other.Name;
        }
    }
}