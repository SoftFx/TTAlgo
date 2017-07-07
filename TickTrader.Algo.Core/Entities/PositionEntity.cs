using System;
using TickTrader.Algo.Api;
using BL = TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class PositionEntity : NetPosition, BL.IPositionModel
    {
        private SideProxy _buyProxy;
        private SideProxy _sellProxy;

        public PositionEntity()
        {
            _buyProxy = new SideProxy(this, OrderSide.Buy);
            _sellProxy = new SideProxy(this, OrderSide.Sell);
        }

        public PositionEntity(PositionExecReport report)
            : this()
        {
            Symbol = report.Symbol;
            Update(report);
        }

        public PositionEntity(PositionEntity src)
            : this()
        {
            Symbol = src.Symbol;
            Volume = src.Volume;
            Commission = src.Commission;
            Price = src.Price;
            SettlementPrice = src.Price;
            Side = src.Side;
            Swap = src.Swap;
        }

        public void Update(PositionExecReport report)
        {
            Volume = report.Volume;
            Commission = report.Commission;
            Price = report.Price;
            SettlementPrice = report.SettlementPrice;
            Side = report.Side;
            Swap = report.Swap;
        }

        public PositionEntity Clone()
        {
            return new PositionEntity(this);
        }

        public static PositionEntity CreateEmpty(string symbol)
        {
            return new PositionEntity() { Symbol = symbol };
        }

        public double AgentCommission { get; set; }
        public TradeVolume Volume { get; set; }
        public double Commission { get; set; }
        public double Price { get; set; }
        public double SettlementPrice { get; set; }
        public OrderSide Side { get; set; }
        public double Swap { get; set; }
        public string Symbol { get; set; }
        public double Margin { get; set; }
        public double Profit { get; set; }
        public DateTime? Modified { get; set; }

        double NetPosition.Volume => Volume.Lots;

        decimal BL.IPositionModel.Commission => (decimal)Commission;
        decimal BL.IPositionModel.AgentCommission => (decimal)AgentCommission;
        decimal BL.IPositionModel.Swap => (decimal)Swap;
        BL.IPositionSide BL.IPositionModel.Long => _buyProxy;
        BL.IPositionSide BL.IPositionModel.Short => _sellProxy;
        BL.OrderCalculator BL.IPositionModel.Calculator { get; set; }

        private class SideProxy : BL.IPositionSide
        {
            private PositionEntity _pos;
            private OrderSide _side;

            public SideProxy(PositionEntity pos, OrderSide side)
            {
                _pos = pos;
                _side = side;
            }

            private bool IsEnabled => _pos.Side == _side;

            public decimal Amount => IsEnabled ? (decimal)_pos.Volume.Units : 0;
            public decimal Price => IsEnabled ? (decimal)_pos.Price : 0;
            public decimal Margin
            {
                get => IsEnabled ? (decimal)_pos.Margin : 0;
                set { if (IsEnabled) _pos.Margin = (double)value; }
            }
            public decimal Profit
            {
                get => IsEnabled ? (decimal)_pos.Profit : 0;
                set { if (IsEnabled) _pos.Profit = (double)value; }
            }
        }
    }
}
