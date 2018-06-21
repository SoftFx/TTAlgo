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
        private SideProxy _buy;
        private SideProxy _sell;
        private Symbol _symbol;
        private double _lotSize;

        internal PositionAccessor(PositionEntity entity, Func<string, Symbol> symbolProvider)
            : this(entity, symbolProvider(entity.Symbol))
        {
        }

        internal PositionAccessor(Symbol symbol)
        {
            _buy = new SideProxy(this);
            _sell = new SideProxy(this);
            _symbol = symbol;
            _lotSize = symbol?.ContractSize ?? 1;
        }

        internal PositionAccessor(PositionEntity entity, Symbol symbol)
            : this(symbol)
        {
            Update(entity ?? throw new ArgumentNullException("entity"));
        }

        internal PositionAccessor(PositionAccessor src)
            : this(src._symbol)
        {
            _buy.Price = src._buy.Price;
            _buy.Amount = src._buy.Amount;

            _sell.Price = src._sell.Price;
            _sell.Amount = src._sell.Amount;

            SettlementPrice = src.SettlementPrice;
            Swap = src.Swap;
            Modified = src.Modified;
            Commission = src.Commission;
        }

        internal void Update(PositionEntity entity)
        {
            if (entity.Side == OrderSide.Buy)
            {
                _buy.Update(entity.Volume, entity.Price);
                _sell.Update(0, 0);
            }
            else
            {
                _buy.Update(0, 0);
                _sell.Update(entity.Volume, entity.Price);
            }

            SettlementPrice = entity.SettlementPrice;
            Swap = (decimal)entity.Swap;
            Commission = entity.Commission;
            Modified = entity.Modified;

            FireChanged();
        }

        internal void Remove()
        {
            Removed?.Invoke(this);
        }

        internal static PositionAccessor CreateEmpty(string symbol)
        {
            return new PositionAccessor(new PositionEntity { Symbol = symbol }, (Symbol)null);
        }

        public PositionAccessor Clone()
        {
            return new PositionAccessor(this);
        }

        internal bool IsBuySided => _buy.Amount > _sell.Amount;

        public double Volume => VolumeUnits / _lotSize;
        public double Commission { get; internal set; }
        public double Price => (double)(IsBuySided? _buy.Price : _sell.Price);
        public double SettlementPrice { get; internal set; }
        public OrderSide Side => IsBuySided ? OrderSide.Buy : OrderSide.Sell;
        public decimal Swap { get; internal set; }
        public string Symbol { get; }
        public double Margin => (double)(IsBuySided ? _buy.Margin : _sell.Margin);
        public double Profit => (double)(IsBuySided ? _buy.Profit : _sell.Profit);
        public DateTime? Modified { get; set; }
        public bool IsEmpty => VolumeUnits == 0;
        public BL.OrderCalculator Calculator { get; set; }

        public double VolumeUnits => (double)Math.Max(_buy.Amount, _sell.Amount);
        public SideProxy Long => _buy;
        public SideProxy Short => _sell;

        double NetPosition.Swap => (double)Swap;

        decimal BL.IPositionModel.Commission => (decimal)Commission;
        decimal BL.IPositionModel.AgentCommission => 0;
        decimal BL.IPositionModel.Swap => Swap;
        BL.IPositionSide BL.IPositionModel.Long => _buy;
        BL.IPositionSide BL.IPositionModel.Short => _sell;

        internal event Action<PositionAccessor> Changed;
        internal event Action<PositionAccessor> Removed;

        private void FireChanged()
        {
            Changed?.Invoke(this);
        }

        #region Emulator

        internal void Increase(decimal amount, decimal price, OrderSide side)
        {
            if (side == OrderSide.Buy)
            {
                Long.Price = CalculatePositionAvgPrice(Long, price, amount);
                Long.Amount += amount;
            }
            else
            {
                Short.Price = CalculatePositionAvgPrice(Short, price, amount);
                Short.Amount += amount;
            }
        }

        private static decimal CalculatePositionAvgPrice(BL.IPositionSide position, decimal price2, decimal amount2)
        {
            return CalculatePositionAvgPrice(position.Price, position.Amount, price2, amount2);
        }

        private static decimal CalculatePositionAvgPrice(decimal price1, decimal amount1, decimal price2, decimal amount2)
        {
            // some optimization
            if (amount1 == 0)
                return price2;
            else if (amount2 == 0)
                return price1;

            return (price1 * amount1 + price2 * amount2) / (amount1 + amount2);
        }

        #endregion

        public class SideProxy : BL.IPositionSide
        {
            private PositionAccessor _parent;

            public SideProxy(PositionAccessor parent)
            {
                _parent = parent;
            }

            public void Update(double amount, double price)
            {
                Amount = (decimal)amount;
                Price = (decimal)price;
                Profit = 0;
                Margin = 0;

                _parent.FireChanged();
            }

            public decimal Amount { get; internal set; }
            public decimal Price { get; internal set; }
            public decimal Margin { get; set; }
            public decimal Profit { get; set; }
        }
    }
}
