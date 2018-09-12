using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.UnitTest
{
    public class KeyValueSnapshot : List<BinKeyValue>
    {
        public void Add(byte[] key, byte[] value)
        {
            Add(new BinKeyValue(key, value));
        }

        public LevelDB.WriteBatch ToLevelDbBatch()
        {
            var batch = new LevelDB.WriteBatch();
            foreach (var item in this)
                batch.Put(item.Key, item.Value);
            return batch;
        }
    }
}
