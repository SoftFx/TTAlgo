using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Calc;

namespace TickTrader.Algo.Common.Model
{
    public class OrderModel : IOrderModel2
    {
        private string fullTag, userTag;

        public string Id { get; }

        public string Symbol { get; }

        public OrderCalculator Calculator { get; set; }

        public decimal CashMargin { get; set; }

        public ISymbolInfo2 SymbolInfo { get; }

        decimal IOrderCalcInfo.RemainingAmount => (decimal)RemainingAmount;

        public bool IsHidden => MaxVisibleAmount.HasValue && MaxVisibleAmount.Value < 1e-9;

        public OrderModel(Domain.OrderInfo record, IOrderDependenciesResolver resolver)
        {
            Id = record.Id;
            Symbol = record.Symbol;
            SymbolInfo = resolver.GetSymbolOrNull(record.Symbol);
            Update(record);
        }

        public OrderModel(ExecutionReport report, IOrderDependenciesResolver resolver)
        {
            Id = report.OrderId ?? "0";
            Symbol = report.Symbol;
            SymbolInfo = resolver.GetSymbolOrNull(report.Symbol);
            Update(report);
        }

        public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        public event Action<OrderPropArgs<decimal>> SwapChanged;
        public event Action<OrderPropArgs<decimal>> CommissionChanged;


        public double Amount { get; private set; }

        public double RemainingAmount { get; private set; }

        public Domain.OrderInfo.Types.Type Type { get; private set; }

        public Domain.OrderInfo.Types.Side Side { get; private set; }

        public double? Price { get; private set; }

        public double? LimitPrice { get; private set; }

        public double? MaxVisibleAmount { get; private set; }

        public double? StopPrice { get; private set; }

        public decimal? Swap { get; private set; }

        public decimal? Commission { get; private set; }

        public DateTime? Created { get; private set; }

        public DateTime? Expiration { get; private set; }

        public string Comment { get; set; }

        public string UserTag
        {
            get => userTag;
            private set
            {
                if (fullTag != value)
                {
                    fullTag = value;

                    if (CompositeTag.TryParse(fullTag, out CompositeTag compositeTag))
                    {
                        userTag = compositeTag.Tag;
                        InstanceId = compositeTag.Key;
                    }
                    else
                    {
                        userTag = fullTag;
                        InstanceId = "";
                    }
                }
            }
        }

        public string InstanceId { get; private set; }

        public string ParentOrderId { get; private set; }

        public double? TakeProfit { get; private set; }

        public double? StopLoss { get; private set; }

        public double? Slippage { get; private set; }

        public DateTime? Modified { get; private set; }

        public double? ExecPrice { get; private set; }
        public double? ExecAmount { get; private set; }
        public double? LastFillPrice { get; private set; }
        public double? LastFillAmount { get; private set; }

        public Domain.OrderInfo.Types.Type InitialType { get; private set; }

        public double? ReqOpenPrice { get; private set; }

        public Domain.OrderOptions ExecOptions { get; private set; }

        public Domain.OrderInfo GetInfo()
        {
            return new Domain.OrderInfo
            {
                Id = Id,
                RemainingAmount = RemainingAmount,
                RequestedAmount = Amount,
                MaxVisibleAmount = MaxVisibleAmount,
                Symbol = Symbol,
                InitialType = InitialType,
                Type = Type,
                Side = Side,
                Price = LimitPrice ?? Price,
                StopPrice = StopPrice,
                StopLoss = StopLoss,
                TakeProfit = TakeProfit,
                Slippage = Slippage,
                Comment = Comment,
                UserTag = UserTag,
                InstanceId = InstanceId,
                Created = Created?.ToTimestamp(),
                Modified = Modified?.ToTimestamp(),
                ExecPrice = ExecPrice, //?? 
                ExecAmount = ExecAmount, //??
                LastFillPrice = LastFillPrice, //??
                LastFillAmount = LastFillAmount, //??
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
            var isStopOrder = record.Type == Domain.OrderInfo.Types.Type.Stop || record.Type == Domain.OrderInfo.Types.Type.StopLimit;
            var isLimitOrder = record.Type == Domain.OrderInfo.Types.Type.Limit || record.Type == Domain.OrderInfo.Types.Type.StopLimit;

            var oldAmount = Amount;
            var oldPrice = Type == Domain.OrderInfo.Types.Type.Stop || Type == Domain.OrderInfo.Types.Type.StopLimit ? StopPrice : Price;
            var oldStopPrice = StopPrice;
            var oldType = Type;
            var oldSwap = Swap;
            var oldCommission = Commission;
            var oldIsHidden = IsHidden;

            Amount = record.RequestedAmount;
            RemainingAmount = record.RemainingAmount;
            Type = record.Type;
            InitialType = record.InitialType;
            Side = record.Side;
            MaxVisibleAmount = record.MaxVisibleAmount;
            Price = isStopOrder ? record.StopPrice : record.Price;
            LimitPrice = isLimitOrder ? record.Price : null;
            StopPrice = record.StopPrice;
            Created = record.Created?.ToDateTime();
            Modified = record.Modified?.ToDateTime();
            Expiration = record.Expiration?.ToDateTime();
            Comment = record.Comment;
            UserTag = record.UserTag;
            StopLoss = record.StopLoss;
            TakeProfit = record.TakeProfit;
            Slippage = record.Slippage;
            Swap = (decimal?)record.Swap;
            Commission = (decimal?)record.Commission;
            ExecOptions = record.Options;
            ReqOpenPrice = record.RequestedOpenPrice;
            ParentOrderId = record.ParentOrderId;

            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, (decimal)oldAmount, oldPrice, oldStopPrice, oldType, oldIsHidden));

