using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage
{
    public abstract class SymbolData : ISymbolData, ISymbolKey
    {
        private IVarSet<FeedCacheKey> _keys;
        private FeedCache.Handler _storage;

        public SymbolData(string name, FeedCache.Handler storage)
        {
            _storage = storage;
            Name = name;
            _keys = storage.Keys.Where(k => k.Symbol == Name);
            SeriesCollection = _keys.Transform(k => new SymbolStorageSeries(k, this, storage));
        }

        public string Name { get; }
        public abstract string Description { get; }
        public abstract string Key { get; }
        public abstract string Security { get; }
        public abstract bool IsCustom { get; }
        public abstract ISymbolInfo InfoEntity { get; }
        public abstract CustomSymbol StorageEntity { get; }
        public abstract bool IsDataAvailable { get; }

        public abstract SymbolConfig.Types.SymbolOrigin Origin { get; }

        public IVarSet<SymbolStorageSeries> SeriesCollection { get; }

        public ISymbolKey StorageKey => this;

        string ISymbolKey.Id => Name;

        public abstract Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null);

        public abstract Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken,
            Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to);

        public abstract Task Remove();

        public abstract SymbolToken ToSymbolToken();

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

        public BlockingChannel<Slice<DateTime, BarData>> ReadCachedBars(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            var seriesKey = new FeedCacheKey(Name, timeFrame, priceType);
            return _storage.IterateBarCache(seriesKey, from, to);
        }

        [Conditional("DEBUG")]
        public void PrintCacheData(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType)
        {
            var seriesKey = new FeedCacheKey(Name, timeFrame, priceType);
            _storage.PrintSlices(seriesKey);
        }

        public virtual void OnRemoved()
        {
            _keys.Dispose();
            SeriesCollection.Dispose();
        }

        bool IEqualityComparer<ISymbolKey>.Equals(ISymbolKey x, ISymbolKey y)
        {
            return x.Name == y.Name && x.Origin == x.Origin;
        }

        int IEqualityComparer<ISymbolKey>.GetHashCode(ISymbolKey obj)
        {
            return HashCode.GetComposite(Name, Origin);
        }
    }
}