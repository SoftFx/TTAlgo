using System;
using System.IO;

namespace TickTrader.SeriesStorage.Protobuf
{
    public class ProtoValueSerializer<TValue> : IValueSerializer<TValue>
    {
        public TValue Deserialize(ArraySegment<byte> bytes)
        {
            using (var stream = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count))
                return ProtoBuf.Serializer.Deserialize<TValue>(stream);
        }

        public ArraySegment<byte> Serialize(TValue val)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, val);
                return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }
    }
}
