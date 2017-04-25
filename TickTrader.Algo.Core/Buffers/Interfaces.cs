using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal interface IDataSeriesProxy
    {
        object Buffer { get; }
    }

    internal interface IPluginDataBuffer<T>
    {
        int Count { get; }
        int VirtualPos { get; }
        T this[int index] { get; set; }
    }

    internal interface IBuffer
    {
        void Extend();
        void Truncate(int size);
        void Clear();
        void BeginBatch();
        void EndBatch();
    }

    public interface IDataBuffer : IEnumerable
    {
        object this[int index] { get; set; }
    }

    public interface IDataBuffer<T> : IEnumerable<T>
    {
        void Append(T item);
        T this[int index] { get; set; }
    }

    public interface IReaonlyDataBuffer : IEnumerable
    {
        object this[int index] { get; }
    }

    public interface IReaonlyDataBuffer<T> : IEnumerable<T>
    {
        T this[int index] { get; }
    }

    public interface IFixedEntry<T>
    {
        Action<T> Changed { set; }
        void CopyFrom(T val);
    }
}
