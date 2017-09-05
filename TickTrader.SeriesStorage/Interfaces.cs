using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public interface IMulticollectionBinaryStorage : IDisposable, IBinaryStorageFactory
    {
        bool SupportsByteSize { get; }
        IEnumerable<string> Collections { get; }
    }

    public interface IBinaryCollection
    {
        string Name { get; }
        long ByteSize { get; }
    }

    public interface ICollectionStorage<TKey, TValue> : IDisposable
    {
        IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from);
        IEnumerable<TKey> IterateKeys(TKey from, bool reversed);
        bool Read(TKey key, out TValue value);
        void Write(TKey key, TValue value);
        void Remove(TKey key);
        void RemoveAll();
        void Drop(); // deletes whole storage
        long GetSize();
    }

    public interface IBinaryStorageCollection<TKey> : ICollectionStorage<TKey, byte[]>
    {
    }

    public interface IBinaryStorageFactory
    {
        IBinaryStorageCollection<TKey> GetCollection<TKey>(string storageName, IKeySerializer<TKey> keySerializer);
    }

    public interface IStorageFactory
    {
        ICollectionStorage<TKey, TValue> CreateStorage<TKey, TValue>();
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

    public interface ISliceSerializer<T> : IValueSerializer<T[]>
    {
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

    public class KeyRange<TKey> : IComparable
        where TKey : IComparable
    {
        public KeyRange(TKey from, TKey to)
        {
            From = from;
            To = to;
        }

        public TKey From { get; set; }
        public TKey To { get; set; }

        public int CompareTo(object obj)
        {
            var other = (KeyRange<TKey>)obj;

            int result = From.CompareTo(other.From);
            if (result == 0)
                result = To.CompareTo(other.To);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", From, To);
        }
    }
}
