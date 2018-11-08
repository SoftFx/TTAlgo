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

        public ITransaction StartTransaction()
        {
            return NoTransaction.Instance;
        }

        public void Drop()
        {
            RemoveAll();
        }

        public long GetSize()
        {
            return 0;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate(TKey from, bool reversed, ITransaction transaction = null)
        {
            if (reversed)
            {
                var index = list.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

                for (int i = index; i >= 0; i--)
                    yield return new KeyValuePair<TKey, TValue>(list.Keys[i], list.Values[i]);
            }
            else
            {
                var index = list.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

                for (int i = index; i < list.Count; i++)
                    yield return new KeyValuePair<TKey, TValue>(list.Keys[i], list.Values[i]);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Iterate(bool reversed = false, ITransaction transaction = null)
        {
            if (reversed)
            {
                for (int i = list.Count -1; i >= 0; i--)
                    yield return new KeyValuePair<TKey, TValue>(list.Keys[i], list.Values[i]);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                    yield return new KeyValuePair<TKey, TValue>(list.Keys[i], list.Values[i]);
            }
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed, ITransaction transaction = null)
        {
            var index = list.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

            for (int i = index; i < list.Count; i++)
                yield return list.Keys[i];
        }

        public bool Read(TKey key, out TValue val, ITransaction transaction = null)
        {
            return list.TryGetValue(key, out val);
        }

        public void Remove(TKey key, ITransaction transaction = null)
        {
            list.Remove(key);
        }

        public void RemoveAll(ITransaction transaction = null)
        {
            list.Clear();
        }

        public void RemoveRange(TKey from, TKey to, ITransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public void Write(TKey key, TValue value, ITransaction transaction = null)
        {
            list[key] = value;
        }
    }
}
