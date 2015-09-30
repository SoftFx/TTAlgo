using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class DataSeries
    {
        public static Api.DataSeries<T> Create<T>(IList<T> dataSrc)
        {
            return new ListBasedDataSeries<T>(dataSrc);
        }

        public static Api.DataSeries<T> CreateReadonly<T>(IList<T> dataSrc)
        {
            return new ListBasedReadonlyDataSeries<T>(dataSrc);
        }
    }

    internal class ListBasedDataSeries<T> : Api.DataSeries<T>
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

        private IList<T> dataSrc;
        private int virtualSize;

        public ListBasedDataSeries(IList<T> dataSrc)
        {
            if (dataSrc == null)
                throw new ArgumentNullException("dataSrc");

            this.dataSrc = dataSrc;
        }

        public void Extend()
        {
            virtualSize++;
            if (dataSrc.Count < virtualSize)
                throw new InvalidOperationException("Cannot extend. Underlying list must be extended first.");
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
        public ListBasedDataSeries(IList<double> dataSrc)
            : base(dataSrc)
        {
        }
    }

    internal class ListBasedReadonlyDataSeries<T> : ListBasedDataSeries<T>
    {
        public ListBasedReadonlyDataSeries(IList<T> dataSrc)
            : base(dataSrc)
        {
        }

        public override T this[int index]
        {
            get { return base[index]; }
            set { /* Ignore */ }
        }
    }
}