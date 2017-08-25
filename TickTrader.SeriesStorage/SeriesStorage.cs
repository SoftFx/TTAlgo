using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public static class SeriesStorage
    {
        public static SeriesStorage<TKey, TValue> Create<TKey, TValue>(IBinaryStorageFactory factory,
            IKeySerializer<TKey> keySerializer, ISliceSerializer<TValue> valueSerializer, Func<TValue, TKey> keyGetter, string name = null)
            where TKey : IComparable
        {
            var sliceKeySerializer = new KeyRangeSerializer<TKey>(keySerializer);
            var binCollection = factory.GetCollection(name, sliceKeySerializer);
            var sliceCollection = new BinStorageAdapter<KeyRange<TKey>, TValue[]>(binCollection, valueSerializer);
            return new SimpleSeriesStorage<TKey, TValue>(sliceCollection, keyGetter);
        }

        public static SeriesStorage<TKey, TValue> Create<TKey, TValue>(ICollectionStorage<KeyRange<TKey>, TValue[]> collection, Func<TValue, TKey> keyGetter)
           where TKey : IComparable
        {
            return new SimpleSeriesStorage<TKey, TValue>(collection, keyGetter);
        }

        public static SeriesStorage<TKey, TValue> Create<TKey, TValue>(IStorageFactory factory, Func<TValue, TKey> keyGetter)
            where TKey : IComparable
        {
            var sliceCollection = factory.CreateStorage<KeyRange<TKey>, TValue[]>();
            return new SimpleSeriesStorage<TKey, TValue>(sliceCollection, keyGetter);
        }
    }

    public abstract class SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        private Func<TValue, TKey> _keyFunc;

        internal SeriesStorage(ICollectionStorage<KeyRange<TKey>, TValue[]> sliceStorage, Func<TValue, TKey> keyFunc)
        {
            SliceStorage = sliceStorage;
            _keyFunc = keyFunc;
        }

        protected ICollectionStorage<KeyRange<TKey>, TValue[]> SliceStorage { get; }

        protected abstract void WriteInternal(KeyRange<TKey> keyRange, TValue[] values);
        public abstract void Delete(KeyRange<TKey> keyRange);
        public abstract IEnumerable<Slice<TKey, TValue>> IterateSlices(TKey from, TKey to);

        protected void WriteInternal(Slice<TKey, TValue> slice)
        {
            WriteInternal(slice.Range, slice.Content.ToArray());
        }

        public void Write(Slice<TKey, TValue> slice)
        {
            Write(slice.From, slice.To, slice.Content.ToArray());
        }

        public void Write(TKey from, TKey to, params TValue[] values)
        {
            Slice<TKey, TValue>.CheckSlice(from, to, new ArraySegment<TValue>(values), _keyFunc);
            WriteInternal(new KeyRange<TKey>(from, to), values);
        }

        public Slice<TKey, TValue> GetFirstSlice(TKey from, TKey to)
        {
            return IterateSlices(from, to).FirstOrDefault();
        }

        public IEnumerable<TValue> Iterate(TKey from, TKey to)
        {
            foreach (var slice in IterateSlices(from, to))
            {
                if (slice.Content != null)
                {
                    foreach (var item in slice.Content)
                        yield return item;
                }
            }
        }

        public double GetSize()
        {
            return SliceStorage.GetSize();
        }

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

    [Flags]
    public enum SliceIterationOptions
    {
        IterateBackwards = 1,
        AddMissingSlices = 2,
        ReturnUncutSlices = 4
    }

    public class SimpleSeriesStorage<TKey, TValue> : SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        public SimpleSeriesStorage(ICollectionStorage<KeyRange<TKey>, TValue[]> sliceStorage, Func<TValue, TKey> keyFunc) : base(sliceStorage, keyFunc)
        {
        }

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

        public override IEnumerable<Slice<TKey, TValue>> IterateSlices(TKey from, TKey to)
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

    internal class KeyRangeSerializer<TKey> : IKeySerializer<KeyRange<TKey>>
        where TKey : IComparable
    {
        private IKeySerializer<TKey> _oneKeySerializer;

        public KeyRangeSerializer(IKeySerializer<TKey> oneKeySerializer)
        {
            _oneKeySerializer = oneKeySerializer;
        }

        public int KeySize => _oneKeySerializer.KeySize * 2;

        public KeyRange<TKey> Deserialize(IKeyReader reader)
        {
            return new KeyRange<TKey>(
                _oneKeySerializer.Deserialize(reader),
                _oneKeySerializer.Deserialize(reader));
        }

        public void Serialize(KeyRange<TKey> key, IKeyBuilder builder)
        {
            _oneKeySerializer.Serialize(key.From, builder);
            _oneKeySerializer.Serialize(key.To, builder);
        }
    }
}
