using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using BL = TickTrader.BusinessLogic;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public class OrderModel : ObservableObject, BL.IOrderModel
    {
        private string clientOrderId;
        private OrderType orderType;
        private decimal amount;
        private decimal amountRemaining;
        public OrderSide side;
        private decimal? price;
        private decimal? stopPrice;
        private decimal? limitPrice;
        private decimal? maxVisibleVolume;
        private decimal? swap;
        private decimal? commission;
        private DateTime? created;
        private DateTime? expiration;
        private string comment;
        private string tag;
        private string userTag;
        private string instanceId;
        private double? stopLoss;
        private double? takeProfit;
        private decimal? profit;
        private decimal? margin;
        private decimal? currentPrice;
        private SymbolModel symbolModel;
        private BL.OrderError error;
        private double? execPrice;
        private double? execAmount;
        private double? lastFillPrice;
        private double? lastFillAmount;
        private DateTime? modified;

        public OrderModel(OrderEntity record, IOrderDependenciesResolver resolver)
        {
            this.Id = record.OrderId;
            this.clientOrderId = record.ClientOrderId;
            this.OrderId = long.Parse(Id);
            this.Symbol = record.Symbol;
            this.symbolModel = resolver.GetSymbolOrNull(record.Symbol);
            Update(record);
        }

        public OrderModel(ExecutionReport report, IOrderDependenciesResolver resolver)
        {
            this.Id = report.OrderId ?? "0";
            this.clientOrderId = report.ClientOrderId;
            this.OrderId = long.Parse(Id);
            this.Symbol = report.Symbol;
            this.symbolModel = resolver.GetSymbolOrNull(report.Symbol);
            Update(report);
        }

        public event Action<BL.IOrderModel> EssentialParametersChanged;

        private double LotSize => symbolModel.LotSize;

        #region Order Properties

        public string Id { get; private set; }
        public long OrderId { get; private set; }
        public string Symbol { get; private set; }
        public decimal Amount
        {
            get { return amount; }
            set
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
        public decimal? AmountLots { get; private set; } = 0;
        public decimal RemainingAmount
        {
            get { return amountRemaining; }
            set
            {
                if (this.amountRemaining != value)
                {
                    this.amountRemaining = value;
                    this.RemainingAmountLots = AmountToLots(value);
                    NotifyOfPropertyChange(nameof(RemainingAmount));
                    NotifyOfPropertyChange(nameof(RemainingAmountLots));
                }
            }
        }
        public decimal? RemainingAmountLots { get; private set; } = 0;
        public OrderType OrderType
        {
            get { return orderType; }
            set
            {
                if (orderType != value)
                {
                    this.orderType = value;
                    NotifyOfPropertyChange(nameof(OrderType));
                    NotifyOfPropertyChange(nameof(AggregatedType));
                }
            }
        }
        public OrderSide Side
        {
            get { return side; }
            private set
            {
                if (side != value)
                {
                    side = value;
                    NotifyOfPropertyChange(nameof(Side));
                    NotifyOfPropertyChange(nameof(AggregatedType));
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
        public decimal? LimitPrice
        {
            get { return limitPrice; }
            set
            {
                if (limitPrice != value)
                {
                    limitPrice = value;
                    NotifyOfPropertyChange(nameof(LimitPrice));
                }
            }
        }
        public decimal? MaxVisibleVolume
        {
            get { return maxVisibleVolume; }
            set
            {
                if (maxVisibleVolume != value)
                {
                    maxVisibleVolume = value;
                    NotifyOfPropertyChange(nameof(MaxVisibleVolume));
                }
            }
        }
        public decimal? StopPrice
        {
            get { return stopPrice; }
            set
            {
                if (stopPrice != value)
                {
                    stopPrice = value;
                    NotifyOfPropertyChange(nameof(StopPrice));
                }
            }
        }
        public decimal? Swap
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
        public decimal? Commission
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
        public string Tag
        {
            get { return userTag; }
            private set
            {
                if (tag != value)
                {
                    tag = value;

                    if (CompositeTag.TryParse(tag, out CompositeTag compositeTag))
                    {
                        userTag = compositeTag.Tag;
                        instanceId = compositeTag.Key;
                    }
                    else
                    {
                        userTag = tag;
                        instanceId = "";
                    }

                    NotifyOfPropertyChange(nameof(Tag));
                    NotifyOfPropertyChange(nameof(InstanceId));
                }
            }
        }
        public string InstanceId
        {
            get { return instanceId; }
            private set
            {
                if (instanceId != value)
                {
                    instanceId = value;
                    NotifyOfPropertyChange(nameof(InstanceId));
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
        public decimal? Profit
        {
            get { return profit; }
            set
            {
                if (profit != value)
                {
                    profit = value;
                    NotifyOfPropertyChange(nameof(Profit));
                    NetProfit = profit + commission + swap;
                    NotifyOfPropertyChange(nameof(NetProfit));
                }
            }
        }
        public decimal? Margin
        {
            get { return margin; }
            set
            {
                if (margin != value)
                {
                    margin = value;
                    NotifyOfPropertyChange(nameof(Margin));
                }
            }
        }
        public decimal? NetProfit { get; private set; }
        public decimal? CurrentPrice
        {
            get { return currentPrice; }
            set
            {
                if (currentPrice != value)
                {
                    currentPrice = value;
                    NotifyOfPropertyChange(nameof(CurrentPrice));
                }
            }
        }

        public DateTime? Modified
        {
            get { return modified; }
            private set
            {
                if (modified != value)
                {
                    modified = value;
                    NotifyOfPropertyChange(nameof(Modified));
                }
            }
        }

        public double? ExecPrice
        {
            get { return execPrice; }
            set
            {
                if (execPrice != value)
                {
                    execPrice = value;
                    NotifyOfPropertyChange(nameof(ExecPrice));
                }
            }
        }

        public double? ExecAmount
        {
            get { return execAmount; }
            set
            {
                if (execAmount != value)
                {
                    execAmount = value;
                    ExecAmountLots = AmountToLots(value);
                    NotifyOfPropertyChange(nameof(ExecAmount));
                    NotifyOfPropertyChange(nameof(ExecAmountLots));
                }
            }
        }
        public double? ExecAmountLots { get; private set; }

        public double? LastFillPrice
        {
            get { return lastFillPrice; }
            set
            {
                if (lastFillPrice != value)
                {
                    lastFillPrice = value;
                    NotifyOfPropertyChange(nameof(LastFillPrice));
                }
            }
        }

        public double? LastFillAmount
        {
            get { return lastFillAmount; }
            set
            {
                if (lastFillAmount != value)
                {
                    lastFillAmount = value;
                    LastFillAmountLots = AmountToLots(value);
                    NotifyOfPropertyChange(nameof(LastFillAmount));
                    NotifyOfPropertyChange(nameof(LastFillAmountLots));
                }
            }
        }

        public double? LastFillAmountLots { get; private set; }
        public string MarginCurrency => symbolModel?.BaseCurrency?.Name;
        public string ProfitCurrency => symbolModel?.QuoteCurrency?.Name;

        public OrderExecOptions ExecOptions { get; private set; }

        #endregion

        #region IOrderModel

        public bool HasError { get { return CalculationError != null; } }
        public decimal? AgentCommision { get { return 0; } }
        public BL.OrderError CalculationError
        {
            get { return error; }
            set
            {
                error = value;
                NotifyOfPropertyChange(nameof(CalculationError));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }
        public BL.OrderCalculator Calculator { get; set; }
        bool BL.IOrderModel.IsCalculated { get { return CalculationError == null; } }
        decimal? BL.IOrderModel.MarginRateCurrent { get; set; }
        

        BO.OrderTypes BL.ICommonOrder.Type
        {
            get => TickTraderToAlgo.Convert(orderType);
            set => throw new NotImplementedException();
           
        }

        BO.OrderSides BL.ICommonOrder.Side
        {
            get => TickTraderToAlgo.Convert(side);
            set => throw new NotImplementedException();
        }

        bool BL.ICommonOrder.IsHidden => MaxVisibleVolume.HasValue && MaxVisibleVolume.Value == 0;
        bool BL.ICommonOrder.IsIceberg => MaxVisibleVolume.HasValue && MaxVisibleVolume.Value > 0;
        string BL.ICommonOrder.MarginCurrency { get => MarginCurrency; set => throw new NotImplementedException(); }
        string BL.ICommonOrder.ProfitCurrency { get => ProfitCurrency; set => throw new NotImplementedException(); }
        decimal? BL.ICommonOrder.MaxVisibleAmount => MaxVisibleVolume;
        public AggregatedOrderType AggregatedType => side.Aggregate(orderType);

        #endregion

        public OrderEntity GetEntity()
        {
            return new OrderEntity(Id)
            {
                ClientOrderId = this.clientOrderId,
                RemainingVolume = (double)RemainingAmount,
                RequestedVolume = (double)Amount,
                MaxVisibleVolume = (double?)MaxVisibleVolume,
                Symbol = Symbol,
                Type = orderType,
                Side = Side,
                Price = (double?)LimitPrice ?? (double?)Price,
                StopPrice = (double?)StopPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Comment = this.Comment,
                UserTag = this.Tag,
                InstanceId = this.InstanceId,
                Created = this.Created,
                Modified = this.Modified,
                ExecPrice = ExecPrice,
                ExecVolume = ExecAmount,
                LastFillPrice = LastFillPrice,
                LastFillVolume = LastFillAmount,
                Swap = (double)(Swap ?? 0),
                Commission = (double)(Commission ?? 0),
                Expiration = Expiration,
                Options = ExecOptions
            };
        }

        internal void Update(OrderEntity record)
        {
            this.Amount = (decimal)record.InitialVolume;
            this.RemainingAmount = (decimal)record.Volume;
            this.OrderType = record.Type;
            this.Side = record.Side;
            this.MaxVisibleVolume = (decimal?)record.MaxVisibleVolume;
            this.Price = (decimal?)(record.Type == OrderType.Stop ? record.StopPrice : record.Price);
            this.LimitPrice = (decimal?)(record.Type == OrderType.StopLimit || record.Type == OrderType.Limit ? record.Price : null);
            this.StopPrice = (decimal?)record.StopPrice;
            this.Created = record.Created;
            this.Modified = record.Modified;
            this.Expiration = record.Expiration;
            this.Comment = record.Comment;
            this.Tag = record.UserTag;
            this.StopLoss = record.StopLoss;
            this.TakeProfit = record.TakeProfit;
            this.Swap = (decimal?)record.Swap;
            this.Commission = (decimal?)record.Commission;
            this.ExecOptions = record.Options;
            //if (record.ImmediateOrCancel)
            //{
            //    this.RemainingAmount = (decimal)(record.InitialVolume - record.Volume);
            //    this.ExecPrice = record.Price;
            //    this.ExecAmount = record.Volume;
            //    this.LastFillPrice = record.Price;
            //    this.LastFillAmount = record.Volume;
            //}

            EssentialParametersChanged?.Invoke(this);
        }

        internal void Update(ExecutionReport report)
        {
            this.Amount = report.InitialVolume.ToDecimalSafe() ?? 0M;
            this.RemainingAmount = report.LeavesVolume.ToDecimalSafe() ?? 0M;
            this.OrderType = report.OrderType;
            this.Side = report.OrderSide;
            this.MaxVisibleVolume = report.MaxVisibleVolume.ToDecimalSafe();
            this.Price = (report.OrderType == OrderType.Stop ? report.StopPrice : report.Price).ToDecimalSafe();
            this.LimitPrice = (report.OrderType == OrderType.StopLimit || report.OrderType == OrderType.Limit ? report.Price : null).ToDecimalSafe();
            this.StopPrice = report.StopPrice.ToDecimalSafe();
            this.Created = report.Created;
            this.Modified = report.Modified;
            this.Expiration = report.Expiration;
            this.Comment = report.Comment;
            this.Tag = report.Tag;
            this.StopLoss = report.StopLoss;
            this.TakeProfit = report.TakeProfit;
            this.Swap = report.Swap.ToDecimalSafe();
            this.Commission = report.Commission.ToDecimalSafe();
            this.ExecPrice = report.AveragePrice;
            this.ExecAmount = report.ExecutedVolume.AsNullable();
            this.LastFillPrice = report.TradePrice;
            this.LastFillAmount = report.TradeAmount;

            if (report.ImmediateOrCancel)
                ExecOptions = OrderExecOptions.ImmediateOrCancel;

            EssentialParametersChanged?.Invoke(this);
        }

        private decimal? AmountToLots(decimal? volume)
        {
            if (volume == null || symbolModel == null)
                return null;

            return volume / (decimal)symbolModel.LotSize;
        }

        private double? AmountToLots(double? volume)
        {
            if (volume == null || symbolModel == null)
                return null;

            return volume / symbolModel.LotSize;
        }

        private TradeVolume ToVolume(decimal? volume, decimal? volumeLots)
        {
            return new TradeVolume((double?)volume ?? double.NaN, (double?)volumeLots ?? double.NaN);
        }

        private TradeVolume ToVolume(double? volume, double? volumeLots)
        {
            return new TradeVolume(volume ?? double.NaN, volumeLots ?? double.NaN);
        }
    }

    public interface IOrderDependenciesResolver
    {
        SymbolModel GetSymbolOrNull(string name);
    }
}
