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
        private string _baseFolder;
        private FeedCacheKey _key;
        private DateTime _from;
        private DateTime _to;

        public BarCrossDomainReader(string dbFolder, FeedCacheKey key, DateTime from, DateTime to)
        {
            _baseFolder = dbFolder;
            _key = key;
            _from = from;
            _to = to;
        }

        public IEnumerable<BarEntity> GrtBarStream()
        {
            var poolManager = new SeriesStorage.Lmdb.LmdbManager(_baseFolder, true);

            using (var db = SeriesDatabase.Create(poolManager))
            {
                var series = db.GetSeries(new DateTimeKeySerializer(), new BarSerializer(_key.Frame), b => b.OpenTime, _key.ToCodeString(), false);

                foreach (var bar in series.Iterate(_from, _to))
                    yield return bar;
            }
        }
    }
}
