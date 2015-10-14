using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal interface IDataSeries
    {
        void Reset();
    }

    internal interface IInputDataSeries : IDataSeries
    {
        void ReRead();
        void ReadNext();
    }

    internal interface IOutputDataSeries : IDataSeries
    {
        void ExtendBuffer();
    }

    internal abstract class DataSeriesBase<T> : Api.DataSeries<T>, IDataSeries
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

        public int Count { get { return buffer.Count; } }

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

        void IDataSeries.Reset()
        {
            buffer.Clear();
            OnReset();
        }
    }

    internal class InputDataSeries<T> : DataSeriesBase<T>, IInputDataSeries
    {
        private DataSeriesReader<T> reader;

        public InputDataSeries(DataSeriesReader<T> reader)
        {
            this.reader = reader;
        }

        void IInputDataSeries.ReRead()
        {
            if (Count > 0)
                Buffer[Count - 1] = reader.ReRead();
        }

        void IInputDataSeries.ReadNext()
        {
            Buffer.Add(reader.ReadNext());
        }

        protected override void OnReset()
        {
            reader.Reset();
        }
    }

    internal class InputDataSeries : InputDataSeries<double>, Api.DataSeries
    {
        public InputDataSeries(DataSeriesReader<double> reader) : base(reader) { }
    }

    internal class OutputDataSeries<T> : DataSeriesBase<T>, IOutputDataSeries
    {
        private DataSeriesWriter<T> writer;
        private T defaultVal;

        public OutputDataSeries(DataSeriesWriter<T> writer, T defaultVal = default(T))
        {
            this.writer = writer;
            this.defaultVal = defaultVal;
        }

        void IOutputDataSeries.ExtendBuffer()
        {
            Buffer.Add(defaultVal);
        }

        protected override void OnWrite(T data, int index)
        {
            writer.WriteAt(index, data);
        }

        protected override void OnReset()
        {
            writer.Reset();
        }
    }

    internal class OutputDataSeries : OutputDataSeries<double>, Api.DataSeries
    {
        public OutputDataSeries(DataSeriesWriter<double> writer) : base(writer) { }
    }
}