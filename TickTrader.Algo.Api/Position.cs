using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Position
    {
        string Symbol { get; }
    }

    public interface PositionList : IReadOnlyList<Position>
    {
        event Action<PositionModifiedEventArgs> Modified;
    }

    public interface PositionModifiedEventArgs
    {
        Position OldPosition { get; }
        Position NewPosition { get; }
        bool IsRemoved { get; }
    }

    public interface PositionOpenedEventArgs
    {
        Position Position { get; }
    }
}
