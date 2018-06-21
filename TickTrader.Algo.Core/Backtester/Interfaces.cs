using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IQuoteSeriesStorage : IEnumerable<QuoteEntity>
    {
        IEnumerable<QuoteEntity> Query(DateTime from, DateTime to);
    }

    public interface IBarSeriesStorage : IEnumerable<BarEntity>
    {
        IEnumerable<BarEntity> Query(DateTime from, DateTime to);
    }
}
