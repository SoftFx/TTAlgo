using System.Collections.Generic;

namespace TickTrader.SeriesStorage.Tests
{
    public class KeyValueSnapshot : List<BinKeyValue>
    {
        public void Add(byte[] key, byte[] value)
        {
            Add(new BinKeyValue(key, value));
        }
    }
}
