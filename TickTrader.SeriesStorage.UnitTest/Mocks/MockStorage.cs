using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.UnitTest.Mocks
{
    internal class MockStorage<TKey, TValue> : ISliceCollection<TKey, TValue>
    {
        private SortedList<TKey, ISlice<TKey, TValue>> slices = new SortedList<TKey, ISlice<TKey, TValue>>();

        public ISlice<TKey, TValue> CreateSlice(TKey from, TKey to, ArraySegment<TValue> sliceContent)
        {
            return new MockSlice<TKey, TValue>(from, to, sliceContent);
        }

        public void Dispose()
        {
        }

        public void Drop()
        {
            RemoveAll();
        }

        public IEnumerable<KeyValuePair<TKey, ISlice<TKey, TValue>>> Iterate(TKey from, bool reversed)
        {
            var index = slices.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

            for (int i = index; i < slices.Count; i++)
                yield return new KeyValuePair<TKey, ISlice<TKey, TValue>>(slices.Keys[i], slices.Values[i]);
        }

        public IEnumerable<TKey> IterateKeys(TKey from, bool reversed)
        {
            var index = slices.Keys.BinarySearch(from, BinarySearchTypes.NearestLower);

            for (int i = index; i < slices.Count; i++)
                yield return slices.Keys[i];
        }

        public ISlice<TKey, TValue> Read(TKey key)
        {
            return slices[key];
        }

        public void Remove(TKey key)
        {
            slices.Remove(key);
        }

        public void RemoveAll()
        {
            slices.Clear();
        }

        public void AddSlice(TKey from, TKey to, params TValue[] values)
        {
            slices[from] = CreateSlice(from, to, new ArraySegment<TValue>(values));
        }

        public void Write(TKey key, ISlice<TKey, TValue> value)
        {
            slices[key] = value;
        }
    }

    internal class MockSlice<TKey, TValue> : ISlice<TKey, TValue>
    {
        public MockSlice(TKey from, TKey to, ArraySegment<TValue> content)
        {
            if (content.Array == null)
                throw new ArgumentException("content");

            From = from;
            To = to;
            Content = content;
        }

        public TKey From { get; }
        public TKey To { get; }
        public ArraySegment<TValue> Content { get; }
        public bool IsEmpty => Content.Count == 0;
        public bool IsMissing => false;

        public override string ToString()
        {
            return From + "-" + To + ": " + string.Join(",", Content.Select(i => i.ToString()));
        }
    }
}
