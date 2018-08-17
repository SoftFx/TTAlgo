using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public interface IKeyValueBinaryCursor : IDisposable
    {
        bool IsValid { get; } 

        byte[] GetKey();
        byte[] GetValue();

        KeyValuePair<byte[], byte[]> GetRecord();

        void SeekTo(byte[] key);
        void SeekToLast();
        void SeekToFirst();

        void MoveToNext();
        void MoveToPrev();

        void Remove();
    }

    public interface IKeyValueBinaryStorage : IDisposable
    {
        bool SupportsRemoveAll { get; }
        bool SupportsCursorRemove { get; }

        IKeyValueBinaryCursor CreateCursor();
        bool Read(byte[] key, out byte[] value);
        void Write(byte[] key, byte[] value);
        void Remove(byte[] key);
        void RemoveRange(byte[] from, byte[] to);
        void RemoveAll();
        void CompactRange(byte[] from, byte[] to);
        long GetSize();
        long GetSize(byte[] from, byte[] to);
    }

    public interface IKeyValueBinaryDatabase : IDisposable
    {
        bool SupportsTransactions { get; }

        IKeyValueBinaryStorage GetCollection(string name);
        IEnumerable<string> GetCollections();

        IDisposable StartTransaction();
        void Commit();
    }

    /// <summary>
    /// Interface to manipulate (create, alter, delete) a multiple separate key-value databases (files).
    /// This interface is necessary to create a database pool.
    /// </summary>
    public interface IBinaryStorageManager
    {
        IEnumerable<string> GetStorages();
        IKeyValueBinaryStorage OpenStorage(string name);
    }

    public interface ISeriesDatabase : IDisposable
    {
        IEnumerable<string> Collections { get; }
        IBinaryStorageCollection<TKey> GetBinaryCollection<TKey>(string storageName, IKeySerializer<TKey> keySerializer);
    }

    internal interface IBinaryCollection
    {
        string Name { get; }
    }

    public interface ICollectionStorage<TKey, TValue> : IDisposable
    {
        IEnumerable<KeyValuePair<TKey, TValue>> Iterate(bool reversed = false);
        IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from, bool reversed = false);
        IEnumerable<TKey> IterateKeys(TKey from, bool reversed = false);
        bool Read(TKey key, out TValue value);
        void Write(TKey key, TValue value);
        void Remove(TKey key);
        void RemoveRange(TKey from, TKey to);
        void RemoveAll();
        void Drop(); // deletes whole storage
        long GetSize();
    }

    public interface IBinaryStorageCollection<TKey> : ICollectionStorage<TKey, ArraySegment<byte>>
    {
    }

    //public interface IBinaryStorageFactory
    //{
        
    //}

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
        void WriteLe(long val);
        void WriteBe(ulong val);
        void Write(string val);
        void WriteReversed(string val);
        void Write(byte[] byteArray);
    }

    public interface IKeyReader
    {
        byte ReadByte();
        int ReadBeInt();
        uint ReadBeUnit();
        ushort ReadBeUshort();
        DateTime ReadBeDateTime();
        long ReadBeLong();
        long ReadLeLong();
        ulong RadBeUlong();
        string ReadString();
        string ReadReversedString();
        byte[] ReadByteArray(int size);
    }

    public interface ISlice<T>
    {
        T From { get; }
        T To { get; }
        bool IsMissing { get; }
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

    public interface ISeriesStorage
    {
        double GetSize();
        void Drop();
    }

    public interface ISeriesStorage<TKey> : ISeriesStorage
        where TKey : IComparable
    {
        KeyRange<TKey> GetFirstRange(TKey from, TKey to);
        KeyRange<TKey> GetLastRange(TKey from, TKey to);
        IEnumerable<KeyRange<TKey>> IterateRanges(TKey from, TKey to, bool reversed = false);
    }
}
