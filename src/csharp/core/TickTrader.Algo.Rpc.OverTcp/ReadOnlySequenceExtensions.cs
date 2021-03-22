using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public static class ReadOnlySequenceExtensions
    {
        public static int ReadInt32(this ReadOnlySequence<byte> sequence)
        {
            Span<byte> res = stackalloc byte[4];
            var firstSpan = sequence.First.Span;
            var len = firstSpan.Length;
            if (len >= 4)
            {
                firstSpan = firstSpan.Slice(0, 4);
                firstSpan.CopyTo(res);
            }
            else
            {
                var secondSpan = sequence.Slice(len).First.Span.Slice(0, 4 - len);
                firstSpan.CopyTo(res);
                secondSpan.CopyTo(res.Slice(len));
            }
            return MemoryMarshal.Cast<byte, int>(res)[0];
        }

        public static void WriteInt32(this Span<byte> span, int val)
        {
            var intSpan = MemoryMarshal.Cast<byte, int>(span);
            intSpan[0] = val;
        }
    }
}
