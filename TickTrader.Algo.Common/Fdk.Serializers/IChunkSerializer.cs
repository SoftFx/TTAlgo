using System.Collections.Generic;
using TickTrader.BusinessObjects.QuoteHistory;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public interface IChunkSerializer<in T, TList> where TList : IList<T>, new()
    {
        byte[] Serialize(IEnumerable<T> items);
        byte[] Serialize(IEnumerable<T> items, out Crc32Hash uncompressedCrc);
        TList Deserialize(byte[] bytes);
        TList Deserialize(byte[] bytes, out Crc32Hash uncompressedCrc);

    }
}
