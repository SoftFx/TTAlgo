using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace TickTrader.Algo.Core.Lib
{
    public class HexConverter
    {
        private static readonly char[] CharCache = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };


        public static string BytesToString(Span<byte> dataSpan)
        {
            var n = dataSpan.Length;
            var chars = ArrayPool<char>.Shared.Rent(2 * n);

            string res = null;
            try
            {
                var charSpan = chars.AsSpan();

                while (dataSpan.Length > 3)
                {
                    WriteByte(dataSpan, 3, charSpan, 7, 6);
                    WriteByte(dataSpan, 2, charSpan, 5, 4);
                    WriteByte(dataSpan, 1, charSpan, 3, 2);
                    WriteByte(dataSpan, 0, charSpan, 1, 0);

                    dataSpan = dataSpan.Slice(4);
                    charSpan = charSpan.Slice(8);
                }

                while (dataSpan.Length > 0)
                {
                    WriteByte(dataSpan, 0, charSpan, 1, 0);

                    dataSpan = dataSpan.Slice(1);
                    charSpan = charSpan.Slice(2);
                }

                res = new string(chars, 0, 2 * n);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(chars);
            }

            return res;
        }

        public static void StringToBytes(string data, Span<byte> buffer)
        {
            if (data.Length != 2 * buffer.Length)
                throw new ArgumentException("Incorrect buffer length");

            var charSpan = data.AsSpan();

            while (charSpan.Length > 6)
            {
                buffer[3] = ReadByte(charSpan, 7, 6);
                buffer[2] = ReadByte(charSpan, 5, 4);
                buffer[1] = ReadByte(charSpan, 3, 2);
                buffer[0] = ReadByte(charSpan, 1, 0);

                buffer = buffer.Slice(4);
                charSpan = charSpan.Slice(8);
            }

            while (charSpan.Length > 0)
            {
                buffer[0] = ReadByte(charSpan, 1, 0);

                buffer = buffer.Slice(1);
                charSpan = charSpan.Slice(2);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteByte(Span<byte> dataSpan, int i, Span<char> charSpan, int j1, int j0)
        {
            var b = dataSpan[i];
            charSpan[j1] = CharCache[b & 0b1111];
            b = (byte)(b >> 4);
            charSpan[j0] = CharCache[b];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ReadByte(ReadOnlySpan<char> charSpan, int i1, int i0)
        {
            return (byte)((ConvertHexChar(charSpan[i0]) << 4) | ConvertHexChar(charSpan[i1]));
        }

        private static byte ConvertHexChar(char c)
        {
            return (byte)(c < 'a' ? c - '0' : c - 'a' + 10);
        }
    }
}
