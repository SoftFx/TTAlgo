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

    public class OnlineSymbolData : SymbolData
    {
        private readonly SymbolInfo _symbolInfo;
        private readonly FeedProvider.Handler _provider;

        public OnlineSymbolData(SymbolInfo symbol, FeedProvider.Handler provider) : base(symbol.Name, provider.Cache)
        {
            _symbolInfo = symbol ?? throw new ArgumentNullException("symbol");
            _provider = provider;
        }

        public override bool IsCustom => false;
        public override string Key => "online->" + Name;
        public override string Description => _symbolInfo.Description;
        public override string Security => _symbolInfo.Security;
        public override SymbolInfo InfoEntity => _symbolInfo;
        public override CustomSymbol StorageEntity => CustomSymbol.FromAlgo(_symbolInfo);
        public override bool IsDataAvailable => _provider.FeedProxy.IsAvailable;

        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _provider.FeedProxy.GetAvailableSymbolRange(_symbolInfo.Name, timeFrame, priceType ?? Feed.Types.MarketSide.Bid);
        }

        public async override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            var symbol = _symbolInfo.Name;

            var watch = Stopwatch.StartNew();
            int downloadedCount = 0;

            if (!timeFrame.IsTicks())
            {
                observer?.SetMessage("Downloading " + symbol + " " + priceType);

                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var barEnumerator = await _provider.DownloadBarSeriesToStorage(symbol, timeFrame, priceType, from, to);

                while (await barEnumerator.ReadNext())
                {
                    var info = barEnumerator.Current;
                    if (info.Count > 0)
                    {
                        downloadedCount += info.Count;
                        if (showStats)
                        {
                            var msg = "Downloading... " + downloadedCount + " bars are downloaded.";
                            if (watch.ElapsedMilliseconds > 0)
                                msg += "\nBar per second: " + Math.Round(((double)downloadedCount * 1000) / watch.ElapsedMilliseconds);
                            observer.SetMessage(msg);
                        }
                    }

                    observer?.SetProgress(info.To.GetAbsoluteDay());

                    if (cancelToken.IsCancellationRequested)
                    {
                        await barEnumerator.Close();
                        observer.SetMessage("Canceled. " + downloadedCount + " bars were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed. " + downloadedCount + " bars were downloaded.");
            }
            else // ticks
            {
                //var endDay = to.GetAbsoluteDay();
                //var totalDays = endDay - from.GetAbsoluteDay();

                observer?.SetMessage("Downloading " + symbol);

                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var tickEnumerator = await _provider.DownloadTickSeriesToStorage(symbol, timeFrame, from, to);

                while (await tickEnumerator.ReadNext())
                {
                    var info = tickEnumerator.Current;
                    if (showStats && info.Count > 0)
                    {
                        downloadedCount += info.Count;
                        var msg = "Downloading... " + downloadedCount + " ticks are downloaded.";
                        if (watch.ElapsedMilliseconds > 0)
                            msg += "\nTicks per second: " + Math.Round(((double)downloadedCount * 1000) / watch.ElapsedMilliseconds);
                        observer.SetMessage(msg);
                    }

                    observer?.SetProgress(info.From.GetAbsoluteDay());

                    if (cancelToken.IsCancellationRequested)
                    {
                        await tickEnumerator.Close();
                        observer.SetMessage("Canceled! " + downloadedCount + " ticks were downloaded.");
                        return;
                    }
                }

                observer.SetMessage("Completed: " + downloadedCount + " ticks were downloaded.");
            }
        }

        public override Task Remove()
        {
            throw new InvalidOperationException("Cannot remove online symbol!");
        }

        public override SymbolToken ToSymbolToken()
        {
            return new SymbolToken(Name, SymbolConfig.Types.SymbolOrigin.Online);
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

    public class SymbolStorageSeries
    {
        private FeedCache.Handler _storage;

        public SymbolStorageSeries(FeedCacheKey key, SymbolData symbolModel, FeedCache.Handler storage)
        {
            Key = key;
            Symbol = symbolModel;
            _storage = storage;
        }

        public FeedCacheKey Key { get; }
        public SymbolData Symbol { get; }

        public Task<double?> GetCollectionSize()
        {
            return _storage.GetCollectionSize(Key);
        }

        public Task Remove()
        {
            return _storage.RemoveSeries(Key);
        }

        public ActorChannel<Slice<DateTime, BarData>> IterateBarCache(DateTime from, DateTime to)
        {
            return _storage.IterateBarCacheAsync(Key, from, to);
        }

        public ActorChannel<Slice<DateTime, QuoteInfo>> IterateTickCache(DateTime from, DateTime to)
        {
            return _storage.IterateTickCacheAsync(Key, from, to);
        }
    }
}
