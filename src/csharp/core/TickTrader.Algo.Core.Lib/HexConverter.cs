using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace TickTrader.Algo.Core.Lib
{
    public class HexConverter
    {
        private static readonly char[] CharCache = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };


        public static string BytesToString(byte[] data)
        {
            var n = data.Length;
            var chars = ArrayPool<char>.Shared.Rent(2 * n);

            var dataSpan = data.AsSpan();
            var charSpan = chars.AsSpan();

            while (dataSpan.Length > 3)
            {
                ConvertByte(dataSpan, 3, charSpan, 7, 6);
                ConvertByte(dataSpan, 2, charSpan, 5, 4);
                ConvertByte(dataSpan, 1, charSpan, 3, 2);
                ConvertByte(dataSpan, 0, charSpan, 1, 0);

                dataSpan = dataSpan.Slice(4);
                charSpan = charSpan.Slice(8);

            }

            while (dataSpan.Length > 0)
            {
                ConvertByte(dataSpan, 0, charSpan, 1, 0);

                dataSpan = dataSpan.Slice(1);
                charSpan = charSpan.Slice(2);
            }

            var res = new string(chars);
            ArrayPool<char>.Shared.Return(chars);
            return res;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ConvertByte(Span<byte> dataSpan, int i, Span<char> charSpan, int j1, int j0)
        {
            var b = dataSpan[i];
            charSpan[j1] = CharCache[b & 0b1111];
            b = (byte)(b >> 4);
            charSpan[j0] = CharCache[b];
        }
    }
}
