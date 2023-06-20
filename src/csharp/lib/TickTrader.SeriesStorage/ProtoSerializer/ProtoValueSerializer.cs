using System;

namespace TickTrader.SeriesStorage.ProtoSerializer
{
    public class ProtoValueSerializer<TValue> : IValueSerializer<TValue>
    {
        public TValue Deserialize(ArraySegment<byte> bytes)
        {
            return ProtoBuf.Serializer.Deserialize<TValue>(bytes.AsSpan());
        }

        public ArraySegment<byte> Serialize(TValue val)
        {
            var buffer = new Lib.ArrayBufferWriter2<byte>(1024);
            ProtoBuf.Serializer.Serialize(buffer, val);
            return new ArraySegment<byte>(buffer.Buffer, 0, buffer.WrittenCount);
        }
    }
}
