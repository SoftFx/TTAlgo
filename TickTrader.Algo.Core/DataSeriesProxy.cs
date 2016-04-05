using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
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

        public virtual IPluginDataBuffer<T> Buffer { get; set; }
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

    internal class TimeSeriesProxy : DataSeriesProxy<DateTime>, Api.TimeSeries
    {
        public override DateTime NanValue { get { return DateTime.MinValue; } }
    }

    internal class BarSeriesProxy : DataSeriesProxy<Bar>, Api.BarSeries
    {
        private ProxyBuffer<Bar, double> openBuffer = new ProxyBuffer<Bar, double>(b => b.Open);
        private ProxyBuffer<Bar, double> closeBuffer = new ProxyBuffer<Bar, double>(b => b.Close);
        private ProxyBuffer<Bar, double> highBuffer = new ProxyBuffer<Bar, double>(b => b.High);
        private ProxyBuffer<Bar, double> lowBuffer = new ProxyBuffer<Bar, double>(b => b.Low);
        private ProxyBuffer<Bar, double> volumeBuffer = new ProxyBuffer<Bar, double>(b => b.Volume);
        private ProxyBuffer<Bar, DateTime> openTimeBuffer = new ProxyBuffer<Bar, DateTime>(b => b.OpenTime);

        public BarSeriesProxy()
        {
            Open = new DataSeriesProxy() { Buffer = openBuffer };
            Close = new DataSeriesProxy() { Buffer = closeBuffer };
            High = new DataSeriesProxy() { Buffer = highBuffer };
            Low = new DataSeriesProxy() { Buffer = lowBuffer };
            Volume = new DataSeriesProxy() { Buffer = volumeBuffer };
            OpenTime = new TimeSeriesProxy() { Buffer = openTimeBuffer };
            SymbolCode = string.Empty;
        }

        public string SymbolCode { get; set; }

        public override IPluginDataBuffer<Bar> Buffer
        {
            get { return base.Buffer; }
            set
            {
                base.Buffer = value;
                openBuffer.SrcBuffer = value;
                closeBuffer.SrcBuffer = value;
                highBuffer.SrcBuffer = value;
                lowBuffer.SrcBuffer = value;
                volumeBuffer.SrcBuffer = value;
                openTimeBuffer.SrcBuffer = value;
            }
        }

        public Api.DataSeries Open { get; private set; }
        public Api.DataSeries Close { get; private set; }
        public Api.DataSeries High { get; private set; }
        public Api.DataSeries Low { get; private set; }
        public Api.DataSeries Volume { get; private set; }
        public Api.TimeSeries OpenTime { get; private set; }
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

        public ProxyBuffer(Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
            : this(null, readTransform, writeTransform)
        {
        }

        public ProxyBuffer(IPluginDataBuffer<TSrc> srcBuffer, Func<TSrc, TDst> readTransform, Func<TSrc, TDst, TSrc> writeTransform = null)
        {
            this.srcBuffer = srcBuffer;
            this.readTransform = readTransform;
            this.writeTransform = writeTransform;
        }

        public int Count { get { return srcBuffer.Count; } }
        public IPluginDataBuffer<TSrc> SrcBuffer { get { return srcBuffer; } set { srcBuffer = value; } }

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

    public class InputBuffer<T> : IPluginDataBuffer<T>, IDataBuffer, IDataBuffer<T>
    {
        private List<T> data = new List<T>();
        private BuffersCoordinator coordinator;

        internal InputBuffer(BuffersCoordinator coordinator)
        {
            this.coordinator = coordinator;

            coordinator.BuffersCleared += () => data.Clear();
        }

        public void Append(T rec)
        {
            data.Add(rec);
        }

        public void Append(IEnumerable<T> recRange)
        {
            data.AddRange(recRange);
        }

        public T this[int index]
        {
            get { return data[index]; }
            set
            {
                data[index] = value;
                ItemUpdated(index, value);
            }
        }
        public int Count { get { return data.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }
        internal BuffersCoordinator Coordinator { get { return coordinator; } }
        public event Action<T> ItemAppended = delegate { };
        public event Action<int, T> ItemUpdated = delegate { };

        public T Last
        {
            get { return this[Count - 1]; }
            set { this[Count - 1] = value; }
        }

        object IDataBuffer.this[int index] { get { return data[index]; } set { data[index] = (T)value; } }

        void IDataBuffer.Append(object item)
        {
            Append((T)item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
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

    public abstract class CachingProxyBuffer<TSrc, TDst> : IPluginDataBuffer<TDst>
    {
        private List<TDst> cache = new List<TDst>();
        private BuffersCoordinator coordinator;

        internal CachingProxyBuffer(InputBuffer<TSrc> srcSeries)
        {
            coordinator = srcSeries.Coordinator;
            coordinator.BuffersCleared += () => cache.Clear();

            srcSeries.ItemAppended += i => cache.Add(Convert(i));
        }

        protected abstract TDst Convert(TSrc srcItem);

        public TDst this[int index]
        {
            get { return cache[index]; }
            set { throw new NotSupportedException("Proxy buffers cannot be modified directly!"); }
        }

        public int Count { get { return cache.Count; } }
        public int VirtualPos { get { return coordinator.VirtualPos; } }
    }

    public class BarToQuoteAdapter : CachingProxyBuffer<BarEntity, QuoteEntity>
    {
        public BarToQuoteAdapter(InputBuffer<BarEntity> srcSeries) : base(srcSeries) { }

        protected override QuoteEntity Convert(BarEntity bar)
        {
            return new QuoteEntity() { Ask = bar.Open, Bid = bar.Open };
        }
    }

    public class QuoteToBarAdapter : CachingProxyBuffer<QuoteEntity, Bar>
    {
        public QuoteToBarAdapter(InputBuffer<QuoteEntity> srcSeries) : base(srcSeries) { }

        protected override Bar Convert(QuoteEntity quote)
        {
            return new BarEntity()
            {
                OpenTime = quote.Time,
                Open = quote.Bid,
                Close = quote.Bid,
                High = quote.Bid,
                Low = quote.Bid,
                CloseTime = quote.Time,
                Volume = 1
            };
        }
    }
}
