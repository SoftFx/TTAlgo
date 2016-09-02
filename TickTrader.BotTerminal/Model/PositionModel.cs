using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class PositionModel : PropertyChangedBase
    {
        private double commission;
        private double agentCommission;
        private string symbol;
        private double buyAmount;
        private double? buyPrice;
        private double? sellPrice;
        private double sellAmount;
        private double settlementPrice;
        private TradeRecordSide side;
        private double price;
        private double amount;

        public PositionModel(Position position)
        {
            Symbol = position.Symbol;
            Update(position);
        }

        private void Update(Position position)
        {
            if (position.Symbol != Symbol)
                return;

            SellPrice = position.SellPrice;
            SellAmount = position.SellAmount;
            BuyPrice = position.BuyPrice;
            BuyAmount = position.BuyAmount;
            AgentCommission = position.AgentCommission;
            Commission = position.Commission;
            SettlementPrice = position.SettlementPrice;
            Side = position.BuyAmount > 0 ? TradeRecordSide.Buy : TradeRecordSide.Sell;
            Amount = Math.Max(position.BuyAmount, position.SellAmount);
            Price = Math.Max(position.BuyPrice ?? 0, position.SellPrice ?? 0);
        }

        public double Commission
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
                if (amount != value)
                {
                    amount = value;
                    NotifyOfPropertyChange(nameof(Amount));
                }
            }
        }

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

        public TradeRecordSide Side
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

        public double BuyAmount
        {
            get { return buyAmount; }
            private set
            {
                if (buyAmount != value)
                {
                    buyAmount = value;
                    NotifyOfPropertyChange(nameof(BuyAmount));
                }
            }
        }

        public double? BuyPrice
        {
            get { return buyPrice; }
            private set
            {
                if (buyPrice != value)
                {
                    buyPrice = value;
                    NotifyOfPropertyChange(nameof(BuyPrice));
                }
            }
        }

        public double SellAmount
        {
            get { return sellAmount; }
            private set
            {
                if (sellAmount != value)
                {
                    sellAmount = value;
                    NotifyOfPropertyChange(nameof(SellAmount));
                }
            }
        }

        public double? SellPrice
        {
            get { return sellPrice; }
            private set
            {
                if (sellPrice != value)
                {
                    sellPrice = value;
                    NotifyOfPropertyChange(nameof(SellPrice));
                }
            }
        }

        public double AgentCommission
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
    }
}
