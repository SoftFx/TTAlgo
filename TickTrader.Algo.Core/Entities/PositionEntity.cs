using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PositionEntity
    {
        public PositionEntity(string symbol)
        {
            Symbol = symbol;
        }

        public PositionEntity(PositionExecReport report)
            : this(report.PositionInfo.Symbol)
        {
            Update(report.PositionInfo);
        }

        public PositionEntity(PositionEntity src)
            : this(src.Symbol)
        {
            Update(src);
        }

        public void Update(PositionExecReport report)
        {
            Update(report.PositionInfo);
        }

        public void Update(PositionEntity src)
        {
            Symbol = src.Symbol;
            Volume = src.Volume;
            Commission = src.Commission;
            Price = src.Price;
            SettlementPrice = src.Price;
            Side = src.Side;
            Swap = src.Swap;
            Modified = src.Modified;
        }

        public PositionEntity Clone()
        {
            return new PositionEntity(this);
        }

        public string Id { get; set; }
        public double AgentCommission { get; set; }
        public double Volume { get; set; }
        public double Commission { get; set; }
        public double Price { get; set; }
        public double SettlementPrice { get; set; }
        public OrderSide Side { get; set; }
        public double Swap { get; set; }
        public string Symbol { get; private set; }
        public DateTime? Modified { get; set; }
        public bool IsEmpty => Volume == 0;
    }
}
