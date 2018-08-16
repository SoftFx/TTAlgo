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

        #region Uint

        public static void WriteUintBe(uint val, System.IO.Stream s)
        {
            var m = new UintMarshaler { Value = val };
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte1);
        }

        public static void WriteUintLe(uint val, System.IO.Stream s)
        {
            var m = new UintMarshaler { Value = val };
            s.WriteByte(m.Byte1);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte4);
        }

        public static void WriteUintBe(uint val, byte[] buffer, ref int offset)
        {
            var m = new UintMarshaler { Value = val };
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte1;
        }

        public static void WriteUintLe(uint val, byte[] buffer, ref int offset)
        {
            var m = new UintMarshaler { Value = val };
            buffer[offset++] = m.Byte1;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte4;
        }

        public static uint ReadUintBe(byte[] buffer, ref int offset)
        {
            var m = new UintMarshaler();
            m.Byte4 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte1 = buffer[offset++];
            return m.Value;
        }

        public static uint ReadUintLe(byte[] buffer, ref int offset)
        {
            var m = new UintMarshaler();
            m.Byte1 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte4 = buffer[offset++];
            return m.Value;
        }

        #endregion

        #region Long

        public static void WriteLongBe(long val, System.IO.Stream s)
        {
            var m = new LongMarshaler { Value = val };
            s.WriteByte(m.Byte8);
            s.WriteByte(m.Byte7);
            s.WriteByte(m.Byte6);
            s.WriteByte(m.Byte5);
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte1);
        }

        public static void WriteLongLe(long val, System.IO.Stream s)
        {
            var m = new LongMarshaler { Value = val };
            s.WriteByte(m.Byte1);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte5);
            s.WriteByte(m.Byte6);
            s.WriteByte(m.Byte7);
            s.WriteByte(m.Byte8);
        }

        public static void WriteLongBe(long val, byte[] buffer, ref int offset)
        {
            var m = new LongMarshaler { Value = val };
            buffer[offset++] = m.Byte8;
            buffer[offset++] = m.Byte7;
            buffer[offset++] = m.Byte6;
            buffer[offset++] = m.Byte5;
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte1;
        }

        public static void WriteLongLe(long val, byte[] buffer, ref int offset)
        {
            var m = new LongMarshaler { Value = val };
            buffer[offset++] = m.Byte1;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte5;
            buffer[offset++] = m.Byte6;
            buffer[offset++] = m.Byte7;
            buffer[offset++] = m.Byte8;
        }

        public static long ReadLongBe(byte[] buffer, ref int offset)
        {
            var m = new LongMarshaler();
            m.Byte8 = buffer[offset++];
            m.Byte7 = buffer[offset++];
            m.Byte6 = buffer[offset++];
            m.Byte5 = buffer[offset++];
            m.Byte4 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte1 = buffer[offset++];
            return m.Value;
        }

        public static long ReadLongLe(byte[] buffer, ref int offset)
        {
            var m = new LongMarshaler();
            m.Byte1 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte4 = buffer[offset++];
            m.Byte5 = buffer[offset++];
            m.Byte6 = buffer[offset++];
            m.Byte7 = buffer[offset++];
            m.Byte8 = buffer[offset++];
            return m.Value;
        }

        #endregion

        #region Double

        public static void WriteDoubleBe(double val, System.IO.Stream s)
        {
            var m = new DoubleMarshaler { Value = val };
            s.WriteByte(m.Byte8);
            s.WriteByte(m.Byte7);
            s.WriteByte(m.Byte6);
            s.WriteByte(m.Byte5);
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte1);
        }

        public static void WriteDoubleLe(double val, System.IO.Stream s)
        {
            var m = new DoubleMarshaler { Value = val };
            s.WriteByte(m.Byte1);
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte3);
            s.WriteByte(m.Byte4);
            s.WriteByte(m.Byte5);
            s.WriteByte(m.Byte6);
            s.WriteByte(m.Byte7);
            s.WriteByte(m.Byte8);
        }

        public static void WriteDoubleBe(double val, byte[] buffer, ref int offset)
        {
            var m = new DoubleMarshaler { Value = val };
            buffer[offset++] = m.Byte8;
            buffer[offset++] = m.Byte7;
            buffer[offset++] = m.Byte6;
            buffer[offset++] = m.Byte5;
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte1;
        }

        public static void WriteDoubleLe(double val, byte[] buffer, ref int offset)
        {
            var m = new DoubleMarshaler { Value = val };
            buffer[offset++] = m.Byte1;
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte3;
            buffer[offset++] = m.Byte4;
            buffer[offset++] = m.Byte5;
            buffer[offset++] = m.Byte6;
            buffer[offset++] = m.Byte7;
            buffer[offset++] = m.Byte8;
        }

        public static double ReadDoubleBe(byte[] buffer, ref int offset)
        {
            var m = new DoubleMarshaler();
            m.Byte8 = buffer[offset++];
            m.Byte7 = buffer[offset++];
            m.Byte6 = buffer[offset++];
            m.Byte5 = buffer[offset++];
            m.Byte4 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte1 = buffer[offset++];
            return m.Value;
        }

        public static double ReadDoubleLe(byte[] buffer, ref int offset)
        {
            var m = new DoubleMarshaler();
            m.Byte1 = buffer[offset++];
            m.Byte2 = buffer[offset++];
            m.Byte3 = buffer[offset++];
            m.Byte4 = buffer[offset++];
            m.Byte5 = buffer[offset++];
            m.Byte6 = buffer[offset++];
            m.Byte7 = buffer[offset++];
            m.Byte8 = buffer[offset++];
            return m.Value;
        }

        #endregion

        #region Ushort

        public static void WriteUshortBe(ushort val, System.IO.Stream s)
        {
            var m = new UshortMarshaler { Value = val };
            s.WriteByte(m.Byte2);
            s.WriteByte(m.Byte1);
        }

        public static void WriteUshortLe(ushort val, System.IO.Stream s)
        {
            var m = new UshortMarshaler { Value = val };
            s.WriteByte(m.Byte1);
            s.WriteByte(m.Byte2);
        }

        public static void WriteUshortBe(ushort val, byte[] buffer, ref int offset)
        {
            var m = new UshortMarshaler { Value = val };
            buffer[offset++] = m.Byte2;
            buffer[offset++] = m.Byte1;
        }

        public static void WriteUshortLe(ushort val, byte[] buffer, ref int offset)
        {
            var m = new UshortMarshaler { Value = val };
            buffer[offset++] = m.Byte1;
            buffer[offset++] = m.Byte2;
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
        public struct UintMarshaler
        {
            [FieldOffset(0)]
            public uint Value;
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
        public struct LongMarshaler
        {
            [FieldOffset(0)]
            public long Value;
            [FieldOffset(0)]
            public byte Byte1;
            [FieldOffset(1)]
            public byte Byte2;
            [FieldOffset(2)]
            public byte Byte3;
            [FieldOffset(3)]
            public byte Byte4;
            [FieldOffset(4)]
            public byte Byte5;
            [FieldOffset(5)]
            public byte Byte6;
            [FieldOffset(6)]
            public byte Byte7;
            [FieldOffset(7)]
            public byte Byte8;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DoubleMarshaler
        {
            [FieldOffset(0)]
            public double Value;
            [FieldOffset(0)]
            public byte Byte1;
            [FieldOffset(1)]
            public byte Byte2;
            [FieldOffset(2)]
            public byte Byte3;
            [FieldOffset(3)]
            public byte Byte4;
            [FieldOffset(4)]
            public byte Byte5;
            [FieldOffset(5)]
            public byte Byte6;
            [FieldOffset(6)]
            public byte Byte7;
            [FieldOffset(7)]
            public byte Byte8;
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
