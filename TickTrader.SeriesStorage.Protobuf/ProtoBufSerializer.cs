using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.Protobuf
{
    public class ProtoBufSerializer<TValue> : IValueSerializer<TValue>
    {
        //public Slice<TKey, TValue> Deserialize(ArraySegment<byte> bytes)
        //{
        //    using (var stream = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count))
        //        return ProtoBuf.Serializer.Deserialize<SliceImpl>(stream);
        //}

        //public byte[] Serialize(ISlice<TKey, TValue> slice)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        ProtoBuf.Serializer.Serialize(stream, slice);
        //        return stream.ToArray();
        //        //return new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        //    }
        //}

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
