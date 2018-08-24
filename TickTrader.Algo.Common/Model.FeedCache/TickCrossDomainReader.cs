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

        public TickCrossDomainReader(string dbFolder, FeedCacheKey key, DateTime from, DateTime to)
        {
            _baseFolder = dbFolder;
            _key = key;
            _from = from;
            _to = to;
        }

        public IEnumerable<QuoteEntity> GetQuoteStream()
        {
            var poolManager = new SeriesStorage.Lmdb.LmdbManager(_baseFolder, true);

            using (var db = SeriesDatabase.Create(poolManager))
            {
                var series = db.GetSeries(new DateTimeKeySerializer(), new TickSerializer(_key.Symbol), b => b.Time, _key.ToCodeString(), true);

                foreach (var tick in series.Iterate(_from, _to))
                    yield return tick;
            }
        }
    }
}
