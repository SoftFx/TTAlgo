using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.SeriesStorage
{
    internal class SeriesStorageNoMetadata<TKey, TValue> : SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        public SeriesStorageNoMetadata(ICollectionStorage<KeyRange<TKey>, TValue[]> sliceStorage, Func<TValue, TKey> keyFunc, bool allowDuplicates)
            : base(keyFunc, allowDuplicates)
        {
            SliceStorage = sliceStorage;
        }

        public ICollectionStorage<KeyRange<TKey>, TValue[]> SliceStorage {get;}

        public override void Delete(KeyRange<TKey> deleteRange)
        {
            // find all existing slices
            var existingRanges = GetCoveredSlices(deleteRange.From, deleteRange.To).ToList();

            // TO DO : cut first and last if required

            Slice<TKey, TValue> openSlice = null;
            Slice<TKey, TValue> closeSlice = null;

            if (existingRanges.Count > 0)
            {
                var firstSliceRange = existingRanges[0];
                var lastSliceRange = existingRanges[existingRanges.Count - 1];
                TValue[] firstSliceContent = null;
                TValue[] lastSliceContent = null;

                if (KeyHelper.IsGreater(deleteRange.From, firstSliceRange.From))
                {
                    if (SliceStorage.Read(firstSliceRange, out firstSliceContent))
                        openSlice = GetSliceSegment(new KeyRange<TKey>(firstSliceRange.From, deleteRange.From), firstSliceRange, firstSliceContent);
                }

                if (KeyHelper.IsLess(deleteRange.To, lastSliceRange.To))
                {
                    if (existingRanges.Count == 1) // first is last
                    {
                        if (firstSliceContent != null)
                            closeSlice = GetSliceSegment(new KeyRange<TKey>(deleteRange.To, firstSliceRange.To), firstSliceRange, firstSliceContent);
                    }
                    else if (SliceStorage.Read(lastSliceRange, out lastSliceContent))
                        closeSlice = GetSliceSegment(new KeyRange<TKey>(deleteRange.To, lastSliceRange.To), lastSliceRange, lastSliceContent);
                }
            }

            // delete all existing

            foreach (var er in existingRanges)
                SliceStorage.Remove(er);

            // write open and close segments

            if (openSlice != null)
                WriteInternal(openSlice);

            if (closeSlice != null)
                WriteInternal(closeSlice);
        }

        public override void Drop()
        {
            SliceStorage.Drop();
        }

        public override double GetSize()
        {
            return SliceStorage.GetSize();
        }

        public override ITransaction StartTransaction()
        {
            return SliceStorage.StartTransaction();
        }

        protected override IEnumerable<Slice<TKey, TValue>> IterateSlicesInternal(TKey from, TKey to, bool reversed, ITransaction transaction)
        {
            if (!reversed)
            {
                var fromKey = new KeyRange<TKey>(from, default(TKey));

                foreach (var entry in SliceStorage.Iterate(fromKey))
                {
                    var range = entry.Key;
                    var content = entry.Value;

                    if (KeyHelper.IsGreaterOrEqual(range.From, to))
                        yield break;

                    if (KeyHelper.IsLessOrEqual(range.To, from))
                        continue;

                    var sliceFrom = KeyHelper.IsLess(range.From, from) ? from : range.From;
                    var sliceTo = KeyHelper.IsLess(to, range.To) ? to : range.To;

                    yield return GetSliceSegment(new KeyRange<TKey>(sliceFrom, sliceTo), range, content);
                }
            }
            else
            {
                var toKey = new KeyRange<TKey>(to, default(TKey));

                foreach (var entry in SliceStorage.Iterate(toKey, true))
                {
                    var range = entry.Key;
                    var content = entry.Value;

                    if (KeyHelper.IsLessOrEqual(range.To, from))
                        yield break;

                    var sliceFrom = KeyHelper.IsLess(range.From, from) ? from : range.From;
                    var sliceTo = KeyHelper.IsLess(to, range.To) ? to : range.To;

                    yield return GetSliceSegment(new KeyRange<TKey>(sliceFrom, sliceTo), range, content);
                }
            }
        }

        protected override IEnumerable<KeyRange<TKey>> IterateRangesInternal(TKey from, TKey to, bool reversed, ITransaction transaction)
        {
            if (!reversed)
            {
                var fromKey = new KeyRange<TKey>(from, default(TKey));

                foreach (var range in SliceStorage.IterateKeys(fromKey, false))
                {
                    if (KeyHelper.IsGreaterOrEqual(range.From, to))
                        yield break;

                    if (KeyHelper.IsLessOrEqual(range.To, from))
                        continue;

                    var sliceFrom = KeyHelper.IsLess(range.From, from) ? from : range.From;
                    var sliceTo = KeyHelper.IsLess(to, range.To) ? to : range.To;

                    yield return new KeyRange<TKey>(sliceFrom, sliceTo);
                }
            }
            else
            {
                var toKey = new KeyRange<TKey>(to, default(TKey));

                foreach (var range in SliceStorage.IterateKeys(toKey, true))
                {
                    if (KeyHelper.IsLessOrEqual(range.To, from))
                        yield break;

                    var sliceFrom = KeyHelper.IsLess(range.From, from) ? from : range.From;
                    var sliceTo = KeyHelper.IsLess(to, range.To) ? to : range.To;

                    yield return new KeyRange<TKey>(sliceFrom, sliceTo);
                }
            }
        }

        protected override void WriteInternal(KeyRange<TKey> range, TValue[] values)
        {
            Delete(range);
            SliceStorage.Write(range, values);
        }

        private IEnumerable<KeyRange<TKey>> GetCoveredSlices(TKey from, TKey to)
        {
            var fromKey = new KeyRange<TKey>(from, default(TKey));

            foreach (var range in SliceStorage.IterateKeys(fromKey, false))
            {
                if (KeyHelper.IsGreaterOrEqual(range.From, to))
                    yield break;

                if (KeyHelper.IsLessOrEqual(range.To, from))
                    continue;

                yield return range;
            }
        }
    }
}
