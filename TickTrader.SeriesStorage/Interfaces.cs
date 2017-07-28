using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public interface IBinaryStorage
    {
        bool SupportsByteSize { get; }
        IEnumerable<IBinaryCollection> Collections { get; }
    }

    public interface IStorageCollection<TKey, TValue> : IDisposable
    {
        IStorageIterator<TKey, TValue> Iterate(TKey from, bool reversed);
        StorageResultCodes Read(TKey key, out TValue value);
        StorageResultCodes Write(TKey key, TValue value);
        StorageResultCodes Remove(TKey key);
        void Drop(); // deletes whole storage
    }

    public interface ISliceCollection<TKey, TValue> : IStorageCollection<TKey, ISlice<TKey, TValue>>
    {
    }

    public interface IBinaryCollection
    {
        string Name { get; }
        long ByteSize { get; }
    }

    public interface IBinaryStorageCollection<TKey> : IStorageCollection<TKey, ArraySegment<Byte>>
    {
    }

    public interface IStorageIterator<TKey, TValue> : IDisposable
    {
        TKey Key { get; }
        TValue Value { get; }
        StorageResultCodes Next();
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
        ArraySegment<byte> Serialize(ISlice<TKey, TValue> slice);
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

    public enum StorageResultCodes
    {
        Ok,
        ValueIsMissing,
        Error // generic error, no detalization
    }

    public struct KeyRange<TKey>
    {
        public TKey From { get; set; }
        public TKey To { get; set; }
    }
}
