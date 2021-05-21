using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface DataSeries<T> : IEnumerable<T>
    {
        T this[int index] { get; set; }
        int Count { get; }
    }

    public interface DataSeries : DataSeries<double>
    {
    }

    public interface TimeSeries : DataSeries<DateTime>
    {
    }

    public interface MarkerSeries : DataSeries<Marker>
    {
    }
}
