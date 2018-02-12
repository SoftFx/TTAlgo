using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TickTrader.Server.QuoteHistory.Serialization.Binary
{
    public class ByteArrayBuilder
    {
	
        readonly List<byte> _content;

        public ByteArrayBuilder(int capacity)
        {
            _content = new List<byte>(capacity);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddByte(byte b)
        {
            _content.Add(b);
        }
        public void AddFormat(string format, object data)
        {
            var text = String.Format(format, data);
            AddText(text);
        }

        public void AddText(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            _content.AddRange(bytes);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTextAscii(string text)
        {
            foreach (var ch in text)
            {
                _content.Add((byte)ch);
            }

        }

        public void AddRange(byte[] bytes)
        {
            _content.AddRange(bytes);
        }

        public byte[] ToArray()
        {
            return _content.ToArray();
        }

        public void AddTextAsciiObj(object data)
        {
            var text = data.ToString();
            AddTextAscii(text);
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteIntToByteBuffer(byte[] data, int pos, int digits, int value)
        {
            var index = pos + digits - 1;
            for (var i = 0; i < digits; i++)
            {
                var digit = value % 10;
                value /= 10;
                data[index] = (byte)(digit + '0');
                index--;
            }
            return pos + digits;
        }

        public static int ReadIntFromByteBuffer(byte[] data, int pos, int digits)
        {
            int res = 0;
            for (var i = pos; i < pos+digits; i++)
            {
                res = res * 10 + data[i] - '0';
            }
            return res;
        }
    }
}