using System;
using TickTrader.SeriesStorage;

namespace TickTrader.FeedStorage.Serializers
{
    public class GuidKeySerializer : IKeySerializer<Guid>
    {
        public int KeySize => 16;

        public Guid Deserialize(IKeyReader reader)
        {
            return new Guid(reader.ReadByteArray(16));
        }

        public void Serialize(Guid key, IKeyBuilder builder)
        {
            builder.Write(key.ToByteArray());
        }
    }
}
