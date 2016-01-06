using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    //public class AsyncDatasource<TRow> : ReaderBase<TRow>, IAlgoDataReader<TRow>
    //{
    //    private IAlgoObservaleCollection<TRow> srcCollection;
    //    private int rowsReadCount;

    //    public AsyncDatasource(IAlgoObservaleCollection<TRow> srcCollection, ISimpleSyncContext syncContext)
    //    {
    //        this.srcCollection = srcCollection;
    //    }

    //    public IDataSeriesBuffer BindInput(string id, InputFactory factory)
    //    {
    //    }

    //    public event Action<TRow> Appended;
    //    public event Action<IEnumerable<TRow>> AppendedRange;
    //    public event Action Initialized;
    //    public event Action<TRow> Updated;

    //    public class Collection
    //    {
    //        private AsyncDatasource<TRow> dsRef;

    //        internal Collection(AsyncDatasource<TRow> ds)
    //        {
    //            this.dsRef = ds;
    //        }

    //        public void Clear()
    //        {
    //        }

    //        public void Append()
    //        {
    //        }

    //        public void 
    //    }

    //}


    //public interface IAlgoObservaleCollection<T>
    //{
    //    int Count { get; }
    //    T this[int index] { get; }

    //    event Action<int> Updated;
    //    event Action<int> Appended;
    //    event Action Cleared;
    //}
}
