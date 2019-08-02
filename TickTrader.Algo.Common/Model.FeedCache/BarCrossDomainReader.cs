using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.SeriesStorage;
using TickTrader.SeriesStorage.Protobuf;

namespace TickTrader.Algo.Common.Model
{
    [Serializable]
    internal class BarCrossDomainReader : IBarStorage
    {
        private object _sync = new object();
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

        public IEnumerable<BarEntity> GrtBarStream()
        {
            var series = _db.GetSeries(new DateTimeKeySerializer(), new BarSerializer(_key.Frame), b => b.OpenTime, _key.ToCodeString(), false);

            foreach (var bar in series.Iterate(_from, _to))
                yield return bar;
        }
    }
}
