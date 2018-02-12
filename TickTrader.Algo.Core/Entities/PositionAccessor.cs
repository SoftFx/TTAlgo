using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using BL = TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    public class PositionAccessor : NetPosition, BL.IPositionModel
    {
        private PositionEntity _entity;
        private SideProxy _buyProxy;
        private SideProxy _sellProxy;
        private Symbol _symbol;
        private double _lotSize;

        internal PositionAccessor(PositionEntity entity, Func<string, Symbol> symbolProvider)
            : this(entity, symbolProvider(entity.Symbol))
        {
        }

        internal PositionAccessor(PositionEntity entity, Symbol symbol)
        {
            Update(entity ?? throw new ArgumentNullException("entity"));
            Margin = 0;
            Profit = 0;

            _symbol = symbol;
            _lotSize = symbol?.ContractSize ?? 1;
        }

        internal void Update(PositionEntity entity)
        {
            _entity = entity;
            _buyProxy = new SideProxy(this, OrderSide.Buy);
            _sellProxy = new SideProxy(this, OrderSide.Sell);
        }

        internal static PositionAccessor CreateEmpty(string symbol)
        {
            return new PositionAccessor(new PositionEntity { Symbol = symbol }, (Symbol)null);
        }

        public PositionAccessor Clone()
        {
            return new PositionAccessor(_entity, _symbol);
        }

        public double AgentCommission => _entity.AgentCommission;
        public double Volume => _entity.Volume / _lotSize;
        public double Commission => _entity.Commission;
        public double Price => _entity.Price;
        public double SettlementPrice => _entity.SettlementPrice;
        public OrderSide Side => _entity.Side;
        public double Swap => _entity.Swap;
        public string Symbol => _entity.Symbol;
        public double Margin { get; set; }
        public double Profit { get; set; }
        public DateTime? Modified { get; set; }
        public bool IsEmpty => Volume == 0;

        public double VolumeUnits => _entity.Volume;

        decimal BL.IPositionModel.Commission => (decimal)Commission;
        decimal BL.IPositionModel.AgentCommission => (decimal)AgentCommission;
        decimal BL.IPositionModel.Swap => (decimal)Swap;
        public BL.IPositionSide Long => _buyProxy;
        public BL.IPositionSide Short => _sellProxy;
        BL.OrderCalculator BL.IPositionModel.Calculator { get; set; }

        private class SideProxy : BL.IPositionSide
        {
            private PositionAccessor _pos;
            private OrderSide _side;

            public SideProxy(PositionAccessor pos, OrderSide side)
            {
                _pos = pos;
                _side = side;
            }

            private bool IsEnabled => _pos.Side == _side;

            public decimal Amount => IsEnabled ? (decimal)_pos.VolumeUnits : 0;
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
