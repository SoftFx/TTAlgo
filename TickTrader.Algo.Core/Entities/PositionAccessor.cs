using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Calc;

namespace TickTrader.Algo.Core
{
    public class PositionAccessor : NetPosition, IPositionModel2
    {
        private readonly SideProxy _buy = new SideProxy();
        private readonly SideProxy _sell = new SideProxy();
        private readonly Symbol _symbol;
        private readonly double _lotSize;
        private readonly int _leverage;
        private double _volUnitsSlim;

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

            Volume = src.Volume;
            SettlementPrice = src.SettlementPrice;
            Swap = src.Swap;
            Modified = src.Modified;
            Commission = src.Commission;
            Id = src.Id;

            Calculator = src.Calculator;
        }

        internal void Update(PositionEntity entity)
        {
            if (entity.Side == Domain.OrderInfo.Types.Side.Buy)
            {
                _buy.Update((decimal)entity.Volume, (decimal)entity.Price);
                _sell.Update(0, 0);
            }
            else
            {
                _buy.Update(0, 0);
                _sell.Update((decimal)entity.Volume, (decimal)entity.Price);
            }

            SettlementPrice = entity.SettlementPrice;
            Swap = (decimal)entity.Swap;
            Commission = (decimal)entity.Commission;
            Modified = entity.Modified;
            Id = entity.Id;
            OnChanged();
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

        public double Volume { get; private set; }
        public decimal Commission { get; internal set; }
        public double Price => (double)(IsBuySided ? _buy.Price : _sell.Price);
        public double SettlementPrice { get; internal set; }
        public Domain.OrderInfo.Types.Side Side => IsBuySided ? Domain.OrderInfo.Types.Side.Buy : Domain.OrderInfo.Types.Side.Sell;
        OrderSide NetPosition.Side => Side.ToApiEnum();
        public decimal Swap { get; internal set; }
        public string Symbol => _symbol.Name;
        public double Margin => CalculateMargin();
        public double Profit => CalculateProfit();
        public DateTime? Modified { get; set; }
        public string Id { get; set; }
        public bool IsEmpty => VolumeUnits == 0;
        public OrderCalculator Calculator { get; set; }

        public decimal VolumeUnits => Math.Max(_buy.Amount, _sell.Amount);
        public SideProxy Long => _buy;
        public SideProxy Short => _sell;

        double NetPosition.Swap => (double)Swap;
        double NetPosition.Commission => (double)Commission;

        //decimal IPositionModel.Commission => (decimal)Commission;
        //decimal IPositionModel.AgentCommission => 0;
        //decimal IPositionModel.Swap => Swap;
        IPositionSide2 IPositionModel2.Long => _buy;
        IPositionSide2 IPositionModel2.Short => _sell;

        internal event Action<PositionAccessor> Changed;

        private void OnChanged()
        {
            UpdateCache();
            Changed?.Invoke(this);
        }

        private void UpdateCache()
        {
            _volUnitsSlim = (double)VolumeUnits;
            Volume = _volUnitsSlim / _lotSize;
        }

        #region Emulator

        internal void Increase(decimal amount, decimal price, Domain.OrderInfo.Types.Side side)
        {
            if (side == Domain.OrderInfo.Types.Side.Buy)
                Long.Increase(amount, price);
            else
                Short.Increase(amount, price);

            OnChanged();
        }

        internal void DecreaseBothSides(decimal byAmount)
        {
            Long.Decrease(byAmount);
            Short.Decrease(byAmount);
            OnChanged();
        }

        private static decimal CalculatePositionAvgPrice(IPositionSide2 position, decimal price2, decimal amount2)
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

        internal PositionEntity GetEntityCopy()
        {
            return new PositionEntity(Symbol)
            {
                Volume = _volUnitsSlim,
                Price = Price,
                Side = Side,
                Swap = (double)Swap,
                Commission = (double)Commission,
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

            internal void Update(decimal amount, decimal price)
            {
                Amount = amount;
                Price = price;
                Profit = 0;
                Margin = 0;
            }

            internal void Increase(decimal amount, decimal price)
            {
                Price = CalculatePositionAvgPrice(this, price, amount);
                Amount += amount;
            }

            internal void Decrease(decimal byAmount)
            {
                Amount -= byAmount;
            }

            public decimal Amount { get; private set; }
            public decimal Price { get; private set; }
            public decimal Margin { get; set; }
            public decimal Profit { get; set; }
        }

        private double CalculateMargin()
        {
            var calc = Calculator;
            if (calc != null)
            {
                var margin = calc.CalculateMargin(_volUnitsSlim, _leverage, Domain.OrderInfo.Types.Type.Position, Side, false, out var error);
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
                var prof = calc.CalculateProfit(Price, _volUnitsSlim, Side, out _, out var error);
                if (error != CalcErrorCodes.None)
                    return double.NaN;
                return prof;
            }
            return double.NaN;
        }
    }
}
