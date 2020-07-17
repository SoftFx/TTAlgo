using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Calc;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class OrderModel : ObservableObject, IOrderModel2
    {
        private Domain.OrderInfo.Types.Type orderType;
        private Domain.OrderInfo.Types.Type initOrderType;
        private decimal amount;
        private decimal amountRemaining;
        private Domain.OrderInfo.Types.Side side;
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
        private double? slippage;
        private decimal? profit;
        private decimal? margin;
        private decimal? currentPrice;
        private SymbolModel symbolModel;
        private double? execPrice;
        private double? reqOpenPrice;
        private double? execAmount;
        private double? lastFillPrice;
        private double? lastFillAmount;
        private DateTime? modified;
        private string orderExecutionOptionsStr;
        private string parentOrderId;

        public OrderModel(Domain.OrderInfo record, IOrderDependenciesResolver resolver)
        {
            this.Id = record.Id;
            this.OrderId = long.Parse(Id);
            this.Symbol = record.Symbol;
            this.symbolModel = resolver.GetSymbolOrNull(record.Symbol);
            Update(record);
        }

        public OrderModel(ExecutionReport report, IOrderDependenciesResolver resolver)
        {
            this.Id = report.OrderId ?? "0";
            this.OrderId = long.Parse(Id);
            this.Symbol = report.Symbol;
            this.symbolModel = resolver.GetSymbolOrNull(report.Symbol);
            Update(report);
        }

        //public event Action<BL.IOrderModel> EssentialParametersChanged;
        public event Action<IOrderModel2> EssentialParametersChanged;
        public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        public event Action<OrderPropArgs<decimal>> SwapChanged;
        public event Action<OrderPropArgs<decimal>> CommissionChanged;

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
        public Domain.OrderInfo.Types.Type OrderType
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
        public Domain.OrderInfo.Types.Side Side
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

        public decimal? MaxVisibleVolumeToLots => maxVisibleVolume.HasValue ? maxVisibleVolume / (decimal)LotSize : null;

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
                    var old = swap;
                    swap = value;

                    NotifyOfPropertyChange(nameof(Swap));
                    SwapChanged?.Invoke(new OrderPropArgs<decimal>(this, old ?? 0, swap ?? 0));
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
                    var old = commission;
                    commission = value;

                    NotifyOfPropertyChange(nameof(Commission));
                    CommissionChanged?.Invoke(new OrderPropArgs<decimal>(this, old ?? 0, commission ?? 0));
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
        public string ParentOrderId
        {
            get { return parentOrderId; }
            private set
            {
                if (parentOrderId != value)
                {
                    parentOrderId = value;
                    NotifyOfPropertyChange(nameof(ParentOrderId));
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
        public double? Slippage
        {
            get { return slippage; }
            private set
            {
                if (slippage != value)
                {
                    slippage = value;
                    NotifyOfPropertyChange(nameof(Slippage));
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

        public string OrderExecutionOptionsStr
        {
            get => orderExecutionOptionsStr;
            set
            {
                if (orderExecutionOptionsStr == value)
                    return;

                orderExecutionOptionsStr = value;

                NotifyOfPropertyChange(nameof(OrderExecutionOptionsStr));
            }
        }

        public Domain.OrderInfo.Types.Type InitOrderType
        {
            get { return initOrderType; }
            set
            {
                if (initOrderType != value)
                {
                    this.initOrderType = value;
                    NotifyOfPropertyChange(nameof(InitOrderType));
                }
            }
        }

        public double? ReqOpenPrice
        {
            get => reqOpenPrice;
            set
            {
                if (reqOpenPrice != value)
                {
                    this.reqOpenPrice = value;
                    NotifyOfPropertyChange(nameof(ReqOpenPrice));
                }
            }
        }

        public string MarginCurrency => symbolModel?.BaseCurrency?.Name;
        public string ProfitCurrency => symbolModel?.QuoteCurrency?.Name;

        public Domain.OrderOptions ExecOptions { get; private set; }

        #endregion

        #region IOrderModel
        public AggregatedOrderType AggregatedType => side.Aggregate(orderType);

        public OrderCalculator Calculator { get; set; }
        public decimal CashMargin { get; set; }
        public ISymbolInfo2 SymbolInfo => symbolModel;

        double? IOrderCalcInfo.Price => (double?)Price;

        double? IOrderCalcInfo.StopPrice => (double?)StopPrice;

        public bool IsHidden => MaxVisibleVolume.HasValue && MaxVisibleVolume.Value == 0;

        public OrderInfo.Types.Type Type => OrderType;

        //double IOrderCalcInfo.Amount => (double)Amount;


        #endregion

        public Domain.OrderInfo GetInfo()
        {
            return new Domain.OrderInfo
            {
                Id = Id,
                RemainingAmount = (double)RemainingAmount,
                RequestedAmount = (double)Amount,
                MaxVisibleAmount = (double?)MaxVisibleVolume,
                Symbol = Symbol,
                InitialType = initOrderType,
                Type = orderType,
                Side = Side,
                Price = (double?)LimitPrice ?? (double?)Price,
                StopPrice = (double?)StopPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Slippage = slippage,
                Comment = Comment,
                UserTag = Tag,
                InstanceId = InstanceId,
                Created = Created?.ToTimestamp(),
                Modified = Modified?.ToTimestamp(),
                ExecPrice = ExecPrice,
                ExecAmount = ExecAmount,
                LastFillPrice = LastFillPrice,
                LastFillAmount = LastFillAmount,
                Swap = (double)(Swap ?? 0),
                Commission = (double)(Commission ?? 0),
                Expiration = Expiration?.ToTimestamp(),
                Options = ExecOptions,
                RequestedOpenPrice = ReqOpenPrice,
                ParentOrderId = ParentOrderId,
            };
        }

        internal void Update(Domain.OrderInfo record)
        {
            var oldAmount = Amount;
            var oldPrice = OrderType == Domain.OrderInfo.Types.Type.Stop || OrderType == Domain.OrderInfo.Types.Type.StopLimit ? StopPrice : Price;
            var oldStopPrice = StopPrice;
            var oldType = Type;

            this.Amount = (decimal)record.RequestedAmount;
            this.RemainingAmount = (decimal)record.RemainingAmount;
            this.OrderType = record.Type;
            this.InitOrderType = record.InitialType;
            this.Side = record.Side;
            this.MaxVisibleVolume = (decimal?)record.MaxVisibleAmount;
            this.Price = (decimal?)(record.Type == Domain.OrderInfo.Types.Type.Stop || record.Type == Domain.OrderInfo.Types.Type.StopLimit ? record.StopPrice : record.Price);
            this.LimitPrice = (decimal?)(record.Type == Domain.OrderInfo.Types.Type.StopLimit || record.Type == Domain.OrderInfo.Types.Type.Limit ? record.Price : null);
            this.StopPrice = (decimal?)record.StopPrice;
            this.Created = record.Created?.ToDateTime();
            this.Modified = record.Modified?.ToDateTime();
            this.Expiration = record.Expiration?.ToDateTime();
            this.Comment = record.Comment;
            this.Tag = record.UserTag;
            this.StopLoss = record.StopLoss;
            this.TakeProfit = record.TakeProfit;
            this.Slippage = record.Slippage;
            this.Swap = (decimal?)record.Swap;
            this.Commission = (decimal?)record.Commission;
            this.ExecOptions = record.Options;
            this.OrderExecutionOptionsStr = record.Options.ToString();
            this.ReqOpenPrice = record.RequestedOpenPrice;
            this.ParentOrderId = record.ParentOrderId;
            //if (record.ImmediateOrCancel)
            //{
            //    this.RemainingAmount = (decimal)(record.InitialVolume - record.Volume);
            //    this.ExecPrice = record.Price;
            //    this.ExecAmount = record.Volume;
            //    this.LastFillPrice = record.Price;
            //    this.LastFillAmount = record.Volume;
            //}

            EssentialParametersChanged?.Invoke(this);
            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, (decimal)oldAmount, (double?)oldPrice, (double?)oldStopPrice, oldType, false));
        }

        internal void Update(ExecutionReport report)
        {
            var oldAmount = Amount;
            var oldPrice = OrderType == Domain.OrderInfo.Types.Type.Stop || OrderType == Domain.OrderInfo.Types.Type.StopLimit ? StopPrice : Price;
            var oldStopPrice = StopPrice;
            var oldType = Type;

            this.Amount = report.InitialVolume.ToDecimalSafe() ?? 0M;
            this.RemainingAmount = report.LeavesVolume.ToDecimalSafe() ?? 0M;
            this.OrderType = report.OrderType;
            this.InitOrderType = report.InitialOrderType;
            this.Side = report.OrderSide;
            this.MaxVisibleVolume = report.MaxVisibleVolume.ToDecimalSafe();
            this.Price = (report.OrderType == Domain.OrderInfo.Types.Type.Stop || report.OrderType == Domain.OrderInfo.Types.Type.StopLimit ? report.StopPrice : report.Price).ToDecimalSafe();
            this.LimitPrice = (report.OrderType == Domain.OrderInfo.Types.Type.StopLimit || report.OrderType == Domain.OrderInfo.Types.Type.Limit ? report.Price : null).ToDecimalSafe();
            this.StopPrice = report.StopPrice.ToDecimalSafe();
            this.Created = report.Created;
            this.Modified = report.Modified;
            this.Expiration = report.Expiration;
            this.Comment = report.Comment;
            this.Tag = report.Tag;
            this.StopLoss = report.StopLoss;
            this.TakeProfit = report.TakeProfit;
            this.Slippage = report.Slippage;
            this.Swap = report.Swap.ToDecimalSafe();
            this.Commission = report.Commission.ToDecimalSafe();
            this.ExecPrice = report.AveragePrice;
            this.ExecAmount = report.ExecutedVolume.AsNullable();
            this.LastFillPrice = report.TradePrice;
            this.LastFillAmount = report.TradeAmount;
            this.OrderExecutionOptionsStr = GetOrderExecOptions(report);
            this.ReqOpenPrice = report.ReqOpenPrice;
            this.ParentOrderId = report.ParentOrderId;

            if (report.ImmediateOrCancel)
                ExecOptions = Domain.OrderOptions.ImmediateOrCancel;

            EssentialParametersChanged?.Invoke(this);
            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, (decimal)oldAmount, (double?)oldPrice, (double?)oldStopPrice, oldType, false));
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

        private string GetOrderExecOptions(ExecutionReport report)
        {
            var op = new List<Domain.OrderOptions>();

            if (report.ImmediateOrCancel)
                op.Add(Domain.OrderOptions.ImmediateOrCancel);

            if (report.MarketWithSlippage)
                op.Add(Domain.OrderOptions.MarketWithSlippage);

            if (report.MaxVisibleVolume >= 0)
                op.Add(Domain.OrderOptions.HiddenIceberg);

            return string.Join(",", op);
        }
    }

    public interface IOrderDependenciesResolver
    {
        SymbolModel GetSymbolOrNull(string name);
    }
}
