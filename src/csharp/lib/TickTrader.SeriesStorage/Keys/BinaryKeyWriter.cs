﻿using System;
using System.Text;

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
            Array.Copy(byteArray, 0, Buffer, _index, byteArray.Length);
            _index += byteArray.Length;
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
            ByteConverter.WriteLongBe(val.Ticks, Buffer, ref _index);
        }

        public void WriteBe(long val)
        {
            ByteConverter.WriteLongBe(val, Buffer, ref _index);
        }

        public void WriteLe(long val)
        {
            ByteConverter.WriteLongLe(val, Buffer, ref _index);
        }

        public void WriteBe(ulong val)
        {
        }
    }
}
