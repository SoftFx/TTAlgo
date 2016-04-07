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

    public interface IDataBuffer : IEnumerable
    {
        void Append(object item);
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
}
