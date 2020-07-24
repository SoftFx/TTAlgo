using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class PositionModel : IPositionModel2
    {
        public PositionModel(Domain.PositionInfo position, IOrderDependenciesResolver resolver)
        {
            Symbol = position.Symbol;
            SymbolModel = resolver.GetSymbolOrNull(position.Symbol);

            Update(position);
        }

        private void Update(Domain.PositionInfo position)
        {
            if (position.Symbol != Symbol)
                return;

            Id = position.Id;
            Commission = (decimal)position.Commission;
            Side = position.Side;
            Amount = position.Volume;
            Swap = (decimal)position.Swap;
            Price = position.Price;
            Modified = position.Modified?.ToDateTime();

            Long = new PositionSide(IsBuy ? (decimal)Amount : 0, IsBuy ? (decimal)Price : 0);
            Short = new PositionSide(!IsBuy ? (decimal)Amount : 0, !IsBuy ? (decimal)Price : 0);
        }

        public string Id { get; private set; }

        public bool IsBuy => Side == Domain.OrderInfo.Types.Side.Buy;

        public SymbolInfo SymbolModel { get; private set; }

        public decimal Commission { get; private set; }

        public decimal Swap { get; private set; }

        public string Symbol { get; private set; }

        public double Amount { get; private set; }

        public double Price { get; private set; }

        public Domain.OrderInfo.Types.Side Side { get; private set; }

        public DateTime? Modified { get; private set; }

        public Core.Calc.OrderCalculator Calculator { get; set; }

        public IPositionSide2 Long { get; private set; }

        public IPositionSide2 Short { get; private set; }

        internal Domain.PositionExecReport ToReport(Domain.OrderExecReport.Types.ExecAction action)
        {
            var info = GetInfo();

            return new Domain.PositionExecReport()
            {
                PositionCopy = info,
                ExecAction = action,
            };
        }

        internal Domain.PositionInfo GetInfo()
        {
            return new Domain.PositionInfo
            {
                Symbol = Symbol,
                Commission = (double)Commission,
                Side = Side,
                Volume = Amount,
                Swap = (double)Swap,
                Price = Price,
                Id = Id,
                Modified = Modified?.ToTimestamp(),
            };
        }

        public class PositionSide : IPositionSide2
        {
            private decimal margin;
            private decimal profit;

            public decimal Amount { get; set; }
            public decimal Price { get; set; }

            public PositionSide(decimal amount, decimal price)
            {
                Amount = amount;
                Price = price;
            }

            public decimal Margin
            {
                get { return margin; }
                set
                {
                    if (margin != value)
                    {
                        margin = value;
                        MarginUpdated?.Invoke();
                    }
                }
            }

            public decimal Profit
            {
                get { return profit; }
                set
                {
                    if (profit != value)
                    {
                        profit = value;
                        ProfitUpdated?.Invoke();
                    }
                }
            }

            public System.Action ProfitUpdated;
            public System.Action MarginUpdated;
        }
    }
}
