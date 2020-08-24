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

        public static void ReadBytes(this ReadOnlySequence<byte> sequence, Span<byte> dst, int count)
        {
            foreach(var segment in sequence)
            {
                var len = segment.Length;
                if (len < count)
                {
                    segment.Span.CopyTo(dst);
                    dst = dst.Slice(len);
                    count -= len;
                }
                else
                {
                    segment.Span.Slice(0, count).CopyTo(dst);
                    return;
                }
            }
        }
    }
}
