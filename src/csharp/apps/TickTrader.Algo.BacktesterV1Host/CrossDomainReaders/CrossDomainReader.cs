using System;
using System.Collections.Generic;
using TickTrader.Algo.Backtester;
using TickTrader.FeedStorage;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.BacktesterV1Host
{
    public sealed class CrossDomainReaderRequest
    {
        public FeedCacheKey Key { get; }

        public DateTime From { get; }

        public DateTime To { get; }


        public CrossDomainReaderRequest(FeedCacheKey key, DateTime from, DateTime to)
        {
            Key = key;
            From = from;
            To = to;
        }
    }


    public abstract class CrossDomainReader<T> : ICrossDomainStorage<T>
    {
        private readonly string _dataBaseFolder;
        protected readonly CrossDomainReaderRequest _request;

        protected ISeriesDatabase _dataBase;


        protected CrossDomainReader(string dataBaseFolder, CrossDomainReaderRequest request)
        {
            _dataBaseFolder = dataBaseFolder;
            _request = request;
        }


        public void Start()
        {
            var poolManager = BinaryStorageManagerFactory.Create(_dataBaseFolder, true);
            _dataBase = SeriesDatabase.Create(poolManager);
        }

        public void Stop()
        {
            _dataBase?.Dispose();
            _dataBase = null;
        }

        public IEnumerable<T> GetStream()
        {
            SeriesStorage<DateTime, T> series = null;

            try
            {
                series = GetSeriesStorage();
            }
            catch (DbMissingException) { }

            if (series != null)
            {
                using (var e = series.Iterate(_request.From, _request.To).GetEnumerator())
                {
                    while (true)
                    {
                        try
                        {
                            if (!e.MoveNext())
                                break;
                        }
                        catch (DbMissingException) { break; }

                        yield return e.Current;
                    }
                }
            }
        }

        protected abstract SeriesStorage<DateTime, T> GetSeriesStorage();
    }
}
