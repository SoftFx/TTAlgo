using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.CoreV1
{
    public sealed class PositionAccessor : NetPosition
    {
        private readonly string _symbolName;
        private readonly double _lotSize;

        internal PositionAccessor(string symbolName, Symbol symbol, PositionInfo info = null)
            : this(symbolName, symbol?.ContractSize, info)
        {
        }

        private PositionAccessor(string symbolName, double? lotSize, PositionInfo info = null)
        {
            _symbolName = symbolName;
            _lotSize = lotSize ?? 1;

            Info = info ?? new PositionInfo() { Symbol = symbolName };
            Info.Short = Short;
            Info.Long = Long;

            if (info != null)
                Update(Info);
        }

        internal void Update(PositionInfo info)
        {
            if (info.Side == OrderInfo.Types.Side.Buy)
            {
                Long.Update(info.Volume, info.Price);
                Short.Update(0, 0);
            }
            else
            {
                Long.Update(0, 0);
                Short.Update(info.Volume, info.Price);
            }

            Changed?.Invoke(this);
        }

        public PositionAccessor Clone() => new PositionAccessor(_symbolName, _lotSize, Info);

        public double Price => (double)(Side == OrderInfo.Types.Side.Buy ? Long.Price : Short.Price);

        public OrderInfo.Types.Side Side => Long.Amount > Short.Amount ? OrderInfo.Types.Side.Buy : OrderInfo.Types.Side.Sell;

        public string Id => Info.Id;
        public bool IsEmpty => Amount == 0;
        public string Symbol => _symbolName;
        public double Amount => Math.Max(Long.Amount, Short.Amount);

        public PositionInfo Info { get; }

        public SideProxy Long { get; } = new SideProxy();
        public SideProxy Short { get; } = new SideProxy();

        double NetPosition.Volume => (double)Amount / _lotSize;
        double NetPosition.Margin => ProcessResponse(Info.Calculator?.Margin?.Calculate(Info));
        double NetPosition.Profit => ProcessResponse(Info.Calculator?.Profit.Calculate(Info));
        double NetPosition.Swap => (double)Info.Swap;
        double NetPosition.Commission => (double)Info.Commission;
        OrderSide NetPosition.Side => Side.ToApiEnum();
        DateTime? NetPosition.Modified => Info.Modified?.ToDateTime();

        internal event Action<PositionAccessor> Changed;

        private static double ProcessResponse(ICalculateResponse<double> response)
        {
            return response != null && response.IsCompleted ? response.Value : double.NaN;
        }

        #region Emulator

        internal void Increase(double amount, double price, OrderInfo.Types.Side side)
        {
            if (side == OrderInfo.Types.Side.Buy)
                Long.Increase(amount, price);
            else
                Short.Increase(amount, price);

            Changed?.Invoke(this);
        }

        internal void DecreaseBothSides(double byAmount)
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

            private static double CalculatePositionAvgPrice(IPositionSide side, double price2, double amount2)
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
