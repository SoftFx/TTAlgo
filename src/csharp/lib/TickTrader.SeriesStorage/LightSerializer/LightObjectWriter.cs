using System;
using System.Collections.Generic;

namespace TickTrader.SeriesStorage.LightSerializer
{
    public class LightObjectWriter
    {
        private byte[] stream = new byte[1024];

        public LightObjectWriter()
        {
            Count = 2;
            stream[0] = (byte)'L'; // light serializer token
        }

        public byte ObjectVersion { get { return stream[1]; } set { stream[1] = value; } }

        public void WriteArray<T>(IList<T> array, Action<T, LightObjectWriter> elementSerializer)
        {
            Write(array.Count);
            for (int i = 0; i < array.Count; i++)
            {
                var elementStartOffset = StartObject();
                elementSerializer(array[i], this);
                EndObject(elementStartOffset);
            }
        }

        public void WriteFixedSizeArray<T>(IList<T> array, Action<T, LightObjectWriter> elementSerializer)
        {
            Write(array.Count);
            for (int i = 0; i < array.Count; i++)
                elementSerializer(array[i], this);
        }

        public int StartObject()
        {
            return EnlargeBy(4); // reserve length field
        }

        public void EndObject(int startOffset)
        {
            int objectSize = Count - startOffset;
            ByteConverter.WriteIntLe(objectSize, stream, ref startOffset);
        }

        private int EnlargeBy(int byteCount)
        {
            var offset = Count;
            var newCount = offset + byteCount;
            EnsureCapacity(newCount);
            Count = newCount;
            return offset;
        }

        private void EnsureCapacity(int min)
        {
            if (stream.Length < min)
            {
                int newCapacity = stream.Length * 2;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public void Write(byte value)
        {
            var offset = EnlargeBy(1);
            stream[offset++] = value;
        }

        public void Write(int value)
        {
            var offset = EnlargeBy(4);
            ByteConverter.WriteIntLe(value, stream, ref offset);
        }

        public void Write(long value)
        {
            var offset = EnlargeBy(8);
            ByteConverter.WriteLongLe(value, stream, ref offset);
        }

        public void Write(double value)
        {
            var offset = EnlargeBy(8);
            ByteConverter.WriteDoubleLe(value, stream, ref offset);
        }

        public void Write(DateTime value)
        {
            Write(ToUtc(value).Ticks);
        }

        public void Write(ReadOnlySpan<byte> src)
        {
            var offset = EnlargeBy(src.Length);
            var dst = stream.AsSpan(offset, src.Length);
            src.CopyTo(dst);
        }

        private static DateTime ToUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            else if (dateTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            else
                return dateTime.ToUniversalTime();
        }

        public int Count { get; private set; }

        public int Capacity
        {
            get { return stream.Length; }
            private set
            {
                var newStream = new byte[value];
                Array.Copy(stream, newStream, stream.Length);
                stream = newStream;
            }
        }

        public ArraySegment<byte> GetBuffer()
        {
            return new ArraySegment<byte>(stream, 0, Count);
        }
    }
}
