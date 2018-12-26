using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api.Indicators
{
    public interface IFastTrendLineMomentum
    {
        int CountBars { get; }

        DataSeries Price { get; }

        DataSeries Ftlm { get; }
    }
}
