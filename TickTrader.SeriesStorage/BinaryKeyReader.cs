using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public class BinaryKeyReader : IKeyReader
    {
        private byte[] _buffer;
        private int _index;

        public BinaryKeyReader(byte[] buffer)
        {
            _buffer = buffer;
        }

        public ulong RadBeUlong()
        {
            throw new NotImplementedException();
        }

        public DateTime ReadBeDateTime()
        {
            throw new NotImplementedException();
        }

        public int ReadBeInt()
        {
            return ByteConverter.ReadIntBe(_buffer, ref _index);
        }

        public long ReadBeLong()
        {
            return ByteConverter.ReadLongBe(_buffer, ref _index);
        }

        public uint ReadBeUnit()
        {
            throw new NotImplementedException();
        }

        public ushort ReadBeUshort()
        {
            return ByteConverter.ReadUshortBe(_buffer, ref _index);
        }

        public byte ReadByte()
        {
            return _buffer[_index++];
        }

        public string ReadReversedString()
        {
            throw new NotImplementedException();
        }

        public bool ReadSeparator()
        {
            throw new NotImplementedException();
        }

        public string ReadString()
        {
            throw new NotImplementedException();
        }
    }
}
