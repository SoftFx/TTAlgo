using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public interface IBinaryStorage : IDisposable
    {
        bool SupportsByteSize { get; }
        IEnumerable<IBinaryCollection> Collections { get; }
    }

    public interface IStorageCollection<TKey, TValue> : IDisposable where TValue : class
    {
        IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from, bool reversed);
        IEnumerable<TKey> IterateKeys(TKey from, bool reversed);
        TValue Read(TKey key);
        void Write(TKey key, TValue value);
        void Remove(TKey key);
        void RemoveAll();
        void Drop(); // deletes whole storage
    }

    public interface ISliceCollection<TKey, TValue> : IStorageCollection<TKey, ISlice<TKey, TValue>>
    {
        ISlice<TKey, TValue> CreateSlice(KeyRange<TKey> range, TValue[] sliceContent);
    }

    public interface IBinaryCollection
    {
        string Name { get; }
        long ByteSize { get; }
    }

    public interface IBinaryStorageCollection<TKey> : IStorageCollection<TKey, byte[]>
    {
    }

    public interface IKeySerializer<TKey>
    {
        int KeySize { get; } // return 0 if key is not of fixed size
        void Serialize(TKey key, IKeyBuilder builder);
        TKey Deserialize(IKeyReader reader);
    }

    public interface IValueSerializer<T>
    {
        ArraySegment<byte> Serialize(T val);
        T Deserialize(ArraySegment<byte> bytes);
    }

    public interface ISliceSerializer<TKey, TValue>
    {
        ISlice<TKey, TValue> CreateSlice(KeyRange<TKey> range, TValue[] sliceContent);
        byte[] Serialize(ISlice<TKey, TValue> slice);
        ISlice<TKey, TValue> Deserialize(ArraySegment<byte> bytes);
    }

    public interface ISlice<TKey, TValue>
    {
        KeyRange<TKey> Range { get; }
        TValue[] Content { get; }
    }

    public interface IKeyBuilder
    {
        void Write(byte val);
        void WriteBe(ushort val);
        void WriteBe(int val);
        void WriteBe(uint val);
        void WriteBe(DateTime val);
        void WriteBe(long val);
        void WriteBe(ulong val);
        void Write(string val);
        void WriteReversed(string val);
    }

    public interface IKeyReader
    {
        byte ReadByte();
        int ReadBeInt();
        uint ReadBeUnit();
        ushort ReadBeUshort();
        DateTime ReadBeDateTime();
        long ReadBeLong();
        ulong RadBeUlong();
        string ReadString();
        string ReadReversedString();
    }

    public struct KeyRange<TKey>
    {
        public TKey From { get; set; }
        public TKey To { get; set; }
    }
}
