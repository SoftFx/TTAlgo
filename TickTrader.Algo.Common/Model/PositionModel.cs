using Google.Protobuf.WellKnownTypes;
using System;
using System.Globalization;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Calc;

namespace TickTrader.Algo.Common.Model
{
    public class PositionModel : ObservableObject, IPositionModel2/* TickTrader.BusinessLogic.IPositionModel*/
    {
        private static readonly string DefaultVolumeFormat = $"0.{new string('#', 15)}";

        private decimal commission;
        private decimal agentCommission;
        private string symbol;
        private decimal swap;
        private double settlementPrice;
        private Domain.OrderInfo.Types.Side side;
        private double price;
        private double amount;
        private DateTime? modified;

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
            Amount = position.Volume;// Math.Max(position.BuyAmount, position.SellAmount);
            Swap = (decimal)position.Swap;
            Price = position.Price; //  Math.Max(position.BuyPrice ?? 0, position.SellPrice ?? 0);
            Modified = position.Modified?.ToDateTime();

            Long = new PositionSide();
            Short = new PositionSide();

            Long.Amount = position.Side == Domain.OrderInfo.Types.Side.Buy ? (decimal)Amount : 0;
            Long.Price = position.Side == Domain.OrderInfo.Types.Side.Buy ? (decimal)Price : 0;
            Short.Amount = position.Side == Domain.OrderInfo.Types.Side.Sell ? (decimal)Amount : 0;
            Short.Price = position.Side == Domain.OrderInfo.Types.Side.Sell ? (decimal)Price : 0;

            Long.ProfitUpdated = OnProfitUpdated;
            Short.ProfitUpdated = OnProfitUpdated;
            Long.MarginUpdated = OnMarginUpdated;
            Short.MarginUpdated = OnMarginUpdated;

            SymbolModel.RateUpdated += OnDeviationPriceUpdate;
        }

        private void OnProfitUpdated()
        {
            NotifyOfPropertyChange(nameof(Profit));
            NotifyOfPropertyChange(nameof(NetProfit));
        }

        private void OnMarginUpdated()
        {
            NotifyOfPropertyChange(nameof(Margin));
        }

        private void OnDeviationPriceUpdate(SymbolModel symbol)
        {
            NotifyOfPropertyChange(nameof(DeviationPrice));
        }
        #region Properties

        public string Id { get; private set; }

        public SymbolModel SymbolModel { get; private set; }

        public decimal Commission
        {
            get { return commission; }
            private set
            {
                if (commission != value)
                {
                    commission = value;
                    NotifyOfPropertyChange(nameof(Commission));
                }
            }
        }

        public decimal Swap
        {
            get { return swap; }
            private set
            {
                if (swap != value)
                {
                    swap = value;
                    NotifyOfPropertyChange(nameof(Swap));
                }
            }
        }

        public string Symbol
        {
            get { return symbol; }
            private set
            {
                if (symbol != value)
                {
                    symbol = value;
                    NotifyOfPropertyChange(nameof(Symbol));
                }
            }
        }

        public double Amount
        {
            get { return amount; }
            private set
            {
                if (this.amount != value)
                {
                    this.amount = value;
                    NotifyOfPropertyChange(nameof(Amount));
                    NotifyOfPropertyChange(nameof(AmountLots));
                }
            }
        }

        public string AmountLots => SymbolModel == null ? "" : (Amount / SymbolModel.LotSize).ToString(DefaultVolumeFormat, CultureInfo.InvariantCulture);

        public double Price
        {
            get { return price; }
            private set
            {
                if (price != value)
                {
                    price = value;
                    NotifyOfPropertyChange(nameof(Price));
                }
            }
        }

        public Domain.OrderInfo.Types.Side Side
        {
            get { return side; }
            private set
            {
                if (side != value)
                {
                    side = value;
                    NotifyOfPropertyChange(nameof(Side));
                }
            }
        }

        public DateTime? Modified
        {
            get { return modified; }
            set
            {
                if (modified != value)
                {
                    modified = value;
                    NotifyOfPropertyChange(nameof(Modified));
                }
            }
        }

        public double DeviationPrice
        {
            get
            {
                double sPrice = (Side == Domain.OrderInfo.Types.Side.Buy ? SymbolModel.CurrentBid : SymbolModel.CurrentAsk) ?? 0;
                return Side == Domain.OrderInfo.Types.Side.Buy ? sPrice - Price : Price - sPrice;
            }
        }

        public decimal Profit { get { return Long.Profit + Short.Profit; } }
        public decimal? NetProfit => Profit + Commission + Swap;
        public decimal Margin { get { return Long.Margin + Short.Margin; } }
        public Core.Calc.OrderCalculator Calculator { get; set; }
        //public PositionSide Long { get; private set; }
        //public PositionSide Short { get; private set; }

        //Core.Calc.OrderCalculator IPositionModel2.Calculator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PositionSide Long { get; private set; }
        public PositionSide Short { get; private set; }

        IPositionSide2 IPositionModel2.Long => Long;

        IPositionSide2 IPositionModel2.Short => Short;

        #endregion

        public class PositionSide : IPositionSide2/*IPositionSide*/
        {
            private decimal margin;
            private decimal profit;

            public decimal Amount { get; set; }
            public decimal Price { get; set; }

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
                Price = (double)Price,
                Id = Id,
                Modified = Modified?.ToTimestamp(),
            };
        }
    }
}
