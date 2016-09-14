using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface NetPosition
    {
        string Symbol { get; }
        double Volume { get; }
        OrderSide Side { get; }
    }

    public interface NetPositionList : IEnumerable<NetPosition>
    {
        NetPosition this[string symbol] { get; }

        event Action<NetPositionModifiedEventArgs> Modified;
    }

    public interface NetPositionModifiedEventArgs
    {
        NetPosition OldPosition { get; }
        NetPosition NewPosition { get; }
        bool IsClosed { get; }
    }
}
