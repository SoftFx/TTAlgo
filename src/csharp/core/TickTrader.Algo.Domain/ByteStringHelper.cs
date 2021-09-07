using Google.Protobuf;
using System;
using System.Buffers;

namespace TickTrader.Algo.Domain
{
    public static class ByteStringHelper
    {
        // Span<byte> is security critical in NetFx and ByteString.CopyFrom(span) is not marked as SecuritySafeCritical
        // We need to copy data into temp buffer to bypass this limitation
        // TODO: Remove after changing target to .NET Core
        public static ByteString CopyFromUglyHack(ReadOnlySpan<byte> span)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(span.Length);
            span.CopyTo(buffer);
            var res = ByteString.CopyFrom(buffer, 0, span.Length);
            ArrayPool<byte>.Shared.Return(buffer);
            return res;
        }
    }
}
