using System;

namespace TickTrader.Algo.Domain
{
    public partial class PositionInfo
    {
        public bool IsEmpty => Math.Abs(Volume) < 1e-9;
    }
}
