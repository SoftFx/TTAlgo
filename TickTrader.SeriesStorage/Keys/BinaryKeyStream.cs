using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public class BinaryKeyStream : System.IO.MemoryStream, IKeyBuilder
    {
        public void Write(byte val)
        {
            base.WriteByte(val);
        }

        public void WriteBe(ushort val)
        {
            ByteConverter.WriteUshortBe(val, this);
        }

        public void Write(byte[] byteArray)
        {
            base.Write(byteArray, 0, byteArray.Length);
        }

        public void Write(string val)
        {
            var buff = Encoding.UTF8.GetBytes(val);
            base.Write(buff, 0, buff.Length);
        }

        public void WriteReversed(string val)
        {
            var buff = Encoding.UTF8.GetBytes(val);
            Array.Reverse(buff);
            this.Write(buff, 0, buff.Length);
        }

        public void WriteBe(int val)
        {
            ByteConverter.WriteIntBe(val, this);
        }

        public void WriteBe(uint val)
        {
        }

        public void WriteBe(DateTime val)
        {
        }

        public void WriteBe(long val)
        {
        }

        public void WriteBe(ulong val)
        {
        }
    }
}
