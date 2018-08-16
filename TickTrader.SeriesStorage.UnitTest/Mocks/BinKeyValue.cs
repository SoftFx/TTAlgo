using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.UnitTest
{
    public class BinKeyValue
    {
        public BinKeyValue(byte[] key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        public byte[] Key { get; }
        public byte[] Value { get; }

        public override bool Equals(object obj)
        {
            var other = obj as BinKeyValue;
            return other != null
                && Enumerable.SequenceEqual(other.Key, Key)
                && Enumerable.SequenceEqual(other.Value, Value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
