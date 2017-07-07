using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PositionExecReport
    {
        public double AgentCommission { get; set; }
        public TradeVolume Volume { get; set; }
        public double Commission { get; set; }
        public double Price { get; set; }
        public double SettlementPrice { get; set; }
        public OrderSide Side { get; set; }
        public double Swap { get; set; }
        public string Symbol { get; set; }
        public OrderExecAction ExecAction { get; set; }
    }
}
