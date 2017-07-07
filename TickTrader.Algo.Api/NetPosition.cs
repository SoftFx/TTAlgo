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
        double Margin { get; }
        double Profit { get; }
        double Commission { get; }
        double Swap { get; }
    }

    public interface NetPositionList : IEnumerable<NetPosition>
    {
        int Count { get; }
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
