using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PositionEntity
    {
        public PositionEntity()
        {
        }

        public double AgentCommission { get; set; }
        public double Amount { get; set; }
        public double Commission { get; set; }
        public double Price { get; set; }
        public double SettlementPrice { get; set; }
        public OrderSide Side { get; set; }
        public double Swap { get; set; }
        public string Symbol { get; set; }
    }
}
