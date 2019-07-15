using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Calc;
using BL = TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    public class PositionAccessor : NetPosition, IPositionModel2
    {
        private readonly SideProxy _buy = new SideProxy();
        private readonly SideProxy _sell = new SideProxy();
        private readonly Symbol _symbol;
        private readonly double _lotSize;
        private readonly int _leverage;

        internal PositionAccessor(PositionEntity entity, int leverage, Func<string, Symbol> symbolProvider)
            : this(entity, symbolProvider(entity.Symbol), leverage)
        {
        }

        internal PositionAccessor(Symbol symbol, int leverage)
        {
            _symbol = symbol;
            _lotSize = symbol?.ContractSize ?? 1;
            _leverage = leverage;
        }

        internal PositionAccessor(PositionEntity entity, Symbol symbol, int leverage)
            : this(symbol, leverage)
        {
            Update(entity ?? throw new ArgumentNullException("entity"));
        }

        internal PositionAccessor(PositionAccessor src)
            : this(src._symbol, src._leverage)
        {
            _buy.Update(src._buy.Amount, src._buy.Price);
            _sell.Update(src._sell.Amount, src._sell.Price);

            SettlementPrice = src.SettlementPrice;
            Swap = src.Swap;
            Modified = src.Modified;
            Commission = src.Commission;
            Id = src.Id;

            Calculator = src.Calculator;
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
            Swap = entity.Swap;
            Commission = entity.Commission;
            Modified = entity.Modified;
            Id = entity.Id;
            FireChanged();
        }

        internal static PositionAccessor CreateEmpty(string symbol, Func<string, Symbol> symbolInfoProvider, int leverage)
        {
            return new PositionAccessor(new PositionEntity(symbol), leverage, symbolInfoProvider);
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
        public double Swap { get; internal set; }
        public string Symbol => _symbol.Name;
        public double Margin => CalculateMargin();
        public double Profit => CalculateProfit();
        public DateTime? Modified { get; set; }
        public string Id { get; set; }
        public bool IsEmpty => VolumeUnits.E(0);
        public OrderCalculator Calculator { get; set; }

        public double VolumeUnits => (double)Math.Max(_buy.Amount, _sell.Amount);
        public SideProxy Long => _buy;
        public SideProxy Short => _sell;

        double NetPosition.Swap => (double)Swap;

        //decimal IPositionModel.Commission => (decimal)Commission;
        //decimal IPositionModel.AgentCommission => 0;
        //decimal IPositionModel.Swap => Swap;
        IPositionSide2 IPositionModel2.Long => _buy;
        IPositionSide2 IPositionModel2.Short => _sell;

        internal event Action<PositionAccessor> Changed;

        private void FireChanged()
        {
            Changed?.Invoke(this);
        }

        #region Emulator

        internal void Increase(double amount, double price, OrderSide side)
        {
            if (side == OrderSide.Buy)
                Long.Increase(amount, price);
            else
                Short.Increase(amount, price);

            FireChanged();
        }

        internal void DecreaseBothSides(double byAmount)
        {
            Long.Decrease(byAmount);
            Short.Decrease(byAmount);
            FireChanged();
        }

        private static double CalculatePositionAvgPrice(IPositionSide2 position, double price2, double amount2)
        {
            return CalculatePositionAvgPrice(position.Price, position.Amount, price2, amount2);
        }

        private static double CalculatePositionAvgPrice(double price1, double amount1, double price2, double amount2)
        {
            // some optimization
            if (amount1 == 0)
                return price2;
            else if (amount2 == 0)
                return price1;

            return (price1 * amount1 + price2 * amount2) / (amount1 + amount2);
        }

        internal PositionEntity GetEntityCopy()
        {
            return new PositionEntity(Symbol)
            {
                Volume = VolumeUnits,
                Price = Price,
                Side = Side,
                Swap = (double)Swap,
                Commission = Commission,
                Modified = Modified,
                Id = Id,
            };
        }

        #endregion

        public class SideProxy : IPositionSide2
        {
            public SideProxy()
            {
            }

            internal void Update(double amount, double price)
            {
                Amount = amount;
                Price = price;
                Profit = 0;
                Margin = 0;
            }

            internal void Increase(double amount, double price)
            {
                Price = CalculatePositionAvgPrice(this, price, amount);
                Amount += amount;
            }

            internal void Decrease(double byAmount)
            {
                Amount -= byAmount;
            }

            public double Amount { get; private set; }
            public double Price { get; private set; }
            public double Margin { get; set; }
            public double Profit { get; set; }
        }

        private double CalculateMargin()
        {
            var calc = Calculator;
            if (calc != null)
            {
                var margin = calc.CalculateMargin(VolumeUnits, _leverage, BusinessObjects.OrderTypes.Position, Side.ToBoSide(), false, out var error);
                if (error != CalcErrorCodes.None)
                    return double.NaN;
                return margin;
            }
            return double.NaN;
        }

        private double CalculateProfit()
        {
            var calc = Calculator;
            if (calc != null)
            {
                var prof = calc.CalculateProfit(Price, VolumeUnits, Side.ToBoSide(), out _, out var error);
                if (error != CalcErrorCodes.None)
                    return double.NaN;
                return prof;
            }
            return double.NaN;
        }
    }
}
