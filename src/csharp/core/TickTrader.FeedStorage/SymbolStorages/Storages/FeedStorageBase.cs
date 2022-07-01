using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;
using TickTrader.FeedStorage.Serializers;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage.StorageBase
{
    internal abstract partial class FeedStorageBase : Actor
    {
        private readonly ActorEvent<FeedSeriesUpdate> _seriesListeners = new ActorEvent<FeedSeriesUpdate>();
        private readonly Dictionary<SeriesFileExtensionsOptions, BaseFileFormatter> _formatters = new Dictionary<SeriesFileExtensionsOptions, BaseFileFormatter>
        {
            [SeriesFileExtensionsOptions.Csv] = new CsvFileFormatter(),
            [SeriesFileExtensionsOptions.Txt] = new TxtFileFormatter(),
        };

        protected readonly VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>> _series = new VarDictionary<FeedCacheKey, ISeriesStorage<DateTime>>();

        protected ISeriesDatabase Database { get; private set; }


        public FeedStorageBase()
        {
            _series.Updated += SendSeriesUpdates;
        }


        private void SendSeriesUpdates(DictionaryUpdateArgs<FeedCacheKey, ISeriesStorage<DateTime>> args) =>
            _seriesListeners.FireAndForget(new FeedSeriesUpdate(args.Action, args.Key));


        protected void OpenDatabase(string folder)
        {
            if (Database != null)
                throw new InvalidOperationException("Already started!");

            Database = SeriesDatabase.Create(StorageFactory.BuildBinaryStorage(folder));

            LoadStoredData();
        }

        protected virtual void LoadStoredData(string skippedCollection = null)
        {
            _series.Clear();

            var loadedKeys = new List<FeedCacheKey>(1 << 5);

            foreach (var collectionName in Database.Collections)
                if (collectionName != skippedCollection && FeedCacheKey.TryParse(collectionName, out var key))
                    loadedKeys.Add(key);

            foreach (var key in loadedKeys)
                CreateCollection(key);
        }

        protected virtual void CloseDatabase()
        {
            if (Database != null)
            {
                Database.Dispose();
                Database = null;
            }
        }

        protected (DateTime?, DateTime?) GetRange(FeedCacheKey key)
        {
            DateTime? min = null;
            DateTime? max = null;

            foreach (var r in IterateCacheKeysInternal(key, DateTime.MinValue, DateTime.MaxValue))
            {
                if (min == null)
                    min = r.From;

                max = r.To;
            }

            return (min, max);
        }

        private async void ExportSeriesToStorage(ActorChannel<ISliceInfo> stream, FeedCacheKey key, IExportSeriesSettings settings)
        {
            await GetFileHandler(key, settings).ExportSeries(stream, settings);
        }

        private async void ImportSeriesToStorage(ActorChannel<ISliceInfo> stream, FeedCacheKey key, IImportSeriesSettings settings)
        {
            await GetFileHandler(key, settings).ImportSeries(stream, settings);
        }

        private IFileHandler GetFileHandler(FeedCacheKey key, IBaseFileSeriesSettings settings)
        {
            if (key.TimeFrame.IsTick())
                return new TickFileHandler(this, _formatters[settings.FileType], key, settings);
            else
                return new BarFileHandler(this, _formatters[settings.FileType], key, settings);
        }

        protected IEnumerable<KeyRange<DateTime>> IterateCacheKeysInternal(FeedCacheKey cacheId, DateTime from, DateTime to)
        {
            for (var i = from; i < to;)
            {
                var page = ReadKeyPage(cacheId, i, to, 20);

                if (page.Count == 0)
                    break;

                foreach (var key in page)
                {
                    yield return key;

                    i = key.To;
                }
            }
        }

        private List<KeyRange<DateTime>> ReadKeyPage(FeedCacheKey key, DateTime from, DateTime to, int pageSize)
        {
            var series = GetSeries(key);
            if (series != null)
                return series.IterateRanges(from, to).Take(pageSize).ToList();

            return new List<KeyRange<DateTime>>();
        }

        internal void Put<T>(FeedCacheKey key, DateTime from, DateTime to, T[] values)
        {
            var collection = GetSeries<T>(key, true);
            collection.Write(from, to, values);

            _seriesListeners.FireAndForget(BuildSeriesUpdate(DLinqAction.Replace, key));
        }

        private FeedSeriesUpdate BuildSeriesUpdate(DLinqAction action, FeedCacheKey key)
        {
            var range = GetRange(key);
            var size = GetSeries(key).GetSize();

            return new FeedSeriesUpdate(action, key, size, range.Item1, range.Item2);
        }

        private ISeriesStorage<DateTime> CreateCollection(FeedCacheKey key)
        {
            ISeriesStorage<DateTime> collection;

            if (key.TimeFrame.IsTick())
                collection = Database.GetSeries(new DateTimeKeySerializer(), TickSerializer.GetSerializer(key), b => b.TimeUtc, key.FullInfo, true);
            else
                collection = Database.GetSeries(new DateTimeKeySerializer(), new BarSerializer(key.TimeFrame), b => b.OpenTime.ToUtcDateTime(), key.FullInfo, false);

            _series.Add(key, collection);

            return collection;
        }

        internal bool RemoveSeries(FeedCacheKey seriesKey)
        {
            if (_series.TryGetValue(seriesKey, out ISeriesStorage<DateTime> series))
            {
                series.Drop();
                _series.Remove(seriesKey);

                return true;
            }

            return false;
        }

        private ISeriesStorage<DateTime> GetSeries(FeedCacheKey key, bool addIfMissing = false)
        {
            if (!_series.TryGetValue(key, out var series) && addIfMissing)
                series = CreateCollection(key);

            return series;
        }

        internal SeriesStorage<DateTime, TVal> GetSeries<TVal>(FeedCacheKey key, bool addIfMissing = false)
        {
            return (SeriesStorage<DateTime, TVal>)GetSeries(key, addIfMissing);
        }
    }
}
