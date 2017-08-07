using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public abstract class SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        public SeriesStorage(ISliceCollection<TKey, TValue> sliceStorage, Func<TKey, TValue> keyFunc)
        {
            SliceStorage = sliceStorage;
        }

        protected ISliceCollection<TKey, TValue> SliceStorage { get; }

        public abstract void Write(KeyRange<TKey> range, IEnumerable<TValue> values);
        public abstract IEnumerable<ISlice<TKey, TValue>> Iterate(TKey from, TKey to, bool backwards);
        public abstract void Delete(KeyRange<TKey> deleteRange);

        protected static ISlice<TKey, TValue> JoinSlices(ISlice<TKey, TValue> slice1, ISlice<TKey, TValue> slice2)
        {
            throw new NotImplementedException();
        }

        protected static ISlice<TKey, TValue> CutSlice(KeyRange<TKey> partBoundaries, ISlice<TKey, TValue> srcSlice)
        {
            throw new NotImplementedException();
        }
    }

    public class SimpleSeriesStorage<TKey, TValue> : SeriesStorage<TKey, TValue> where TKey : IComparable
    {
        public SimpleSeriesStorage(ISliceCollection<TKey, TValue> sliceStorage, Func<TKey, TValue> keyFunc) : base(sliceStorage, keyFunc)
        {
        }

        public override void Delete(KeyRange<TKey> deleteRange)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ISlice<TKey, TValue>> Iterate(TKey from, TKey to, bool backwards)
        {
            //var i = SliceStorage.Iterate(from, backwards);

            //while (i.IsValid)
            //{
            //    if (i.Code != StorageResultCodes.Ok)
            //        throw new Exception();

            //    yield return i.Value;
            //}
            throw new NotImplementedException();
        }

        public override void Write(KeyRange<TKey> range, IEnumerable<TValue> values)
        {

        }
    }
}
