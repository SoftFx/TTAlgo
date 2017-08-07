using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public class BinaryKeyWriter : IKeyBuilder
    {
        private int _index;

        public BinaryKeyWriter(int keySize)
        {
            Buffer = new byte[keySize];
        }

        public byte[] Buffer { get; private set; }

        public void Write(byte val)
        {
            Buffer[_index++] = val;
        }

        public void WriteBe(ushort val)
        {
            ByteConverter.WriteUshortBe(val, Buffer, ref _index);
        }

        public void Write(byte[] byteArray)
        {
            throw new NotImplementedException();
        }

        public void Write(string val)
        {
            var valBytes = Encoding.UTF8.GetBytes(val);
            Array.Copy(valBytes, 0, Buffer, _index, valBytes.Length);
            _index += valBytes.Length;
        }

        public void WriteReversed(string val)
        {
            var valBytes = Encoding.UTF8.GetBytes(val);
            Array.Reverse(valBytes);
            Array.Copy(valBytes, 0, Buffer, _index, valBytes.Length);
            _index += valBytes.Length;
        }

        public void WriteBe(int val)
        {
            ByteConverter.WriteIntBe(val, Buffer, ref _index);
        }

        public void WriteBe(uint val)
        {
        }

        public void WriteBe(DateTime val)
        {
        }

        public void WriteBe(long val)
        {
            ByteConverter.WriteLongBe(val, Buffer, ref _index);
        }

        public void WriteBe(ulong val)
        {
        }
    }
}
