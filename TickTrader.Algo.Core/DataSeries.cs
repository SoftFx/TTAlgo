using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal abstract class DataSeriesBuffer : NoTimeoutByRefObject, IDataSeriesBuffer
    {
        public string Id { get; internal set; }

        public abstract Type SeriesDataType { get; }

        public abstract void IncrementVirtualSize();
        public abstract void Reset();
    }

    internal abstract class DataSeriesBuffer<T> : DataSeriesBuffer, Api.DataSeries<T>
    {
        private List<T> buffer = new List<T>();

        protected List<T> Buffer { get { return buffer; } }
        protected T NanValue { get; set; }

        public virtual T this[int index]
        {
            get
            {
                if (IsOutOfBoundaries(index))
                    return NanValue;
                return buffer[GetRealIndex(index)];
            }
            set
            {
                if (!IsOutOfBoundaries(index))
                {
                    int readlIndex = GetRealIndex(index);
                    OnWrite(value, readlIndex);
                }
            }
        }

        protected virtual void OnWrite(T data, int index) { }
        protected virtual void OnReset() { }

        // virtual count
        public int Count { get; protected set; }
        // real buffer count
        public int BuffLength { get { return buffer.Count; } }

        public override Type SeriesDataType { get { return typeof(T); } }

        public override void IncrementVirtualSize()
        {
            if (Count >= BuffLength)
                throw new Exception("Virtual size out of buffer boundaries!");
            Count++;
        }

        public void Append(T val)
        {
            Buffer.Add(val);
        }

        public void Append(IEnumerable<T> range)
        {
            Buffer.AddRange(range);
        }

        private int GetRealIndex(int virtualIndex)
        {
            return Count - virtualIndex - 1;
        }

        private bool IsOutOfBoundaries(int index)
        {
            return index < 0 || index >= Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                var val = buffer[i];
                yield return val;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override void Reset()
        {
            Count = 0;
            buffer.Clear();
            OnReset();
        }
    }

    internal class InputDataSeries<T> : DataSeriesBuffer<T>
    {
        public InputDataSeries(T nanVal = default(T))
        {
            NanValue = nanVal;
        }

        public void Update(int index, T val)
        {
        }
    }

    internal class InputDataSeries : InputDataSeries<double>, Api.DataSeries
    {
        public InputDataSeries() : base(double.NaN) { }
    }

    internal class OutputDataSeries<T> : DataSeriesBuffer<T>
    {
        public OutputDataSeries(T nanVal = default(T))
        {
            this.NanValue = nanVal;
        }

        protected override void OnWrite(T data, int index)
        {
            Buffer[index] = data;
            Updated(data, index);
        }

        public override void IncrementVirtualSize()
        {
            Append(NanValue);
            Count++;
            Appended(NanValue, Count - 1);
        }

        public event Action<T, int> Appended = delegate { };
        public event Action<T, int> Updated = delegate { };
    }

    internal class OutputDataSeries : OutputDataSeries<double>, Api.DataSeries
    {
        public OutputDataSeries() : base(double.NaN) { }
    }
}