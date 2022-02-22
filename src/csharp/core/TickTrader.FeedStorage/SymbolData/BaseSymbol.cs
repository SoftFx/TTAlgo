using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    public abstract class BaseSymbol : ISymbolData, ISymbolKey
    {
        private readonly VarDictionary<ISeriesKey, SymbolStorageSeries> _serieses;
        private protected readonly FeedStorageBase.FeedHandler _storage;


        public ISymbolKey Key => this;

        public string Name => Info.Name;

        public bool IsCustom => Origin == SymbolConfig.Types.SymbolOrigin.Custom;


        public virtual bool IsDownloadAvailable => true;


        public ISymbolInfo Info { get; }

        public abstract SymbolConfig.Types.SymbolOrigin Origin { get; }


        public List<IStorageSeries> SeriesCollection => _serieses.Snapshot.Values.Cast<IStorageSeries>().ToList();


        public event Action<IStorageSeries> SeriesAdded;

        public event Action<IStorageSeries> SeriesRemoved;


        public BaseSymbol(ISymbolInfo info, FeedStorageBase.FeedHandler storage)
        {
            _serieses = new VarDictionary<ISeriesKey, SymbolStorageSeries>();
            _storage = storage;

            Info = info;
        }


        public abstract Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        public abstract Task<ActorChannel<ISliceInfo>> DownloadBarSeriesToStorage(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide marketSide, DateTime from, DateTime to);

        public abstract Task<ActorChannel<ISliceInfo>> DownloadTickSeriesToStorage(Feed.Types.Timeframe timeframe, DateTime from, DateTime to);


        internal void AddSeries()
        {

        }

        internal void RemoveSeries()
        {

        }


        public BarCrossDomainReader GetCrossDomainBarReader(Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            return _storage.CreateBarCrossDomainReader(new CrossDomainReaderRequest(new FeedCacheKey(Name, frame, priceType), from, to));
        }

        public TickCrossDomainReader GetCrossDomainTickReader(Feed.Types.Timeframe timeFrame, DateTime from, DateTime to)
        {
            return _storage.CreateTickCrossDomainReader(new CrossDomainReaderRequest(new FeedCacheKey(Name, timeFrame), from, to));
        }

        public void WriteSlice(Feed.Types.Timeframe frame, Feed.Types.MarketSide priceType, Timestamp from, Timestamp to, BarData[] values)
        {
            _storage.Put(Name, frame, priceType, from.ToDateTime(), to.ToDateTime(), values).Wait();
        }

        public void WriteSlice(Feed.Types.Timeframe timeFrame, Timestamp from, Timestamp to, QuoteInfo[] values)
        {
            _storage.Put(Name, timeFrame, from.ToDateTime(), to.ToDateTime(), values).Wait();
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

        SymbolConfig.Types.SymbolOrigin ISymbolKey.Origin => Origin;

        bool IEqualityComparer<ISymbolKey>.Equals(ISymbolKey x, ISymbolKey y)
        {
            return x.Name == y.Name && x.Origin == x.Origin;
        }

        int IEqualityComparer<ISymbolKey>.GetHashCode(ISymbolKey obj)
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
    }
}