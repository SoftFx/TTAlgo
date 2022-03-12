using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;
using TickTrader.FeedStorage.StorageBase;

namespace TickTrader.FeedStorage
{
    internal sealed class SymbolStorageSeries : IStorageSeries
    {
        private readonly FeedStorageBase.FeedHandler _storage;
        private readonly FeedCacheKey _key;


        public ISeriesKey Key => _key;

        public double Size { get; private set; }

        public DateTime? From { get; private set; }

        public DateTime? To { get; private set; }


        public event Action<IStorageSeries> SeriesUpdated;


        public SymbolStorageSeries(FeedCacheKey key, FeedStorageBase.FeedHandler storage)
        {
            _key = key;
            _storage = storage;
        }


        public Task<bool> TryRemove() => _storage.RemoveSeries(_key);


        public Task<ActorChannel<ISliceInfo>> ExportSeriesToFile(IExportSeriesSettings settings)
        {
            return _storage.ExportSeriesToFile(_key, settings);
        }


        internal Task<(DateTime?, DateTime?)> GetSeriesRange()
        {
            return _storage.GetRange(_key);
        }

        internal void Update(FeedSeriesUpdate update)
        {
            Size = update.Size;
            From = update.Range.Item1;
            To = update.Range.Item2;

            SeriesUpdated?.Invoke(this);
        }
    }
}