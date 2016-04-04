using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.DataflowConcept
{
    internal interface IDataSeriesProxy
    {
        object Buffer { get; }
    }

    internal class DataSeriesProxy<T> : IDataSeriesProxy, Api.DataSeries<T>
    {
        public DataSeriesProxy()
        {
        }

        public IPluginDataBuffer<T> Buffer { get; set; }
        public virtual T NanValue { get { return default(T); } }
        public bool Readonly { get; set; }

        public int Count { get { return Buffer.VirtualPos; } }

        object IDataSeriesProxy.Buffer { get { return Buffer; } }

        public virtual T this[int index]
        {
            get
            {
                if (IsOutOfBoundaries(index))
                    return NanValue;

                return Buffer[index];
            }

            set
            {
                if (Readonly || IsOutOfBoundaries(index))
                    return;

                int readlIndex = GetRealIndex(index);
                Buffer[readlIndex] = value;
            }
        }

        private int GetRealIndex(int virtualIndex)
        {
            return Count - virtualIndex - 1;
        }

        private bool IsOutOfBoundaries(int index)
        {
            return index < 0 || index >= Buffer.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = Count - 1; i >= 0; i--)
                yield return Buffer[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class DataSeriesProxy : DataSeriesProxy<double>, Api.DataSeries
    {
        public override double NanValue { get { return double.NaN; } }
    }

    internal interface IPluginDataBuffer<T>
    {
        int Count { get; }
        int VirtualPos { get; }
        T this[int index] { get; set; }
    }

    internal class EmptyBuffer<T> : IPluginDataBuffer<T>
    {
        public int Count { get { return 0; } }
        public int VirtualPos { get { return 0; } }
        public T this[int index] { get { return default(T); } set { } }
    }

    internal class ProxyBuffer<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private IPluginDataBuffer<TSrc> srcBuffer;
        private Func<TSrc, TDst> readTransform;
        private Func<TSrc, TDst, TSrc> writeTransform = null;

        public ProxyBuffer(IPluginDataBuffer<TSrc> srcBuffer, Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
        {
            this.srcBuffer = srcBuffer;
            this.readTransform = readTransform;
            this.writeTransform = writeTransform;
        }

        public int Count { get { return srcBuffer.Count; } }

        int IPluginDataBuffer<TDst>.VirtualPos { get { return srcBuffer.VirtualPos; } }

        int VirtualPost { get { return srcBuffer.VirtualPos; } }

        public TDst this[int index]
        {
            get { return readTransform(srcBuffer[index]); }
            set
            {
                TSrc srcRecord = srcBuffer[index];
                srcBuffer[index] = writeTransform(srcRecord, value);
            }
        }
    }

    public class OutputBuffer<T> : IPluginDataBuffer<T>, IReaonlyDataBuffer, IReaonlyDataBuffer<T>
    {
        private List<T> data = new List<T>();
        private BuffersCoordinator coordinator;

        internal OutputBuffer(BuffersCoordinator coordinator)
        {
            this.coordinator = coordinator;

            coordinator.BuffersCleared += () => data.Clear();
            coordinator.BuffersExtended += () => data.Add(default(T));
        }

        public T this[int index] { get { return data[index]; } set { data[index] = value; } }
        public int Count { get { return data.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }

        object IReaonlyDataBuffer.this[int index] { get { return this[index]; } }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}
