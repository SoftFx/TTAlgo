using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
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

        public IEnumerable<QuoteEntity> GetQuoteStream()
        {
            SeriesStorage<DateTime, QuoteEntity> series = null;

            try
            {
                series = _db.GetSeries(new DateTimeKeySerializer(), TickSerializer.GetSerializer(_key), b => b.Time, _key.ToCodeString(), true);
            }
            catch (DbMissingException)
            {
            }

            if (series != null)
            {
                var e = series.Iterate(_from, _to).GetEnumerator();

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
