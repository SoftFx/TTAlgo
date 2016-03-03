using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public interface IDataSeriesBuffer
    {
        Type SeriesDataType { get; }
    }

    public interface IAlgoDataReader<TRow>
    {
        TRow ReadAt(int index);
        List<TRow> ReadAt(int index, int pageSize);
        void BindInput(string id, object buffer);
        void Reset();
    }

    public interface IObservableDataReader<TRow> : IAlgoDataReader<TRow>
    {
        event Action<int> Updated;
    }

    public interface IAlgoDataWriter<TRow>
    {
        void Extend(List<TRow> rows);
        void UpdateLast(TRow row);
        void BindOutput(string id, object buffer);
        void Reset();
    }

    public interface CollectionWriter<T, TRow>
    {
        void Append(TRow row, T data);
        void WriteAt(int index, T data, TRow row);
        void Reset();
    }

    public interface IIndicatorBuilder
    {
        void Build();
        void Build(CancellationToken cToken);
        void RebuildLast();
        void Reset();
    }

    public interface IMetadataProvider
    {
    }

    public interface IDataSeriesProvider
    {
    }

    public interface IAccountDataProvider
    {
    }
}
