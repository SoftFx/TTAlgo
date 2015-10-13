using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal interface IManagedDataSeries
    {
        void Extend();
    }

    internal class ListBasedDataSeries<T> : Api.DataSeries<T>, IManagedDataSeries
    {
        private static readonly T Default;

        static ListBasedDataSeries()
        {
            if (typeof(T) == typeof(double))
                Default = (T)(object)double.NaN;
            else if (typeof(T) == typeof(float))
                Default = (T)(object)float.NaN;
            else
                Default = default(T);
        }

        private ISeriesAccessor<T> dataSrc;
        private int virtualSize;

        public ListBasedDataSeries(ISeriesAccessor<T> dataSrc)
        {
            if (dataSrc == null)
                throw new ArgumentNullException("dataSrc");

            this.dataSrc = dataSrc;
        }

        public void Extend()
        {
            virtualSize++;
        }

        private bool IsInBoundaries(int index)
        {
            return index < 0 || index >= virtualSize;
        }

        private int GetRealIndex(int virtualIndex)
        {
            return virtualSize - virtualIndex - 1;
        }

        public virtual T this[int index]
        {
            get
            {
                if (IsInBoundaries(index))
                    return default(T);
                return dataSrc[GetRealIndex(index)];
            }
            set
            {
                if (IsInBoundaries(index))
                    dataSrc[GetRealIndex(index)] = value;
            }
        }

        public long Count { get { return virtualSize; } }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = virtualSize - 1; i >= 0; i++)
                yield return dataSrc[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class ListBasedDataSeries : ListBasedDataSeries<double>, Api.DataSeries
    {
        public ListBasedDataSeries(ISeriesAccessor<double> dataSrc)
            : base(dataSrc)
        {
        }
    }

    internal class ListBasedReadonlyDataSeries<T> : ListBasedDataSeries<T>
    {
        public ListBasedReadonlyDataSeries(ISeriesAccessor<T> dataSrc)
            : base(dataSrc)
        {
        }

        public override T this[int index]
        {
            get { return base[index]; }
            set { /* Ignore */ }
        }
    }

    public interface ISeriesAccessor<T>
    {
        T this[int index] { get; set; }
    }

    public class SeriesListAdapter<T> : ISeriesAccessor<T>
    {
        private IList<T> list;
        private T defaultVal;

        public SeriesListAdapter(IList<T> list, T defaultVal = default(T))
        {
            this.list = list;
            this.defaultVal = defaultVal;
        }

        public T this[int index]
        {
            get
            {
                if (index >= list.Count)
                    return default(T);
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }
    }
}