using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage.StorageBase
{
    internal partial class FeedStorageBase
    {
        internal readonly struct FeedSeriesUpdate
        {
            public DLinqAction Action { get; }

            public FeedCacheKey Key { get; }

            public double SeriesSize { get; }


            internal FeedSeriesUpdate(DLinqAction action, FeedCacheKey key, double size = 0)
            {
                Action = action;
                Key = key;
                SeriesSize = size;
            }
        }


        internal abstract class FeedHandler : ISymbolCollection
        {
            protected readonly VarDictionary<string, BaseSymbol> _symbols = new VarDictionary<string, BaseSymbol>();

            private readonly ActorCallback<FeedSeriesUpdate> _seriesChangeCallback;
            private readonly Ref<FeedStorageBase> _ref;


            public event Action<ISymbolData> SymbolAdded;
            public event Action<ISymbolData> SymbolRemoved;
            public event Action<ISymbolData, ISymbolData> SymbolUpdated;


            public List<ISymbolData> Symbols => _symbols.Values.Cast<ISymbolData>().ToList();

            public ISymbolData this[string name] => _symbols.GetOrDefault(name);


            internal bool IsStarted { get; private set; }

            public string StorageFolder { get; private set; }


            public FeedHandler(Ref<FeedStorageBase> actorRef)
            {
                _ref = actorRef;
                _seriesChangeCallback = ActorCallback.Create<FeedSeriesUpdate>(UpdateSeriesHandler);

                _symbols.Updated += SendCollectionUpdates;
            }


            public async Task Start(string folder)
            {
                IsStarted = true;
                StorageFolder = folder;

                await _ref.Call(a => a.OpenDatabase(folder));
                await SyncSymbolCollection();
                await SyncStorageSeries();
            }

            protected abstract Task SyncSymbolCollection();


            public abstract Task<bool> TryAddSymbol(ISymbolInfo symbol);

            public abstract Task<bool> TryUpdateSymbol(ISymbolInfo symbol);

            public abstract Task<bool> TryRemoveSymbol(string name);


            internal async Task<ActorChannel<ISliceInfo>> ExportSeriesToFile(FeedCacheKey key, IExportSeriesSettings settings)
            {
                var channel = ActorChannel.NewOutput<ISliceInfo>();
                await _ref.OpenChannel(channel, (a, c) => a.ExportSeriesToStorage(c, key, settings));
                return channel;
            }

            protected async Task SyncStorageSeries()
            {
                var snapshot = await _ref.Call(a =>
                {
                    a._seriesListeners.Add(_seriesChangeCallback);
                    return a._series.Snapshot.Select(u => (u.Key, u.Value.GetSize())).ToList();
                });

                foreach (var item in snapshot)
                    if (_symbols.TryGetValue(item.Key.Symbol, out var symbol))
                        symbol.AddSeries(item.Key, item.Item2);
            }

            internal Task Stop()
            {
                _symbols.Clear();

                _ref?.Send(a => a._seriesListeners.Remove(_seriesChangeCallback));

                IsStarted = false;
                StorageFolder = string.Empty;

                return _ref.Call(a => a.CloseDatabase());
            }

            public virtual void Dispose()
            {
                _symbols.Updated -= SendCollectionUpdates;
            }

            //public Task Put(FeedCacheKey key, DateTime from, DateTime to, BarData[] values)
            //    => _ref.Call(a => a.Put(key, from, to, values));

            //public Task Put(string symbol, Feed.Types.Timeframe frame, Feed.Types.MarketSide marketSide, DateTime from, DateTime to, BarData[] values)
            //    => Put(new FeedCacheKey(symbol, frame, marketSide), from, to, values);



            public Task<(DateTime?, DateTime?)> GetRange(FeedCacheKey key) => _ref.Call(a => a.GetRange(key));

            public Task<bool> RemoveSeries(FeedCacheKey seriesKey) => _ref.Call(a => a.RemoveSeries(seriesKey));

            [Conditional("DEBUG")]
            public void PrintSlices(FeedCacheKey key) => _ref.Send(a => a.PrintSlices(key));


            private void UpdateSeriesHandler(FeedSeriesUpdate update)
            {
                if (!_symbols.TryGetValue(update.Key.Symbol, out var symbol))
                    return;

                switch (update.Action)
                {
                    case DLinqAction.Insert:
                        symbol.AddSeries(update.Key);
                        break;
                    case DLinqAction.Remove:
                        symbol.RemoveSeries(update.Key);
                        break;
                    case DLinqAction.Replace:
                        symbol.UpdateSeries(update.Key, update.SeriesSize);
                        break;
                    default:
                        break;
                }
            }

            private void SendCollectionUpdates(DictionaryUpdateArgs<string, BaseSymbol> update)
            {
                switch (update.Action)
                {
                    case DLinqAction.Insert:
                        SymbolAdded?.Invoke(update.NewItem);
                        break;
                    case DLinqAction.Remove:
                        SymbolRemoved?.Invoke(update.OldItem);
                        break;
                    case DLinqAction.Replace:
                        SymbolUpdated?.Invoke(update.OldItem, update.NewItem);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
