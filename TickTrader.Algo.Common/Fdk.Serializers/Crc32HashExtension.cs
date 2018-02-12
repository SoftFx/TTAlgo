using System;
using ICSharpCode.SharpZipLib.Checksums;
using TickTrader.BusinessObjects.QuoteHistory;

namespace TickTrader.Server.QuoteHistory
{
    public static class Crc32HashExtension
    {

        public static Crc32Hash Compute(this Crc32Hash crc32Hash, byte[] buffer)
        {
            return Compute(buffer);
        }

        public static Crc32Hash Compute(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            var crc32 = new Crc32();
            crc32.Update(buffer);
            return new Crc32Hash((uint)crc32.Value);
        }
    }
}