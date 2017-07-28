using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public static class ByteConverter
    {
        #region Int

        public static void WriteIntBe(int val, System.IO.Stream s)
        {
            var m = new IntMarshaler { Value = val };
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte1);
        }

        public static void WriteIntLe(int val, System.IO.Stream s)
        {
            var m = new IntMarshaler { Value = val };
            s.WriteByte(m.Byte1);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte4);
        }

        public static void WriteIntBe(int val, byte[] buffer, ref int offset)
        {
            var m = new IntMarshaler { Value = val };
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte1;
        }

        public static void WriteIntLe(int val, byte[] buffer, ref int offset)
        {
            var m = new IntMarshaler { Value = val };
            buffer[offset++] = m.Byte1;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte4;
        }

        public static int ReadIntBe(byte[] buffer, ref int offset)
        {
            var m = new IntMarshaler();
            m.Byte4 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte1 = buffer[offset++];
            return m.Value;
        }

        public static int ReadIntLe(byte[] buffer, ref int offset)
        {
            var m = new IntMarshaler();
            m.Byte1 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte4 = buffer[offset++];
            return m.Value;
        }

        #endregion

        #region Ushort

        public static void WriteUshortBe(ushort val, System.IO.Stream s)
        {
            var m = new IntMarshaler { Value = val };
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte1);
        }

        public static void WriteUshortLe(ushort val, System.IO.Stream s)
        {
            var m = new IntMarshaler { Value = val };
            s.WriteByte(m.Byte1);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte4);
        }

        public static void WriteUshortBe(ushort val, byte[] buffer, ref int offset)
        {
            var m = new IntMarshaler { Value = val };
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte1;
        }

        public static void WriteUshortLe(ushort val, byte[] buffer, ref int offset)
        {
            var m = new IntMarshaler { Value = val };
            buffer[offset++] = m.Byte1;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte4;
        }

        public static ushort ReadUshortBe(byte[] buffer, ref int offset)
        {
            var m = new UshortMarshaler();
            m.Byte2 = buffer[offset++];
            m.Byte1 = buffer[offset++];
            return m.Value;
        }

        public static ushort ReadUshortLe(byte[] buffer, ref int offset)
        {
            var m = new UshortMarshaler();
            m.Byte1 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            return m.Value;
        }

        #endregion

        [StructLayout(LayoutKind.Explicit)]
        public struct IntMarshaler
        {
            [FieldOffset(0)]
            public int Value;
            [FieldOffset(0)]
            public byte Byte1;
            [FieldOffset(1)]
            public byte Byte2;
            [FieldOffset(2)]
            public byte Byte3;
            [FieldOffset(3)]
            public byte Byte4;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct UshortMarshaler
        {
            [FieldOffset(0)]
            public ushort Value;
            [FieldOffset(0)]
            public byte Byte1;
            [FieldOffset(1)]
            public byte Byte2;
        }
    }
}
