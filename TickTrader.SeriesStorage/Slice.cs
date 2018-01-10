using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public class Slice<TKey, TValue> : ISlice<TKey>
        where TKey : IComparable
    {
        private Func<TValue, TKey> _keyFunc;

        private Slice(TKey from, TKey to, Func<TValue, TKey> keyFunc, ArraySegment<TValue> content, bool isMissing)
        {
            From = from;
            To = to;
            _keyFunc = keyFunc;
            Content = content;
            IsMissing = isMissing;
            IsEmpty = content.Count == 0;
        }

        public static Slice<TKey, TValue> Create(KeyRange<TKey> range, Func<TValue, TKey> keyFunc, TValue[] content)
        {
            return Create(range.From, range.To, keyFunc, new ArraySegment<TValue>(content));
        }

        public static Slice<TKey, TValue> Create(KeyRange<TKey> range, Func<TValue, TKey> keyFunc, ArraySegment<TValue> content)
        {
            return Create(range.From, range.To, keyFunc, content);
        }

        public static Slice<TKey, TValue> Create(TKey from, TKey to, Func<TValue, TKey> keyFunc, TValue[] content)
        {
            return Create(from, to, keyFunc, new ArraySegment<TValue>(content));
        }

        public static Slice<TKey, TValue> Create(TKey from, TKey to, Func<TValue, TKey> keyFunc, ArraySegment<TValue> content)
        {
            if (content.Array == null)
                throw new ArgumentNullException("content");

            return new Slice<TKey, TValue>(from, to, keyFunc, content, false);
        }

        public static Slice<TKey, TValue> CreateMissing(TKey from, TKey to)
        {
            return new Slice<TKey, TValue>(from, to, null, new ArraySegment<TValue>(), true);
        }

        public TKey From { get; }
        public TKey To { get; }
        public KeyRange<TKey> Range => new KeyRange<TKey>(From, To);
        public ArraySegment<TValue> Content { get; }
        public bool IsEmpty { get; }
        public bool IsMissing { get; }

        public Slice<TKey, TValue> ChangeBounds(TKey from, TKey to)
        {
            return new Slice<TKey, TValue>(from, to, _keyFunc, Content, IsMissing);
        }

        public Slice<TKey, TValue> GetSegment(TKey from, TKey to)
        {
            return GetSegment(new KeyRange<TKey>(from, to));
        }

        public Slice<TKey, TValue> GetSegment(KeyRange<TKey> newRange)
        {
            return GetSliceSegment(newRange, Range, Content, _keyFunc);
        }

        public void Check()
        {
            CheckSlice(From, To, Content, _keyFunc);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", From, To);
        }

        public static Slice<TKey, TValue> GetSliceSegment(KeyRange<TKey> newRange, KeyRange<TKey> sliceRange, ArraySegment<TValue> content, Func<TValue, TKey> keyFunc)
        {
            var list = (IList<TValue>)content;

            if (KeyHelper.IsLess(newRange.From, sliceRange.From))
                throw new Exception();

            if (KeyHelper.IsLess(sliceRange.To, newRange.To))
                throw new Exception();

            if (KeyHelper.IsEqual(newRange.From, sliceRange.From) &&  KeyHelper.IsEqual(newRange.To, sliceRange.To))
                return Create(newRange, keyFunc, content);

            if (content.Count == 0)
                return Create(newRange, keyFunc, new TValue[0]);

            var firstIndex = content.BinarySearch(keyFunc, newRange.From, BinarySearchTypes.NearestHigher);
            var lastIndex = content.BinarySearch(keyFunc, newRange.To, BinarySearchTypes.NearestLower);

            var firstKey = keyFunc(list[firstIndex]);
            var lastKey = keyFunc(list[lastIndex]);

            if (KeyHelper.IsEqual(lastKey, newRange.To) && lastIndex > 0)
            {
                lastIndex--;
                lastKey = keyFunc(list[lastIndex]);
            }

            if (KeyHelper.IsGreaterOrEqual(firstKey, newRange.To) || KeyHelper.IsLess(lastKey, newRange.From))
                return Create(newRange, keyFunc, new TValue[0]);

            var segment = new ArraySegment<TValue>(content.Array, firstIndex + content.Offset, lastIndex - firstIndex + 1);

            return Create(newRange.From, newRange.To, keyFunc, segment);
        }

        public static void CheckSlice(TKey from, TKey to, ArraySegment<TValue> content, Func<TValue, TKey> keyFunc)
        {
            var list = (IList<TValue>)content;

            // check sequence

            TKey prevKey = default(TKey);

            for (int i = 0; i < list.Count; i++)
            {
                var key = keyFunc(list[i]);

                if (i > 0)
                {
                    var cr = key.CompareTo(prevKey);
                    if (cr == 0)
                        throw new ArgumentException("Slice is invalid: Duplicate key at position " + i);
                    else if (cr < 0)
                        throw new ArgumentException("Slice is invalid: Unsorted key at position " + i);
                }
                prevKey = key;
            }

            // chec bounds

            if (KeyHelper.IsGreaterOrEqual(from, to))
                throw new ArgumentException("Invalid key range!");

            if (list.Count > 0)
            {
                var firstKey = keyFunc(list[0]);
                var lastKey = keyFunc(list[list.Count - 1]);

                if (KeyHelper.IsGreater(from, firstKey))
                    throw new ArgumentException("Slice is invalid: Content is outside the left boundary!");

                if (KeyHelper.IsLessOrEqual(to, lastKey))
                    throw new ArgumentException("Slice is invalid: Content is outside the right boundary!");
            }
        }
    }
}
