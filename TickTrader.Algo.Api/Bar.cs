using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Bar
    {
        double Open { get; }
        double Close { get; }
        double High { get; }
        double Low { get; }
        double Volume { get; }
        DateTime OpenTime { get; }
        DateTime CloseTime { get; }
    }
}
