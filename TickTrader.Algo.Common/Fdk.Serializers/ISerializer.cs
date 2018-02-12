using System.Collections.Generic;
using TickTrader.BusinessObjects.QuoteHistory;
using TickTrader.BusinessObjects.QuoteHistory.Store;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public interface IItemsSerializer<in T, TList> where TList : new()
    {
        string FileName { get; }
        byte[] Serialize(IEnumerable<T> items);
        byte[] Serialize(IEnumerable<T> items, out Crc32Hash uncompressedCrc);
        TList Deserialize(byte[] bytes);
        TList Deserialize(byte[] bytes, out Crc32Hash uncompressedCrc);
        ChunkMetaInfo.SerializationMethod SerializerType { get; }
    }
}