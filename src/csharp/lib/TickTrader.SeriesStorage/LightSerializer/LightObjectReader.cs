using System;

namespace TickTrader.SeriesStorage.LightSerializer
{
    public class LightObjectReader
    {
        private byte[] _buffer;
        private int _offset;
        private int _maxOffset;

        public LightObjectReader()
        {
        }

        public void SetDataBuffer(ArraySegment<byte> buffer)
        {
            _buffer = buffer.Array;
            _offset = buffer.Offset;
            _maxOffset = buffer.Offset + buffer.Count;

            if (buffer.Count < 2)
                throw new Exception("Buffer size is less than serialization header!");

            if (_buffer[_offset] != (byte)'L')
                throw new Exception("Token is missing!");

            _offset += 2;
        }

        public byte Version => _buffer[1];

        private void CheckPossibleRead(int readSize)
        {
            if (_offset + readSize > _maxOffset)
                throw new Exception();
        }

        //public T[] ReadArray<T>(Func<LightObjectReader, T> elementDeserializer)
        //{
        //    var count = ReadInt();
        //    var array = new T[count];
        //    for (int i = 0; i < count; i++)
        //    {
        //        var objectSize = ReadInt();
        //        var newOffset = _offset + objectSize;
        //        array[i] = elementDeserializer(this);
        //        _offset = newOffset;
        //    }
        //    return array;
        //}

        public T[] ReadArray<T>(Func<LightObjectReader, T> elementDeserializer)
        {
            var count = ReadInt();
            var array = new T[count];
            for (int i = 0; i < count; i++)
                array[i] = elementDeserializer(this);
            return array;
        }

        public int ReadByte()
        {
            CheckPossibleRead(1);
            return _buffer[_offset++];
        }

        public int ReadInt()
        {
            CheckPossibleRead(4);
            return ByteConverter.ReadIntLe(_buffer, ref _offset);
        }

        public long ReadLong()
        {
            CheckPossibleRead(8);
            return ByteConverter.ReadLongLe(_buffer, ref _offset);
        }

        public DateTime ReadDateTime(DateTimeKind kind)
        {
            return new DateTime(ReadLong(), kind);
        }

        public double ReadDouble()
        {
            CheckPossibleRead(8);
            return ByteConverter.ReadDoubleLe(_buffer, ref _offset);
        }

        public void ReadBytes(Span<byte> dst)
        {
            CheckPossibleRead(dst.Length);

            var src = _buffer.AsSpan(_offset, dst.Length);
            src.CopyTo(dst);
            _offset += dst.Length;
        }
    }
}
