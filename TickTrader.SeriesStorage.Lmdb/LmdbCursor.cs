using LightningDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.Lmdb
{
    internal class LmdbCursor : IKeyValueBinaryCursor
    {
        private LightningTransaction _tr;
        private LightningCursor _cursor;

        public LmdbCursor(LightningTransaction transaction, string dbName, DatabaseConfiguration cfg, bool readOnly)
        {
            var trFlags = readOnly ? TransactionBeginFlags.ReadOnly : TransactionBeginFlags.None;

            _tr = transaction;
            var db = _tr.OpenDatabase(dbName, cfg);
            _cursor = _tr.CreateCursor(db);
        }

        public bool IsValid { get; private set; }

        public byte[] GetKey() => _cursor.Current.Key;
        public byte[] GetValue() => _cursor.Current.Value;
        public KeyValuePair<byte[], byte[]> GetRecord() => _cursor.Current;
        public void MoveToNext() => IsValid = _cursor.MoveNext();
        public void MoveToPrev() => IsValid = _cursor.MovePrev();
        public void Remove() => _cursor.Delete();
        public void SeekToFirst() => IsValid = _cursor.MoveToFirst();
        public void SeekToLast() => IsValid = _cursor.MoveToLast();

        public void SeekTo(byte[] key)
        {
            //IsValid =  _cursor.MoveToFirst();
            IsValid = _cursor.MoveToFirstAfter(key);
            //if (!IsValid)
            //    _cursor.GetCurrent();
        }

        public void Dispose()
        {
            _cursor.Dispose();
        }
    }
}
