using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public interface IDataSeriesBuffer
    {
        void IncrementVirtualSize();
    }

    public interface IAlgoDataReader<TRow>
    {
        TRow ReadAt(int index);
        List<TRow> ReadAt(int index, int pageSize);
        void BindInput<T>(string id, InputDataSeries<T> buffer);
    }

    public interface IObservableDataReader<TRow> : IAlgoDataReader<TRow>
    {
        event Action<int> Updated;
    }

    public interface IAlgoDataWriter<TRow>
    {
        void Extend(List<TRow> rows);
        void UpdateLast(TRow row);
        void BindOutput<T>(string id, OutputDataSeries<T> buffer);
    }

    public interface InputStream<TRow>
    {
        bool ReadNext(out TRow rec);
    }

    public interface CollectionWriter<T, TRow>
    {
        void Append(TRow row, T data);
        void WriteAt(int index, T data, TRow row);
    }

    public interface IAlgoContext
    {
        void BindInput<T>(string id, InputDataSeries<T> buffer);
        void BindOutput<T>(string id, OutputDataSeries<T> buffer);
        object GetParameter(string id);
    }
}
