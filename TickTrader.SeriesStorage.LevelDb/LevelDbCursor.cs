using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.LevelDb
{
    internal class LevelDbCursor : IKeyValueBinaryCursor
    {
        private LevelDB.Iterator _iterator;

        public LevelDbCursor(LevelDB.Iterator iterator)
        {
            _iterator = iterator;
        }

        public bool IsValid => _iterator.Valid();
        public void Dispose() => _iterator.Dispose();
        public byte[] GetKey() => _iterator.Key();
        public KeyValuePair<byte[], byte[]> GetRecord() => new KeyValuePair<byte[], byte[]>(_iterator.Key(), _iterator.Value());
        public byte[] GetValue() => _iterator.Value();
        public void MoveToNext() => _iterator.Next();
        public void MoveToPrev() => _iterator.Prev();
        public void Remove() => throw new NotSupportedException();
        public void SeekTo(byte[] key) => _iterator.Seek(key);
        public void SeekToFirst() => _iterator.SeekToFirst();
        public void SeekToLast() => _iterator.SeekToLast();
    }
}
