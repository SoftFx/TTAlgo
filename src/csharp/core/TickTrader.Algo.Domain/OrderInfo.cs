using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum OrderOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
        MarketWithSlippage = 2,
        HiddenIceberg = 4,
    }


    public partial class OrderInfo : IOrderUpdateInfo, IOrderInfo
    {
        public OrderOptions Options
        {
            get { return (OrderOptions)OptionsBitmask; }
            set { OptionsBitmask = (int)value; }
        }

        public bool ImmediateOrCancel => Options.HasFlag(OrderOptions.ImmediateOrCancel);

        public bool MarketWithSlippdage => Options.HasFlag(OrderOptions.MarketWithSlippage);

        public bool HiddenIceberg => Options.HasFlag(OrderOptions.HiddenIceberg);

        public bool IsHidden => MaxVisibleAmount.HasValue && MaxVisibleAmount.Value < 1e-9;

        public bool IsStopOrder => Type == Domain.OrderInfo.Types.Type.Stop || Type == Domain.OrderInfo.Types.Type.StopLimit;
        public bool IsLimitOrder => Type == Domain.OrderInfo.Types.Type.Limit || Type == Domain.OrderInfo.Types.Type.StopLimit;


        public IOrderCalculator Calculator { get; set; }

        public decimal CashMargin { get; set; }

        public ISymbolInfo SymbolInfo { get; private set; }

        decimal IMarginProfitCalc.RemainingAmount => (decimal)RemainingAmount;

        decimal? IOrderCalcInfo.Commission => (decimal)Commission;

        decimal? IOrderCalcInfo.Swap => (decimal)Swap;

        double IMarginProfitCalc.Price => Price ?? 0;

        public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        public event Action<OrderPropArgs<decimal>> SwapChanged;
        public event Action<OrderPropArgs<decimal>> CommissionChanged;

        public void SetSymbol(ISymbolInfo symbol)
        {
            SymbolInfo = symbol;
        }

        public OrderInfo(ISymbolInfo symbol, IOrderUpdateInfo info)
        {
            SymbolInfo = symbol;

            Update(info);
        }


        public void Update(IOrderUpdateInfo info)
        {

            var oldAmount = RequestedAmount;
            var oldPrice = IsStopOrder ? StopPrice : Price;
            var oldStopPrice = StopPrice;
            var oldType = Type;
            var oldSwap = Swap;
            var oldCommission = Commission;
            var oldIsHidden = IsHidden;

            Id = info.Id;
            //InstanceId = info.InstanceId;
            Symbol = info.Symbol;
            RequestedAmount = info.RequestedAmount;
            RemainingAmount = (double)info.RemainingAmount;
            Type = info.Type;
            InitialType = info.InitialType;
            Side = info.Side;
            MaxVisibleAmount = info.MaxVisibleAmount;
            //Price = isStopOrder ? info.StopPrice : info.Price;
            Price = info.Price;
            StopPrice = info.StopPrice;
            Created = info.Created;
            Expiration = info.Expiration;
            Modified = info.Modified;
            Comment = info.Comment;
            StopLoss = info.StopLoss;
            TakeProfit = info.TakeProfit;
            Slippage = info.Slippage;
            Swap = (double)info.Swap;
            Commission = (double)info.Commission;
            Options = info.Options;
            RequestedOpenPrice = info.RequestedOpenPrice;
            ParentOrderId = info.ParentOrderId;
            ExecPrice = info.ExecPrice;
            ExecAmount = info.ExecAmount;
            LastFillPrice = info.LastFillPrice;
            LastFillAmount = info.LastFillAmount;

            if (CompositeTag.TryParse(info.UserTag, out CompositeTag compositeTag))
            {
                UserTag = compositeTag.Tag;
                InstanceId = compositeTag.Key;
            }
            else
            {
                UserTag = info.UserTag;
                InstanceId = "";
            }

            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, (decimal)oldAmount, oldPrice, oldStopPrice, oldType, oldIsHidden));

            if (Math.Abs(oldSwap - Swap) > 1e-10)
                SwapChanged?.Invoke(new OrderPropArgs<decimal>(this, (decimal)oldSwap, (decimal)Swap));

            if (Math.Abs(oldCommission - Commission) > 1e-10)
                CommissionChanged?.Invoke(new OrderPropArgs<decimal>(this, (decimal)oldCommission, (decimal)Commission));
        }
    }

    public static class OrderInfoExtensions
    {
        public static bool IsHidden(this OrderInfo order)
        {
            var maxVisibleVolume = order.MaxVisibleAmount;
            return maxVisibleVolume.HasValue && Math.Abs(maxVisibleVolume.Value) < 1e-9;
        }
    }

    public static class OrderOptionsExtensions
    {
        public static string GetString(this OrderOptions options) => options != OrderOptions.None ? (options ^ OrderOptions.None).ToString() : string.Empty;
    }

    public interface IOrderInfo : IOrderCalcInfo
    {
        IOrderCalculator Calculator { get; set; }

        decimal CashMargin { get; set; }

        ISymbolInfo SymbolInfo { get; }

        event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        event Action<OrderPropArgs<decimal>> SwapChanged;
        event Action<OrderPropArgs<decimal>> CommissionChanged;
    }

    public interface IOrderUpdateInfo : IOrderCalcInfo
    {
        string Id { get; }
        double RequestedAmount { get; }
        double? MaxVisibleAmount { get; }

        Domain.OrderInfo.Types.Type InitialType { get; }

        Timestamp Created { get; }

        Timestamp Expiration { get; }

        string Comment { get; set; }

        string ParentOrderId { get; }

        double? TakeProfit { get; }

        double? StopLoss { get; }

        double? Slippage { get; }

        Timestamp Modified { get; }

        double? RequestedOpenPrice { get; }

        Domain.OrderOptions Options { get; }

        string UserTag { get; }

        double? ExecPrice { get; }
        double? ExecAmount { get; }
        double? LastFillPrice { get; }
        double? LastFillAmount { get; }
    }

    public interface IOrderCalcInfo : IMarginProfitCalc
    {
        string Symbol { get; }
        double? StopPrice { get; }
        decimal? Commission { get; }
        decimal? Swap { get; }
    }

    public interface IMarginProfitCalc
    {
        double Price { get; }
        Domain.OrderInfo.Types.Side Side { get; }
        Domain.OrderInfo.Types.Type Type { get; }
        decimal RemainingAmount { get; }
        bool IsHidden { get; }
    }

    public struct OrderPropArgs<T>
    {
        public OrderPropArgs(IOrderInfo order, T oldVal, T newVal)
        {
            Order = order;
            OldVal = oldVal;
            NewVal = newVal;
        }

        public IOrderInfo Order { get; }
        public T OldVal { get; }
        public T NewVal { get; }
    }

    public struct OrderEssentialsChangeArgs
    {
        public OrderEssentialsChangeArgs(IOrderInfo order, decimal oldRemAmount, double? oldPrice, double? oldStopPrice, Domain.OrderInfo.Types.Type oldType, bool oldIsHidden)
        {
            Order = order;
            OldRemAmount = oldRemAmount;
            OldPrice = oldPrice;
            OldStopPrice = oldStopPrice;
            OldType = oldType;
            OldIsHidden = oldIsHidden;
        }

        public IOrderInfo Order { get; }
        public decimal OldRemAmount { get; }
        public double? OldPrice { get; }
        public double? OldStopPrice { get; }
        public Domain.OrderInfo.Types.Type OldType { get; }
        public bool OldIsHidden { get; }
    }
}
