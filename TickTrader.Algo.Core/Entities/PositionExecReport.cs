using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PositionExecReport
    {

        public PositionExecReport(OrderExecAction action, PositionEntity position)
        {
            ExecAction = action;
            PositionCopy = position;
        }

        public PositionEntity PositionCopy { get; set; }
        public OrderExecAction ExecAction { get; set; }
    }
}
