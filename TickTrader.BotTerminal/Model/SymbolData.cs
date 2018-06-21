using ActorSharp;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.BotTerminal.Lib;
using TickTrader.SeriesStorage;
using ISymbolInfo = TickTrader.BusinessObjects.ISymbolInfo;

namespace TickTrader.BotTerminal
{
    internal abstract class SymbolData
    {
        private IVarSet<FeedCacheKey> _keys;
        private FeedCache.Handler _storage;

        public SymbolData(string name, FeedCache.Handler storage)
        {
            _storage = storage;
            Name = name;
            _keys = storage.Keys.Where(k => k.Symbol == Name);
            SeriesCollection = _keys.Transform(k => new SymbolStorageSeries(k, this, storage)).AsObservable();
        }

        public string Name { get; }
        public abstract string Description { get; }
        public abstract string Key { get; }
        public abstract string Security { get; }
        public abstract bool IsCustom { get; }
        public abstract SymbolEntity InfoEntity { get; }

        public IObservableList<SymbolStorageSeries> SeriesCollection { get; }

        public abstract Task<Tuple<DateTime, DateTime>> GetAvailableRange(TimeFrames timeFrame, BarPriceType? priceType = null);
        public abstract Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken,
            TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to);

        public abstract Task Remove();

        public void WriteSlice(TimeFrames frame, BarPriceType priceType, DateTime from, DateTime to, BarEntity[] values)
        {
            _storage.Put(Name, frame, priceType, from, to, values);
        }

        public void WriteSlice(TimeFrames timeFrame, DateTime from, DateTime to, QuoteEntity[] values)
        {
            _storage.Put(Name, timeFrame, from, to, values);
        }

        public BlockingChannel<Slice<DateTime, BarEntity>> ReadCachedBars(TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            var seriesKey = new FeedCacheKey(Name, timeFrame, priceType);
            return _storage.IterateBarCache(seriesKey, from, to);
        }

        public virtual void OnRemoved()
        {
            _keys.Dispose();
            SeriesCollection.Dispose();
        }
    }

    internal class OnlineSymbolData : SymbolData
    {
        private readonly SymbolModel _symbolInfo;
        private readonly TraderClientModel _client;

        public OnlineSymbolData(SymbolModel symbol, TraderClientModel client)
            : base(symbol.Name, client.FeedHistory.Cache)
        {
            _symbolInfo = symbol ?? throw new ArgumentNullException("symbol");
            _client = client;
        }

        public override bool IsCustom => false;
        public override string Key => "online->" + Name;
        public override string Description => _symbolInfo.Description;
        public override string Security => _symbolInfo.Descriptor.Security;
        public override SymbolEntity InfoEntity => _symbolInfo.Descriptor;

        public override Task<Tuple<DateTime, DateTime>> GetAvailableRange(TimeFrames timeFrame, BarPriceType? priceType = null)
        {
            return _client.FeedHistory.GetAvailableRange(_symbolInfo.Name, priceType ?? BarPriceType.Bid, timeFrame);
        }

        public async override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            var symbol = _symbolInfo.Name;

            observer?.SetMessage("Downloading " + symbol);

            var watch = Stopwatch.StartNew();
            int downloadedCount = 0;

            if (!timeFrame.IsTicks())
            {
                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var barEnumerator = await _client.FeedHistory.DownloadBarSeriesToStorage(symbol, timeFrame, priceType, from, to);

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

                observer?.StartProgress(from.GetAbsoluteDay(), to.GetAbsoluteDay());

                var tickEnumerator = await _client.FeedHistory.DownloadTickSeriesToStorage(symbol, timeFrame, from, to);

                while (await tickEnumerator.ReadNext())
                {
                    var info = tickEnumerator.Current;
                    if (info.Count > 0)
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
    }

    internal class CustomSymbolData : SymbolData
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
        public override SymbolEntity InfoEntity => new SymbolEntity(Name) { IsTradeAllowed = true, SwapEnabled = false, MinAmount = 0.001, MaxAmount = 100000 };

        public override Task<Tuple<DateTime, DateTime>> GetAvailableRange(TimeFrames timeFrame, BarPriceType? priceType = null)
        {
            return _storage.GetRange(new FeedCacheKey(_symbolInfo.Name, timeFrame, priceType));
        }

        public override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, TimeFrames timeFrame, BarPriceType priceType, DateTime from, DateTime to)
        {
            return null;
        }

        public override Task Remove()
        {
            throw new NotImplementedException();
        }
    }

    internal class SymbolStorageSeries
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

        internal Channel<Slice<DateTime, BarEntity>> IterateBarCache(DateTime from, DateTime to)
        {
            return _storage.IterateBarCacheAsync(Key, from, to);
        }
    }
}
