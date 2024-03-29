﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.SeriesStorage
{
    //public static class SeriesStorage
    //{
    //    public static SeriesStorage<TKey, TValue> Create<TKey, TValue>(IStorageFactory factory, Func<TValue, TKey> keyGetter)
    //        where TKey : IComparable
    //    {
    //        var sliceCollection = factory.CreateStorage<KeyRange<TKey>, TValue[]>();
    //        return new NoMetadataSeriesDatabase<TKey, TValue>(sliceCollection, keyGetter);
    //    }
    //}

    public abstract class SeriesStorage<TKey, TValue> : ISeriesStorage<TKey>
        where TKey : IComparable
    {
        private Func<TValue, TKey> _keyFunc;
        private bool _allowDuplicates;

        internal SeriesStorage(Func<TValue, TKey> keyFunc, bool allowDuplicates) //ICollectionStorage<KeyRange<TKey>, TValue[]> sliceStorage,
        {
            _keyFunc = keyFunc;
            _allowDuplicates = allowDuplicates;
        }

        //protected ICollectionStorage<KeyRange<TKey>, TValue[]> SliceStorage { get; }

        protected abstract void WriteInternal(KeyRange<TKey> keyRange, TValue[] values);
        public abstract void Delete(KeyRange<TKey> keyRange);
        public abstract ITransaction StartTransaction();
        protected abstract IEnumerable<Slice<TKey, TValue>> IterateSlicesInternal(TKey from, TKey to, bool reversed, ITransaction transaction);
        protected abstract IEnumerable<KeyRange<TKey>> IterateRangesInternal(TKey from, TKey to, bool reversed, ITransaction transaction);

        protected void WriteInternal(Slice<TKey, TValue> slice, ITransaction transaction = null)
        {
            WriteInternal(slice.Range, slice.Content.ToArray());
        }

        public void Write(Slice<TKey, TValue> slice, ITransaction transaction = null)
        {
            Write(slice.From, slice.To, slice.Content.ToArray());
        }

        public void Write(TKey from, TKey to, params TValue[] values)
        {
            Write(from, to, null, values);
        }

        public void Write(TKey from, TKey to, ITransaction transaction, params TValue[] values)
        {
            Slice<TKey, TValue>.CheckSlice(from, to, new ArraySegment<TValue>(values), _keyFunc, _allowDuplicates);
            WriteInternal(new KeyRange<TKey>(from, to), values);
        }

        public Slice<TKey, TValue> GetFirstSlice(TKey from, TKey to, ITransaction transaction = null)
        {
            return IterateSlicesInternal(from, to, false, transaction).FirstOrDefault();
        }

        public KeyRange<TKey> GetFirstRange(TKey from, TKey to, ITransaction transaction = null)
        {
            return IterateRangesInternal(from, to, false, transaction).FirstOrDefault();
        }

        public KeyRange<TKey> GetLastRange(TKey from, TKey to, ITransaction transaction = null)
        {
            return IterateRangesInternal(from, to, true, transaction).FirstOrDefault();
        }

        public IEnumerable<TValue> IterateReversed(TKey from, TKey to, ITransaction transaction = null)
        {
            return Iterate(from, to, true, transaction);
        }

        public IEnumerable<TValue> Iterate(TKey from, TKey to, bool reversed = false, ITransaction transaction = null)
        {
            foreach (var slice in IterateSlices(from, to, reversed))
            {
                if (!reversed)
                {
                    if (slice.Content != null)
                    {
                        foreach (var item in slice.Content)
                            yield return item;
                    }
                }
                else
                {
                    var array = slice.Content.Array;
                    var offset = slice.Content.Offset;
                    var count = slice.Content.Count;

                    for (int i = offset + count - 1; i >= offset; i--)
                        yield return array[i];
                }
            }
        }

        public IEnumerable<Slice<TKey, TValue>> IterateSlices(TKey from, TKey to, bool reversed = false, ITransaction transaction = null)
        {
            return IterateSlicesInternal(from, to, reversed, transaction);
        }

        public IEnumerable<KeyRange<TKey>> IterateRanges(TKey from, TKey to, bool reversed = false, ITransaction transaction = null)
        {
            return IterateRangesInternal(from, to, reversed, transaction);
        }

        public abstract double GetSize();

        public abstract void Drop();


        protected Slice<TKey, TValue> JoinSlices(Slice<TKey, TValue> slice1, Slice<TKey, TValue> slice2)
        {
            if (KeyHelper.IsLess(slice2.From, slice1.To))
                throw new Exception("Slices cannot be joined together: key ranges are intersected!");

            var from = KeyHelper.Min(slice1.From, slice2.From);
            var to = KeyHelper.Max(slice1.To, slice2.To);
            var content = Join(slice1.Content, slice2.Content);
            return Slice<TKey, TValue>.Create(from, to, _keyFunc, content);
        }

        protected Slice<TKey, TValue> CutSlice(TKey from, TKey to, Slice<TKey, TValue> slice)
        {
            var segment = GetSliceSegment(from, to, slice);
            var contentCopy = new ArraySegment<TValue>(slice.Content.ToArray());
            return Slice<TKey, TValue>.Create(segment.From, segment.To, _keyFunc, contentCopy);
        }

        protected Slice<TKey, TValue> GetSliceSegment(TKey from, TKey to, Slice<TKey, TValue> slice)
        {
            return Slice<TKey, TValue>.GetSliceSegment(new KeyRange<TKey>(from, to), slice.Range, slice.Content, _keyFunc);
        }

        protected Slice<TKey, TValue> GetSliceSegment(KeyRange<TKey> newRange, KeyRange<TKey> sliceRange, TValue[] content)
        {
            return Slice<TKey, TValue>.GetSliceSegment(newRange, sliceRange, new ArraySegment<TValue>(content), _keyFunc);
        }

        protected TKey GetKey(TValue value)
        {
            return _keyFunc(value);
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

    //[Flags]
    //public enum SliceIterationOptions
    //{
    //    IterateBackwards = 1,
    //    AddMissingSlices = 2,
    //    ReturnUncutSlices = 4
    //}
}
