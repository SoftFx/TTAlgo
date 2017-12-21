using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessLogic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public class PositionModel : ObservableObject, TickTrader.BusinessLogic.IPositionModel
    {
        private decimal commission;
        private decimal agentCommission;
        private string symbol;
        private decimal swap;
        private double settlementPrice;
        private OrderSide side;
        private double price;
        private double amount;
        private DateTime? modified;
        private SymbolModel _symbolModel;

        public PositionModel(PositionEntity position, IOrderDependenciesResolver resolver)
        {
            Symbol = position.Symbol;
            _symbolModel = resolver.GetSymbolOrNull(position.Symbol);
            Update(position);
        }

        private void Update(PositionEntity position)
        {
            if (position.Symbol != Symbol)
                return;

            AgentCommission = (decimal)position.AgentCommission;
            Commission = (decimal)position.Commission;
            SettlementPrice = position.SettlementPrice;
            Side = position.Side;
            Amount = position.Volume;// Math.Max(position.BuyAmount, position.SellAmount);
            Swap = (decimal)position.Swap;
            Price = position.Price; //  Math.Max(position.BuyPrice ?? 0, position.SellPrice ?? 0);

            Long = new PositionSide();
            Short = new PositionSide();

            Long.Amount = position.Side == OrderSide.Buy ? (decimal)Amount : 0;
            Long.Price = position.Side == OrderSide.Buy ? (decimal)Price : 0;
            Short.Amount = position.Side == OrderSide.Sell ? (decimal)Amount : 0;
            Short.Price = position.Side == OrderSide.Sell ? (decimal)Price : 0;

            Long.ProfitUpdated = OnProfitUpdated;
            Short.ProfitUpdated = OnProfitUpdated;
            Long.MarginUpdated = OnMarginUpdated;
            Short.MarginUpdated = OnMarginUpdated;
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

        #region Properties

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
                    this.AmountLots = AmountToLots(value);
                    NotifyOfPropertyChange(nameof(Amount));
                    NotifyOfPropertyChange(nameof(AmountLots));
                }
            }
        }

        public double AmountLots { get; private set; } = 0;

        public double Price
        {
            get { return price; }
            private set
            {
                if(price != value)
                {
                    price = value;
                    NotifyOfPropertyChange(nameof(Price));
                }
            }
        }

        public OrderSide Side
        {
            get { return side;  }
            private set
            {
                if (side != value)
                {
                    side = value;
                    NotifyOfPropertyChange(nameof(Side));
                }
            }
        }

        public decimal AgentCommission
        {
            get { return agentCommission; }
            private set
            {
                if (agentCommission != value)
                {
                    agentCommission = value;
                    NotifyOfPropertyChange(nameof(AgentCommission));
                }
            }
        }

        public double SettlementPrice
        {
            get { return settlementPrice; }
            private set
            {
                if (settlementPrice != value)
                {
                    settlementPrice = value;
                    NotifyOfPropertyChange(nameof(SettlementPrice));
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

        public decimal Profit { get { return Long.Profit + Short.Profit; } }
        public decimal? NetProfit => Profit + Commission + Swap;
                    
        public decimal Margin { get { return Long.Margin + Short.Margin; } }
        public OrderCalculator Calculator { get; set; }
        public PositionSide Long { get; private set; }
        public PositionSide Short { get; private set; }
        IPositionSide IPositionModel.Long { get { return Long; } }
        IPositionSide IPositionModel.Short { get { return Short; } }

        #endregion

        private double AmountToLots(double volume)
        {
            if (_symbolModel == null)
                return double.NaN;

            return volume / _symbolModel.LotSize;
        }

        public class PositionSide : IPositionSide
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

        internal PositionExecReport ToReport(OrderExecAction action)
        {
            return new PositionExecReport()
            {
                ExecAction = action,
                PositionInfo = ToAlgoPosition()
            };
        }

        internal PositionEntity ToAlgoPosition()
        {
            return new PositionEntity
            {
                Symbol = this.Symbol,
                AgentCommission = (double)this.AgentCommission,
                Commission = (double)this.Commission,
                SettlementPrice = this.SettlementPrice,
                Side = this.Side,
                Volume = Amount,
                Swap = (double)this.Swap,
                Price = (double)this.Price
            };
        }
    }
}
