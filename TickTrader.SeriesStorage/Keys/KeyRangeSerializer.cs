using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    internal class KeyRangeSerializer<TKey> : IKeySerializer<KeyRange<TKey>>
         where TKey : IComparable
    {
        private IKeySerializer<TKey> _oneKeySerializer;

        public KeyRangeSerializer(IKeySerializer<TKey> oneKeySerializer)
        {
            _oneKeySerializer = oneKeySerializer;
        }

        public int KeySize => _oneKeySerializer.KeySize * 2;

        public KeyRange<TKey> Deserialize(IKeyReader reader)
        {
            return new KeyRange<TKey>(
                _oneKeySerializer.Deserialize(reader),
                _oneKeySerializer.Deserialize(reader));
        }

        public void Serialize(KeyRange<TKey> key, IKeyBuilder builder)
        {
            _oneKeySerializer.Serialize(key.From, builder);
            _oneKeySerializer.Serialize(key.To, builder);
        }
    }
}
