using ActorSharp;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage.Api
{
    public abstract class SymbolData : ISymbolData
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
        public abstract SymbolInfo InfoEntity { get; }
        public abstract CustomSymbol StorageEntity { get; }
        public abstract bool IsDataAvailable { get; }

        public IVarSet<SymbolStorageSeries> SeriesCollection { get; }

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
    }

    public class CustomSymbolData : SymbolData
    {
        private CustomSymbol _symbolInfo;
        private CustomFeedStorage.Handler _storage;

        public CustomSymbolData(CustomSymbol symbol, CustomFeedStorage.Handler storage)
            : base(symbol.Name, storage)
        {
            _symbolInfo = symbol;
            _storage = storage;
        }

        public CustomSymbol Entity => _symbolInfo;
        public override string Key => "custom->";
        public override bool IsCustom => true;
        public override string Description => _symbolInfo.Description;
        public override string Security => "";
        public override SymbolInfo InfoEntity => _symbolInfo.ToAlgo();
        public override CustomSymbol StorageEntity => _symbolInfo;
        public override bool IsDataAvailable => true;

        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _storage.GetRange(new FeedCacheKey(_symbolInfo.Name, timeFrame, priceType));
        }

        public override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            return Task.CompletedTask;
        }

        public override Task Remove()
        {
            return _storage.Remove(_symbolInfo.Name);
        }

        public override SymbolToken ToSymbolToken()
        {
            return new SymbolToken(Name, SymbolConfig.Types.SymbolOrigin.Custom);
        }
    }
}
