using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PositionExecReport
    {
        public PositionEntity PositionInfo { get; set; }

        public OrderExecAction ExecAction => PositionInfo.Type;
        public double Volume => PositionInfo.Volume;
        public Domain.OrderInfo.Types.Side Side => PositionInfo.Side;
        public string Symbol => PositionInfo.Symbol;
        public double Price => PositionInfo.Price;
    }
}
