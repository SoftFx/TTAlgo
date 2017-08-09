using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public abstract class SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        private Func<TValue, TKey> _keyFunc;

        public SeriesStorage(ISliceCollection<TKey, TValue> sliceStorage, Func<TValue, TKey> keyFunc)
        {
            SliceStorage = sliceStorage;
            _keyFunc = keyFunc;
        }

        protected ISliceCollection<TKey, TValue> SliceStorage { get; }

        public abstract void Write(KeyRange<TKey> keyRange, IEnumerable<TValue> values);
        public abstract void Delete(KeyRange<TKey> keyRange);
        public abstract IEnumerable<ISlice<TKey, TValue>> IterateSlices(TKey from, TKey to, bool backwards, bool includeMissings);
        public abstract void Append(TValue item);

        public IEnumerable<TValue> Iterate(TKey from, TKey to, bool backwards = false)
        {
            foreach (var slice in IterateSlices(from, to, backwards, false))
            {
                if (slice.Content != null)
                {
                    foreach (var item in slice.Content)
                        yield return item;
                }
            }
        }

        protected ISlice<TKey, TValue> JoinSlices(ISlice<TKey, TValue> slice1, ISlice<TKey, TValue> slice2)
        {
            if (IsLess(slice2.From, slice1.To))
                throw new Exception("Slices cannot be joined together: key ranges are intersected!");

            var from = Min(slice1.From, slice2.From);
            var to = Max(slice1.To, slice2.To);
            var content = Join(slice1.Content, slice2.Content);
            return SliceStorage.CreateSlice(from, to, new ArraySegment<TValue>(content));
        }

        protected ISlice<TKey, TValue> CutSlice(TKey from, TKey to, ISlice<TKey, TValue> slice)
        {
            var segment = GetSliceSegment(from, to, slice);
            var contentCopy = new ArraySegment<TValue>(slice.Content.ToArray());
            return SliceStorage.CreateSlice(segment.From, segment.To, contentCopy);
        }

        protected ISlice<TKey, TValue> GetSliceSegment(TKey from, TKey to, ISlice<TKey, TValue> slice)
        {
            if (IsLess(from, slice.From))
                throw new Exception();

            if (IsLess(slice.To, to))
                throw new Exception();

            if (IsEqual(from, slice.From) && IsEqual(to, slice.To))
                return slice;

            if (slice.Content.Count == 0)
                return SliceStorage.CreateSlice(from, to, new ArraySegment<TValue>(new TValue[0]));

            var firstIndex = slice.Content.BinarySearch(_keyFunc, from, BinarySearchTypes.NearestHigher);
            var lastIndex = slice.Content.BinarySearch(_keyFunc, to, BinarySearchTypes.NearestLower);
            var segment = new ArraySegment<TValue>(slice.Content.Array, firstIndex + slice.Content.Offset, lastIndex - firstIndex + 1);

            return SliceStorage.CreateSlice(from, to, segment);
        }

        protected bool IsLess(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) < 0;
        }

        protected bool IsLessOrEqual(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) <= 0;
        }

        protected bool IsGreater(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) > 0;
        }

        protected bool IsGreaterOrEqual(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) >= 0;
        }

        protected bool IsEqual(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) == 0;
        }

        protected int Compare(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2);
        }

        protected TKey Min(TKey key1, TKey key2)
        {
            return IsLess(key1, key2) ?  key1 : key2;
        }

        protected TKey Max(TKey key1, TKey key2)
        {
            return IsLess(key1, key2) ? key2: key1;
        }

        protected TValue[] Join(TValue[] array1, TValue[] array2)
        {
            TValue[] combined = new TValue[array1.Length + array2.Length];
            Array.Copy(array1, combined, array1.Length);
            Array.Copy(array2, 0, combined, array1.Length, array2.Length);
            return combined;
        }

        protected TValue[] Join(ArraySegment<TValue> segment1, ArraySegment<TValue> segment2)
        {
            TValue[] combined = new TValue[segment1.Count + segment2.Count];
            Array.Copy(segment1.Array, segment1.Offset, combined, 0, segment1.Count);
            Array.Copy(segment2.Array, segment2.Offset, combined, segment1.Count, segment2.Count);
            return combined;
        }
    }

    [Flags]
    public enum SliceIterationOptions
    {
        IterateBackwards = 1,
        AddMissingSlices = 2,
        ReturnUncutSlices = 4
    }

    public class SimpleSeriesStorage<TKey, TValue> : SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        public SimpleSeriesStorage(ISliceCollection<TKey, TValue> sliceStorage, Func<TValue, TKey> keyFunc) : base(sliceStorage, keyFunc)
        {
        }

        public override void Append(TValue item)
        {
            throw new NotImplementedException();
        }

        public override void Delete(KeyRange<TKey> deleteRange)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISlice<TKey, TValue>> IterateSlices(TKey from, TKey to, bool backwards, bool includeMissings)
        {
            foreach (var entry in SliceStorage.Iterate(from, backwards))
            {
                var slice = entry.Value;

                if (IsGreaterOrEqual(slice.From, to))
                    yield break;

                if (IsLessOrEqual(slice.To, from))
                    continue;
                         
                var sliceFrom = IsLess(slice.From, from) ? from : slice.From;
                var sliceTo = IsLess(to, slice.To) ? to : slice.To;

                yield return GetSliceSegment(sliceFrom, sliceTo, slice);
            }
        }

        public override void Write(KeyRange<TKey> range, IEnumerable<TValue> values)
        {

        }
    }
}
