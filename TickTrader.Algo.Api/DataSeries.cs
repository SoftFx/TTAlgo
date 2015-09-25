using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface DataSeries<T> : IEnumerable<T>
    {
        T this[long index] { get; set; }
        long Count { get; }
    }

    public interface DataSeries : DataSeries<double>
    {
    }
}
