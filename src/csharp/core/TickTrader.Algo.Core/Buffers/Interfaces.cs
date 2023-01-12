using System;
using System.Collections;
using System.Collections.Generic;

namespace TickTrader.Algo.Core
{
    public interface IDataSeriesProxy
    {
        object Buffer { get; }
    }

    public interface IPluginDataBuffer<T>
    {
        int Count { get; }
        int VirtualPos { get; }
        T this[int index] { get; set; }
    }

    public interface IBuffer
    {
        void Extend();
        void Truncate(int size);
        void Clear();
        void BeginBatch();
        void EndBatch();
    }

    public interface IDataBuffer : IEnumerable
    {
    }

    public interface IDataBuffer<T> : IEnumerable<T>
    {
        void Append(T item);
        T this[int index] { get; set; }
    }

    public interface IReadOnlyDataBuffer : IEnumerable
    {
    }

    public interface IReadOnlyDataBuffer<T> : IEnumerable<T>
    {
        T this[int index] { get; }
    }

    public interface IFixedEntry<T>
    {
        Action<T> Changed { set; }
        void CopyFrom(T val);
    }
}
