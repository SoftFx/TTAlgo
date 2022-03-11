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


        public event Action<double> SeriesUpdated;


        public SymbolStorageSeries(FeedCacheKey key, FeedStorageBase.FeedHandler storage, double size)
        {
            _key = key;
            _storage = storage;

            UpdateSize(size);
        }


        public Task<bool> TryRemove() => _storage.RemoveSeries(_key);


        public Task<ActorChannel<ISliceInfo>> ExportSeriesToFile(IExportSeriesSettings settings)
        {
            return _storage.ExportSeriesToFile(_key, settings);
        }


        internal void UpdateSize(double size)
        {
            Size = size;

            SeriesUpdated?.Invoke(size);
        }
    }
}