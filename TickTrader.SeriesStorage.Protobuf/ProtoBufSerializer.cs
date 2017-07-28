using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.Protobuf
{
    public class ProtoBufSerializer<TKey, TValue> : ISliceSerializer<TKey, TValue>
    {
        public ISlice<TKey, TValue> CreateSlice(KeyRange<TKey> range, TValue[] sliceContent)
        {
            return new SliceImpl { Range = range, Content = sliceContent };
        }

        public ISlice<TKey, TValue> Deserialize(ArraySegment<byte> bytes)
        {
            using (var stream = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count))
                return ProtoBuf.Serializer.Deserialize<SliceImpl>(stream);
        }

        public ArraySegment<byte> Serialize(ISlice<TKey, TValue> slice)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, slice);
                return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        [ProtoContract]
        private class SliceImpl : ISlice<TKey, TValue>
        {
            [ProtoMember(1)]
            public KeyRange<TKey> Range { get; set; }

            [ProtoMember(2)]
            public TValue[] Content { get; set; }
        }
    }
}
