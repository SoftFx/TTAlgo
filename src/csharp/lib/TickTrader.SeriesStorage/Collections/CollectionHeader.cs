using System;
using System.Collections.Generic;

namespace TickTrader.SeriesStorage
{
    internal enum HeaderTypes : byte
    {
        TableName = 0,
        UserMetadata = 1
    };

    internal class CollectionHeader
    {
        public const ushort HeaderKeyPrefix = 0;

        public ushort CollectionId { get; set; }
        public HeaderTypes Type { get; set; }
        public byte[] Content { get; set; }

        public CollectionHeader(KeyValuePair<byte[], byte[]> dbRecord)
        {
            var reader = new BinaryKeyReader(dbRecord.Key);
            if (reader.ReadBeUshort() != HeaderKeyPrefix)
                throw new Exception("Invalid header prefix!");
            Type = (HeaderTypes)reader.ReadByte();
            CollectionId = reader.ReadBeUshort();
            Content = dbRecord.Value;
        }

        public CollectionHeader(ushort collectionId, HeaderTypes type, byte[] content)
        {
            CollectionId = collectionId;
            Type = type;
            Content = content;
        }

        public byte[] GetBinaryKey()
        {
            return GetBinaryKey(CollectionId, Type);
        }

        public HeaderTypes GetHeaderType(KeyValuePair<byte[], byte[]> dbRecord)
        {
            return (HeaderTypes)dbRecord.Key[5];
        }

        public static byte[] GetBinaryKey(ushort collectionId, HeaderTypes headerType)
        {
            var builder = new BinaryKeyWriter(5);
            builder.WriteBe(HeaderKeyPrefix);
            builder.Write((byte)headerType);
            builder.WriteBe(collectionId);
            return builder.Buffer;
        }

        public static bool IsNameRecord(KeyValuePair<byte[], byte[]> dbRecord)
        {
            var key = dbRecord.Key;
            return key.Length == 5 && key[0] == 0 && key[1] == 0 && key[2] == (byte)HeaderTypes.TableName;
        }
    }
}
