using LightningDB;
using System.Collections.Generic;

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

        public byte[] GetKey() => _cursor.GetCurrent().key.CopyToNewArray();
        public byte[] GetValue() => _cursor.GetCurrent().value.CopyToNewArray();
        public void MoveToNext() => IsValid = _cursor.Next() == MDBResultCode.Success;
        public void MoveToPrev() => IsValid = _cursor.Previous() == MDBResultCode.Success;
        public void Remove() => _cursor.Delete();
        public void SeekToFirst() => IsValid = _cursor.First() == MDBResultCode.Success;
        public void SeekToLast() => IsValid = _cursor.Last() == MDBResultCode.Success;

        public KeyValuePair<byte[], byte[]> GetRecord()
        {
            var cur = _cursor.GetCurrent();

            if (cur.resultCode == MDBResultCode.Success)
                return new KeyValuePair<byte[], byte[]>(cur.key.CopyToNewArray(), cur.value.CopyToNewArray());
            else
                return default;
        }

        public void SeekTo(byte[] key)
        {
            //IsValid =  _cursor.MoveToFirst();
            IsValid = _cursor.SetRange(key) == MDBResultCode.Success;
            //if (!IsValid)
            //    _cursor.GetCurrent();
        }

        public void Dispose()
        {
            _cursor.Dispose();
        }
    }
}
