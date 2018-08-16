using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public static class SeriesDatabase
    {
        /// <summary>
        /// Creates a multi-series database based on collection emulator. All series stored in single binary collection. This method useful
        /// when your key-value storage implementation does not support multiple collections but support compaction/shrinking.
        /// </summary>
        /// <param name="storageImpl"></param>
        /// <returns></returns>
        public static ISeriesDatabase Create(IKeyValueBinaryStorage storageImpl)
        {
            return new CollectionEmulator(storageImpl);
        }

        /// <summary>
        /// Creates a multi-series database based on database pool. Each series stored in separate key-value database.
        /// It useful when you key-value storage implementaion does not support shrinking/compaction.
        /// </summary>
        /// <param name="multiDbManager"></param>
        /// <returns></returns>
        public static ISeriesDatabase Create(IBinaryStorageManager multiDbManager)
        {
            return new DatabasePool(multiDbManager);
        }

        public static SeriesStorage<TKey, TValue> GetSeries<TKey, TValue>(this ISeriesDatabase factory,
            IKeySerializer<TKey> keySerializer, ISliceSerializer<TValue> valueSerializer, Func<TValue, TKey> keyGetter, string name = null)
            where TKey : IComparable
        {
            var sliceKeySerializer = new KeyRangeSerializer<TKey>(keySerializer);
            var binCollection = factory.GetBinaryCollection(name, sliceKeySerializer);
            var sliceCollection = new CollectionSerializer<KeyRange<TKey>, TValue[]>(binCollection, valueSerializer);
            return new SeriesStorageNoMetadata<TKey, TValue>(sliceCollection, keyGetter);
        }

        public static SeriesStorage<TKey, TValue> GetSeries<TKey, TValue>(this ICollectionStorage<KeyRange<TKey>, TValue[]> collection, Func<TValue, TKey> keyGetter)
           where TKey : IComparable
        {
            return new SeriesStorageNoMetadata<TKey, TValue>(collection, keyGetter);
        }
    }
}
