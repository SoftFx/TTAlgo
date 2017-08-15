using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.UnitTest.Mocks
{
    internal class MockStorage<TKey, TValue> : ICollectionStorage<TKey, TValue>
    {
        private SortedList<TKey, TValue> list = new SortedList<TKey, TValue>();

        public void Dispose()
        {
        }

        public void Drop()
        {
            RemoveAll();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from)
        {
            var index = list.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

            for (int i = index; i < list.Count; i++)
                yield return new KeyValuePair<TKey, TValue>(list.Keys[i], list.Values[i]);
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            var index = list.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

            for (int i = index; i < list.Count; i++)
                yield return list.Keys[i];
        }

        public bool Read(TKey key, out TValue val)
        {
            return list.TryGetValue(key, out val);
        }

        public void Remove(TKey key)
        {
            list.Remove(key);
        }

        public void RemoveAll()
        {
            list.Clear();
        }

        public void Write(TKey key, TValue value)
        {
            list[key] = value;
        }
    }
}
