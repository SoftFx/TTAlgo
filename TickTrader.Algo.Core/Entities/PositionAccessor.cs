using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class PositionAccessor : NetPosition
    {
        private readonly Symbol _symbol;

        internal PositionAccessor(Symbol symbol, Domain.PositionInfo info = null)
        {
            _symbol = symbol;

            Info = info ?? new PositionInfo() { Symbol = symbol.Name };
            Info.Short = Short;
            Info.Long = Long;

            if (info != null)
                Update(Info);
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

            Changed?.Invoke(this);
        }

        public PositionAccessor Clone() => new PositionAccessor(_symbol, Info);

        public double Price => (double)(Side == OrderInfo.Types.Side.Buy ? Long.Price : Short.Price);

        public OrderInfo.Types.Side Side => Long.Amount > Short.Amount ? OrderInfo.Types.Side.Buy : OrderInfo.Types.Side.Sell;

        public string Id => Info.Id;
        public bool IsEmpty => Amount == 0;
        public string Symbol => _symbol.Name;
        public decimal Amount => Math.Max(Long.Amount, Short.Amount);

        public PositionInfo Info { get; }

        public SideProxy Long { get; } = new SideProxy();
        public SideProxy Short { get; } = new SideProxy();

        double NetPosition.Volume => (double)Amount / (_symbol?.ContractSize ?? 1);
        double NetPosition.Margin => Info?.Calculator?.CalculateMargin(Info) ?? double.NaN;
        double NetPosition.Profit => Info?.Calculator?.CalculateProfit(Info) ?? double.NaN;
        double NetPosition.Swap => (double)Info.Swap;
        double NetPosition.Commission => (double)Info.Commission;
        OrderSide NetPosition.Side => Side.ToApiEnum();
        DateTime? NetPosition.Modified => Info.Modified?.ToDateTime();

        internal event Action<PositionAccessor> Changed;

        #region Emulator

        internal void Increase(decimal amount, decimal price, Domain.OrderInfo.Types.Side side)
        {
            if (side == Domain.OrderInfo.Types.Side.Buy)
                Long.Increase(amount, price);
            else
                Short.Increase(amount, price);

            Changed?.Invoke(this);
        }

        internal void DecreaseBothSides(decimal byAmount)
        {
            Long.Decrease(byAmount);
            Short.Decrease(byAmount);

            Changed?.Invoke(this);
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

            private static decimal CalculatePositionAvgPrice(IPositionSide side, decimal price2, decimal amount2)
            {
                // some optimization
                if (side.Amount == 0)
                    return price2;
                else if (amount2 == 0)
                    return side.Price;

                return (side.Price * side.Amount + price2 * amount2) / (side.Amount + amount2);
            }
        }
    }
}