            if (oldSwap != Swap)
                SwapChanged?.Invoke(new OrderPropArgs<decimal>(this, oldSwap ?? 0, Swap ?? 0));

            if (oldCommission != Commission)
                CommissionChanged?.Invoke(new OrderPropArgs<decimal>(this, oldCommission ?? 0, Commission ?? 0));
        }

        internal void Update(ExecutionReport report)
        {
            var isStopOrder = report.OrderType == Domain.OrderInfo.Types.Type.Stop || report.OrderType == Domain.OrderInfo.Types.Type.StopLimit;
            var isLimitOrder = report.OrderType == Domain.OrderInfo.Types.Type.Limit || report.OrderType == Domain.OrderInfo.Types.Type.StopLimit;

            var oldAmount = Amount;
            var oldPrice = Type == Domain.OrderInfo.Types.Type.Stop || Type == Domain.OrderInfo.Types.Type.StopLimit ? StopPrice : Price;
            var oldStopPrice = StopPrice;
            var oldType = Type;
            var oldSwap = Swap;
            var oldCommission = Commission;
            var oldIsHidden = IsHidden;

            Amount = report.InitialVolume ?? 0;
            RemainingAmount = report.LeavesVolume;
            Type = report.OrderType;
            InitialType = report.InitialOrderType;
            Side = report.OrderSide;
            MaxVisibleAmount = report.MaxVisibleVolume;
            Price = isStopOrder ? report.StopPrice : report.Price;
            LimitPrice = isLimitOrder ? report.Price : null;
            StopPrice = report.StopPrice;
            Created = report.Created;
            Modified = report.Modified;
            Expiration = report.Expiration;
            Comment = report.Comment;
            UserTag = report.Tag;
            StopLoss = report.StopLoss;
            TakeProfit = report.TakeProfit;
            Slippage = report.Slippage;
            Swap = report.Swap.ToDecimalSafe();
            Commission = report.Commission.ToDecimalSafe();
            ExecPrice = report.AveragePrice; //??
            ExecAmount = report.ExecutedVolume.AsNullable(); //??
            LastFillPrice = report.TradePrice; //??
            LastFillAmount = report.TradeAmount; //??
            ReqOpenPrice = report.ReqOpenPrice;
            ParentOrderId = report.ParentOrderId;

            if (report.ImmediateOrCancel)
                ExecOptions = Domain.OrderOptions.ImmediateOrCancel;

            if (IsHidden)
                ExecOptions |= Domain.OrderOptions.HiddenIceberg;

            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, (decimal)oldAmount, oldPrice, oldStopPrice, oldType, oldIsHidden));

            if (oldSwap != Swap)
                SwapChanged?.Invoke(new OrderPropArgs<decimal>(this, oldSwap ?? 0, Swap ?? 0));

            if (oldCommission != Commission)
                CommissionChanged?.Invoke(new OrderPropArgs<decimal>(this, oldCommission ?? 0, Commission ?? 0));
        }
    }

    public interface IOrderDependenciesResolver
    {
        SymbolModel GetSymbolOrNull(string name);
    }
}
