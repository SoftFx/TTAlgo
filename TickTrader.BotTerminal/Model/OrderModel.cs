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
    internal class OrderModel : PropertyChangedBase, IDisposable
    {
        private SymbolObserver symbolObserver;

        public OrderModel(TradeRecord record) : this(record, null) { }
        public OrderModel(ExecutionReport report): this(report, null) { }

        public OrderModel(TradeRecord record, SymbolModel symbolModel)
        {
            if(symbolModel != null)
                this.symbolObserver = new SymbolObserver(symbolModel);
            this.Id = record.OrderId;
            this.Symbol = record.Symbol;
            Update(record);
        }

        public OrderModel(ExecutionReport report, SymbolModel symbolModel)
        {
            if (symbolModel != null)
                this.symbolObserver = new SymbolObserver(symbolModel);
            this.Id = report.OrderId;
            this.Symbol = report.Symbol;

            Update(report);
        }

        private void Update(TradeRecord record)
        {
            this.Amount = (decimal)record.InitialVolume;
            this.RemainingAmount = (decimal)record.Volume;
            this.OrderType = record.Type;
            this.Side = record.Side;
            this.Price = (decimal)record.Price;
            this.Created = record.Created;
            this.Expiration = record.Expiration;
            this.Comment = record.Comment;
            this.StopLoss = (decimal?)record.StopLoss;
            this.TakeProfit = (decimal?)record.TakeProfit;
        }

        private void Update(ExecutionReport report)
        {
            this.Amount = (decimal)report.InitialVolume;
            this.RemainingAmount = (decimal)report.LeavesVolume;
            this.OrderType = report.OrderType;
            this.Side = report.OrderSide;
            this.Price = (decimal?)(report.Price ?? report.StopPrice);
            this.Created = report.Created;
            this.Expiration = report.Expiration;
            this.Comment = report.Comment;
            this.StopLoss = (decimal?)report.StopLoss;
            this.TakeProfit = (decimal?)report.TakeProfit;
        }

        public OrderEntity ToAlgoOrder()
        {
            return new OrderEntity(Id)
            {
                RemainingAmount = (double)RemainingAmount,
                RequestedAmount = (double)Amount,
                Symbol = Symbol,
                Type = FdkToAlgo.Convert(orderType),
                Side = FdkToAlgo.Convert(Side),
                Price = (double)Price
            };
        }

        #region Order Properties 

        private TradeRecordType orderType;
        private decimal amount;
        private decimal amountRemaining;
        public TradeRecordSide side;
        private decimal? price;
        private DateTime? created;
        private DateTime? expiration;
        private string comment;
        private decimal? stopLoss;
        private decimal? takeProfit;

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
                    NotifyOfPropertyChange(nameof(Amount));
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
                    NotifyOfPropertyChange(nameof(RemainingAmount));
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
                    NotifyOfPropertyChange(nameof(OrderType));
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

                    CurrentPrice = side == TradeRecordSide.Buy ? symbolObserver?.Ask : symbolObserver?.Bid;

                    NotifyOfPropertyChange(nameof(Side));
                    NotifyOfPropertyChange(nameof(CurrentPrice));
                }
            }
        }

        public decimal? Price
        {
            get { return price; }
            set
            {
                if (price != value)
                {
                    price = value;
                    NotifyOfPropertyChange(nameof(Price));
                }
            }
        }

        public DateTime? Created
        {
            get { return created; }
            set
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
            set
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
            set
            {
                if (comment != value)
                {
                    comment = value;
                    NotifyOfPropertyChange(nameof(Comment));
                }
            }
        }

        public decimal? TakeProfit
        {
            get { return takeProfit; }
            set
            {
                if (takeProfit != value)
                {
                    takeProfit = value;
                    NotifyOfPropertyChange(nameof(TakeProfit));
                }
            }
        }

        public decimal? StopLoss
        {
            get { return stopLoss; }
            set
            {
                if (stopLoss != value)
                {
                    stopLoss = value;
                    NotifyOfPropertyChange(nameof(StopLoss));
                }
            }
        }

        public RateDirectionTracker CurrentPrice { get; private set; }

        #endregion

        public void Dispose()
        {
            this.symbolObserver?.Dispose();
            this.symbolObserver = null;
        }
    }
}
