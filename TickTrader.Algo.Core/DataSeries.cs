using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class DataSeries<T> : Api.DataSeries<T>
    {
        public virtual T this[long index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public long Count { get { return 0; } }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class ReadonlyDataSeries<T> : DataSeries<T>
    {
        public override T this[long index]
        {
            get { return base[index]; }
            set { /* do nothing */ }
        }
    }

    //internal class CachingProxyDataSeries<T> : Api.DataSeries<T>
    //{
    //    public CachingProxyDataSeries()
    //    {
    //    }
    //}

    //internal interface AlgoDataSource
    //{

    //}}
}