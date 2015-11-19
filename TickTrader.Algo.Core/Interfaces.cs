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
        IEnumerable<TRow> ReadNext();
        TRow ReRead();
        void ExtendVirtual();
        IDataSeriesBuffer BindInput(string id, InputFactory factory);
    }

    public interface IAlgoDataWriter<TRow>
    {
        void Init(IList<TRow> inputCache);
        void Extend(TRow row);
        IDataSeriesBuffer BindOutput(string id, OutputFactory factory);
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
        void Init();
        IDataSeriesBuffer BindInput(string id, InputFactory factory);
        IDataSeriesBuffer BindOutput(string id, OutputFactory factory);
        object GetParameter(string id);
        int Read();
        void MoveNext();
    }

    public interface InputFactory
    {
        InputDataSeries<T> CreateInput<T>();
    }

    public interface OutputFactory
    {
        OutputDataSeries<T> CreateOutput<T>();
    }
}
