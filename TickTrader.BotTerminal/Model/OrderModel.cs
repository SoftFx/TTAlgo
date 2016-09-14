using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class OrderModel : PropertyChangedBase
    {
        private TradeRecordType orderType;
        private double amount;
        private double amountRemaining;
        public TradeRecordSide side;
        private double price;
        private double swap;
        private double commission;
        private DateTime? created;
        private DateTime? expiration;
        private string comment;
        private double? stopLoss;
        private double? takeProfit;

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

        #region Order Properties 
        public string Id { get; private set; }
        public string Symbol { get; private set; }
        public double Amount
        {
            get { return amount; }
            private set
            {
                if (this.amount != value)
                {
                    this.amount = value;
                    NotifyOfPropertyChange(nameof(Amount));
                }
            }
        }
        public double RemainingAmount
        {
            get { return amountRemaining; }
            private set
            {
                if (this.amountRemaining != value)
                {
                    this.amountRemaining = value;
                    NotifyOfPropertyChange(nameof(RemainingAmount));
                }
            }
        }
        public TradeRecordType OrderType
        {
            get { return orderType; }
            private set
            {
                if (orderType != value)
                {
                    this.orderType = value;
                    NotifyOfPropertyChange(nameof(OrderType));
                }
            }
        }
        public TradeRecordSide Side
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
        public double Swap
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
        public DateTime? Created
        {
            get { return created; }
            private set
            {
                if (created != value)
                {
                    created = value;
                    NotifyOfPropertyChange(nameof(Created));
                }
            }
        }
        public DateTime? Expiration
        {
            get { return expiration; }
            private set
            {
                if (expiration != value)
                {
                    expiration = value;
                    NotifyOfPropertyChange(nameof(Expiration));
                }
            }
        }
        public string Comment
        {
            get { return comment; }
            private set
            {
                if (comment != value)
                {
                    comment = value;
                    NotifyOfPropertyChange(nameof(Comment));
                }
            }
        }
        public double? TakeProfit
        {
            get { return takeProfit; }
            private set
            {
                if (takeProfit != value)
                {
                    takeProfit = value;
                    NotifyOfPropertyChange(nameof(TakeProfit));
                }
            }
        }
        public double? StopLoss
        {
            get { return stopLoss; }
            private set
            {
                if (stopLoss != value)
                {
                    stopLoss = value;
                    NotifyOfPropertyChange(nameof(StopLoss));
                }
            }
        }
        #endregion

        public OrderEntity ToAlgoOrder()
        {
            return new OrderEntity(Id)
            {
                RemainingAmount = RemainingAmount,
                RequestedAmount = Amount,
                Symbol = Symbol,
                Type = FdkToAlgo.Convert(orderType),
                Side = FdkToAlgo.Convert(Side),
                Price = Price
            };
        }

        private void Update(TradeRecord record)
        {
            this.Amount = record.InitialVolume;
            this.RemainingAmount = record.Volume;
            this.OrderType = record.Type;
            this.Side = record.Side;
            this.Price = record.Price;
            this.Created = record.Created;
            this.Expiration = record.Expiration;
            this.Comment = record.Comment;
            this.StopLoss = record.StopLoss;
            this.TakeProfit = record.TakeProfit;
            this.Swap = record.Swap;
            this.Commission = record.Commission;
        }
        private void Update(ExecutionReport report)
        {
            this.Amount = report.InitialVolume ?? 0;
            this.RemainingAmount = report.LeavesVolume;
            this.OrderType = report.OrderType;
            this.Side = report.OrderSide;
            this.Price = (report.Price ?? report.StopPrice) ?? 0;
            this.Created = report.Created;
            this.Expiration = report.Expiration;
            this.Comment = report.Comment;
            this.StopLoss = report.StopLoss;
            this.TakeProfit = report.TakeProfit;
            this.Swap = report.Swap;
            this.Commission = report.Commission;
        }
    }
}
