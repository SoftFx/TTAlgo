using System;
using System.Collections.Generic;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.Common.Model
{
    [Serializable]
    internal class TickCrossDomainReader : ITickStorage
    {
        private string _baseFolder;
        private FeedCacheKey _key;
        private DateTime _from;
        private DateTime _to;
        private ISeriesDatabase _db;

        public TickCrossDomainReader(string dbFolder, FeedCacheKey key, DateTime from, DateTime to)
        {
            _baseFolder = dbFolder;
            _key = key;
            _from = from;
            _to = to;
        }

        public void Start()
        {
            var poolManager = new SeriesStorage.Lmdb.LmdbManager(_baseFolder, true);
            _db = SeriesDatabase.Create(poolManager);
        }

        public void Stop()
        {
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }

        public IEnumerable<QuoteInfo> GetQuoteStream()
        {
            SeriesStorage<DateTime, QuoteInfo> series = null;

            try
            {
                series = _db.GetSeries(new DateTimeKeySerializer(), TickSerializer.GetSerializer(_key), b => b.Time, _key.ToCodeString(), true);
            }
            catch (DbMissingException)
            {
            }

            if (series != null)
            {
                using (var e = series.Iterate(_from, _to).GetEnumerator())
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
    }
}
