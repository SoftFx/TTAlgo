using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.SeriesStorage;

namespace TickTrader.Algo.Account.FeedStorage
{
    internal class BarCrossDomainReader : IBarStorage
    {
        private string _baseFolder;
        private FeedCacheKey _key;
        private DateTime _from;
        private DateTime _to;
        private ISeriesDatabase _db;

        public BarCrossDomainReader(string dbFolder, FeedCacheKey key, DateTime from, DateTime to)
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

        public IEnumerable<BarData> GetBarStream()
        {
            SeriesStorage<DateTime, BarData> series = null;

            try
            {
                series = _db.GetSeries(new DateTimeKeySerializer(), new BarSerializer(_key.Frame), b => b.OpenTime.ToDateTime(), _key.ToCodeString(), false);
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
