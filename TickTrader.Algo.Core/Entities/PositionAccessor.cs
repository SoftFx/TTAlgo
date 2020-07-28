using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class PositionAccessor : NetPosition, IPositionInfo
    {
        private readonly Symbol _symbol;
        //private readonly double _lotSize;
        private readonly int _leverage;
        //private double _volUnitsSlim;

        internal PositionAccessor(Domain.PositionInfo info, int leverage, Func<string, Symbol> symbolProvider)
            : this(info, symbolProvider(info.Symbol), leverage)
        {
        }

        internal PositionAccessor(Symbol symbol, int leverage)
        {
            _symbol = symbol;
            //_lotSize = symbol?.ContractSize ?? 1;
            _leverage = leverage;
        }

        internal PositionAccessor(Domain.PositionInfo info, Symbol symbol, int leverage)
            : this(symbol, leverage)
        {
            Update(info ?? throw new ArgumentNullException("entity"));
        }

        internal PositionAccessor(PositionAccessor src)
            : this(src._symbol, src._leverage)
        {
            Long.Update(src.Long.Amount, src.Long.Price);
            Short.Update(src.Short.Amount, src.Short.Price);

            Volume = src.Volume;
            Swap = src.Swap;
            Modified = src.Modified;
            Commission = src.Commission;
            Id = src.Id;

            Calculator = src.Calculator;
        }

        internal void Update(Domain.PositionInfo info)
        {
            if (info.Side == Domain.OrderInfo.Types.Side.Buy)
            {
                Long.Update((decimal)info.Volume, (decimal)info.Price);
                Short.Update(0, 0);
            }
            else
            {
                Long.Update(0, 0);
                Short.Update((decimal)info.Volume, (decimal)info.Price);
            }

            Swap = (decimal)info.Swap;
            Commission = (decimal)info.Commission;
            Modified = info.Modified?.ToDateTime();
            Id = info.Id;
            OnChanged();
        }

        internal static PositionAccessor CreateEmpty(string symbol, Func<string, Symbol> symbolInfoProvider, int leverage)
        {
            return new PositionAccessor(new Domain.PositionInfo { Symbol = symbol }, leverage, symbolInfoProvider);
        }

        public PositionAccessor Clone()
        {
            return new PositionAccessor(this);
        }

        internal bool IsBuySided => Long.Amount > Short.Amount;

        public double Volume { get; private set; }
        public decimal Commission { get; internal set; }
        public double Price => (double)(IsBuySided ? Long.Price : Short.Price);
        public double SettlementPrice { get; internal set; }
        public Domain.OrderInfo.Types.Side Side => IsBuySided ? Domain.OrderInfo.Types.Side.Buy : Domain.OrderInfo.Types.Side.Sell;
        OrderSide NetPosition.Side => Side.ToApiEnum();
        public decimal Swap { get; internal set; }
        public string Symbol => _symbol.Name;
        public double Margin => Calculator?.CalculateMargin(this) ?? double.NaN;
        public double Profit => Calculator?.CalculateProfit(this) ?? double.NaN;
        public DateTime? Modified { get; set; }
        public string Id { get; set; }
        public bool IsEmpty => Amount == 0;
        public IOrderCalculator Calculator { get; set; }

        public decimal Amount => Math.Max(Long.Amount, Short.Amount);
        public SideProxy Long { get; } = new SideProxy();
        public SideProxy Short { get; } = new SideProxy();

        double NetPosition.Swap => (double)Swap;
        double NetPosition.Commission => (double)Commission;

        IPositionSide IPositionInfo.Long => Long;
        IPositionSide IPositionInfo.Short => Short;

        decimal IMarginProfitCalc.RemainingAmount => Amount;

        public OrderInfo.Types.Type Type => OrderInfo.Types.Type.Position;

        bool IMarginProfitCalc.IsHidden => false;

        internal event Action<PositionAccessor> Changed;

        private void OnChanged()
        {
            UpdateCache();
            Changed?.Invoke(this);
        }

        private void UpdateCache()
        {
            Volume = (double)Amount / (_symbol?.ContractSize ?? 1);
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

        private static decimal CalculatePositionAvgPrice(IPositionSide position, decimal price2, decimal amount2)
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

        internal Domain.PositionInfo GetEntityCopy()
        {
            return new Domain.PositionInfo
            {
                Symbol = Symbol,
                Volume = (double)Amount,
                Price = Price,
                Side = Side,
                Swap = (double)Swap,
                Commission = (double)Commission,
                Modified = Modified?.ToTimestamp(),
                Id = Id,
            };
        }

        #endregion

        public class SideProxy : IPositionSide
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
    }
}
