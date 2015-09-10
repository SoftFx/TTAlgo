using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class OrderModel : PropertyChangedBase
    {
        public OrderModel(TradeRecord record)
        {
            this.Id = record.OrderId;
            this.Symbol = record.Symbol;
            Update(record);
        }

        public OrderModel(ExecutionReport report)
        {
            this.Id = report.OrderId;
            this.Symbol = report.Symbol;
            Update(report);
        }
        
        public void Update(TradeRecord record)
        {
            this.Amount = (decimal)record.InitialVolume;
            this.RemainingAmount = (decimal)record.Volume;
            this.OrderType = record.Type;
            this.Side = record.Side;
            this.Price = (decimal)record.Price;
        }

        public void Update(ExecutionReport report)
        {
            this.Amount = (decimal)report.InitialVolume;
            this.RemainingAmount = (decimal)report.LeavesVolume;
            this.OrderType = report.OrderType;
            this.Side = report.OrderSide;
            this.Price = (decimal)report.Price;
        }

        #region Order Properties 

        private TradeRecordType orderType;
        private decimal amount;
        private decimal amountRemaining;
        public TradeRecordSide side;
        private decimal price;

        public string Id { get; private set; }
        public string Symbol { get; private set; }
        
        public decimal Amount
        {
            get { return amount; }
            set
            {
                if (this.amount != value)
                {
                    this.amount = value;
                    NotifyOfPropertyChange("Amount");
                }
            }
        }

        public decimal RemainingAmount
        {
            get { return amountRemaining; }
            set
            {
                if (this.amountRemaining != value)
                {
                    this.amountRemaining = value;
                    NotifyOfPropertyChange("RemainingAmount");
                }
            }
        }

        public TradeRecordType OrderType
        {
            get { return orderType; }
            set
            {
                if (orderType != value)
                {
                    this.orderType = value;
                    NotifyOfPropertyChange("OrderType");
                }
            }
        }

        public TradeRecordSide Side
        {
            get { return side; }
            set
            {
                if (side != value)
                {
                    side = value;
                    NotifyOfPropertyChange("Side");
                }
            }
        }

        public decimal Price
        {
            get { return price; }
            set
            {
                if (price != value)
                {
                    price = value;
                    NotifyOfPropertyChange("Price");
                }
            }
        }

        #endregion
    }
}
