using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IDataSeriesBuffer
    {
        void IncrementVirtualSize();
    }

    public abstract class DataSeriesBuffer<T> : Api.DataSeries<T>, IDataSeriesBuffer
    {
        private List<T> buffer = new List<T>();

        protected List<T> Buffer { get { return buffer; } }

        public virtual T this[int index]
        {
            get
            {
                if (IsOutOfBoundaries(index))
                    return default(T);
                return buffer[GetRealIndex(index)];
            }
            set
            {
                if (!IsOutOfBoundaries(index))
                {
                    int readlIndex = GetRealIndex(index);
                    buffer[readlIndex] = value;
                    OnWrite(value, readlIndex);
                }
            }
        }

        protected virtual void OnWrite(T data, int index) { }
        protected virtual void OnReset() { }

        // virtual count
        public int Count { get; private set; }
        // real buffer count
        public int BuffLength { get { return buffer.Count; } }

        public void IncrementVirtualSize()
        {
            if (Count >= BuffLength)
                throw new Exception("Virtual size out of buffer boundaries!");
            Count++;
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

        public void Reset()
        {
            buffer.Clear();
            OnReset();
        }
    }

    public class InputDataSeries<T> : DataSeriesBuffer<T>
    {
        public InputDataSeries()
        {
        }

        public void Append(T val)
        {
            Buffer.Add(val);
        }

        public void Append(IEnumerable<T> range)
        {
            Buffer.AddRange(range);
        }

        public void Update(int index, T val)
        {
        }
    }

    public class InputDataSeries : InputDataSeries<double>, Api.DataSeries
    {
    }

    public class OutputDataSeries<T> : DataSeriesBuffer<T>
    {
        public OutputDataSeries(T defaultVal = default(T))
        {
        }
    }

    public class OutputDataSeries : OutputDataSeries<double>, Api.DataSeries
    {
    }
}