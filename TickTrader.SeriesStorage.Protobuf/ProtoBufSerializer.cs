using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.Protobuf
{
    public class ProtoBufSerializer<TKey, TValue> : ISliceSerializer<TKey, TValue>
    {
        public ISlice<TKey, TValue> CreateSlice(TKey from, TKey to, ArraySegment<TValue> sliceContent)
        {
            return new SliceImpl { From = from, To = to, Content = sliceContent };
        }

        public ISlice<TKey, TValue> Deserialize(ArraySegment<byte> bytes)
        {
            using (var stream = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count))
                return ProtoBuf.Serializer.Deserialize<SliceImpl>(stream);
        }

        public byte[] Serialize(ISlice<TKey, TValue> slice)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, slice);
                return stream.ToArray();
                //return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        [ProtoContract]
        private class SliceImpl : ISlice<TKey, TValue>
        {
            [ProtoMember(1)]
            public TKey From { get; set; }

            [ProtoMember(2)]
            public TKey To { get; set; }

            [ProtoMember(3)]
            public ArraySegment<TValue> Content { get; set; }

            public bool IsEmpty => Content.Count == 0;
            public bool IsMissing => false;
        }
    }
}
