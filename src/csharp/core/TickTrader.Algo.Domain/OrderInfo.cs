﻿using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Domain
{
    [Flags]
    public enum OrderOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
        MarketWithSlippage = 2,
        HiddenIceberg = 4,
        OneCancelsTheOther = 8,
        ContingentOrder = 16,
    }


    public partial class OrderInfo : IOrderUpdateInfo, IOrderCalcInfo, IOrderLogDetailsInfo, IMarginCalculateRequest, IProfitCalculateRequest, ISwapCalculateRequest
    {
        public OrderOptions Options
        {
            get { return (OrderOptions)OptionsBitmask; }
            set { OptionsBitmask = (int)value; }
        }

        public UtcTicks? Expiration
        {
            get => ExpirationTicks.ToUtcTicks();
            set => ExpirationTicks = value.ToInt64();
        }

        public bool IsImmediateOrCancel => Options.HasFlag(OrderOptions.ImmediateOrCancel);

        public bool IsContingentOrder => Options.HasFlag(OrderOptions.ContingentOrder);

        public bool IsOcoOrder => IsSupportOco && Options.HasFlag(OrderOptions.OneCancelsTheOther);


        public bool IsSupportOco => Type == Types.Type.Limit || Type == Types.Type.Stop;

        public bool IsSupportedSlippage => Type == Types.Type.Stop || Type == Types.Type.Market || Type == Types.Type.Position;


        public bool MarketWithSlippdage => Options.HasFlag(OrderOptions.MarketWithSlippage);

        public bool HiddenIceberg => Options.HasFlag(OrderOptions.HiddenIceberg);

        public bool IsHidden => MaxVisibleAmount.HasValue && MaxVisibleAmount.Value < 1e-9;

        public bool IgnoreCalculation => IsContingentOrder;


        public ISymbolCalculator Calculator { get; set; }

        public double CashMargin { get; set; }

        public SymbolInfo SymbolInfo { get; private set; }

        double IMarginProfitCalc.Price => Price ?? 0;

        double? IOrderLogDetailsInfo.Amount => Type == Types.Type.Market ? LastFillAmount : RemainingAmount;

        double? IOrderLogDetailsInfo.Price => Price;

        string IOrderLogDetailsInfo.OrderId => Id;


        Types.Type IMarginCalculateRequest.Type => Type;

        double IMarginCalculateRequest.Volume => RemainingAmount;

        bool IMarginCalculateRequest.IsHiddenLimit => IsHidden && Type.IsLimit();


        double IProfitCalculateRequest.Price => Price ?? 0.0;

        double IProfitCalculateRequest.Volume => RemainingAmount;

        Types.Side IProfitCalculateRequest.Side => Side;

        double? IProfitCalculateRequest.ClosePrice => null;


        Types.Side ISwapCalculateRequest.Side => Side;

        double ISwapCalculateRequest.Volume => RemainingAmount;


        public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        public event Action<OrderPropArgs<double>> SwapChanged;
        public event Action<OrderPropArgs<double>> CommissionChanged;

        public void SetSymbol(SymbolInfo symbol)
        {
            SymbolInfo = symbol;
        }

        public OrderInfo(SymbolInfo symbol, IOrderUpdateInfo info)
        {
            SymbolInfo = symbol;

            Update(info);
        }


        public void Update(IOrderUpdateInfo info)
        {
            var oldAmount = RequestedAmount;
            var oldPrice = Type.IsStop() ? StopPrice : Price; //? why not just price
            var oldStopPrice = StopPrice;
            var oldType = Type;
            var oldSwap = Swap;
            var oldCommission = Commission;
            var oldIsHidden = IsHidden;

            Id = info.Id ?? string.Empty; //rejected order doesn't have ID (TradeNotAllowed, server don't send order ID)

            Symbol = info.Symbol;
            RequestedAmount = info.RequestedAmount;
            RemainingAmount = info.RemainingAmount;
            Type = info.Type;
            InitialType = info.InitialType;
            Side = info.Side;
            MaxVisibleAmount = info.MaxVisibleAmount;
            Price = info.Price;
            StopPrice = info.StopPrice;
            Created = info.Created;
            Expiration = info.Expiration;
            Modified = info.Modified;
            Comment = info.Comment;
            StopLoss = info.StopLoss;
            TakeProfit = info.TakeProfit;
            Slippage = info.Slippage;
            Swap = info.Swap;
            Commission = info.Commission;
            Options = info.Options;
            RequestedOpenPrice = info.RequestedOpenPrice;
            ParentOrderId = info.ParentOrderId;
            ExecPrice = info.ExecPrice;
            ExecAmount = info.ExecAmount;
            LastFillPrice = info.LastFillPrice;
            LastFillAmount = info.LastFillAmount;
            OcoRelatedOrderId = info.OcoRelatedOrderId;
            OtoTrigger = info.OtoTrigger;

            if (CompositeTag.TryParse(info.UserTag, out CompositeTag compositeTag))
            {
                UserTag = compositeTag.Tag ?? string.Empty;
                InstanceId = compositeTag.Key;
            }
            else
            {
                UserTag = info.UserTag;
                InstanceId = info.InstanceId;
            }

            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, oldPrice, oldStopPrice, oldType, oldIsHidden));

            if (Math.Abs(oldSwap - Swap) > 1e-10)
                SwapChanged?.Invoke(new OrderPropArgs<double>(this, oldSwap, Swap));

            if (Math.Abs(oldCommission - Commission) > 1e-10)
                CommissionChanged?.Invoke(new OrderPropArgs<double>(this, oldCommission, Commission));
        }

        public string GetSnapshotString() => ToString();

        public void SetSwap(double swap)
        {
            var oldSwap = Swap;
            Swap = swap;
            SwapChanged?.Invoke(new OrderPropArgs<double>(this, oldSwap, swap));
        }

        public void ChangeCommission(double newCommision)
        {
            var oldCom = Commission;
            Commission = newCommision;
            CommissionChanged?.Invoke(new OrderPropArgs<double>(this, oldCom, newCommision));
        }

        public void ChangeRemaningAmount(double newAmount)
        {
            var oldAmount = RemainingAmount;
            RemainingAmount = newAmount;
            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, Price, StopPrice, Type, false));
        }

        public void ChangeEssentials(OrderInfo.Types.Type newType, double newAmount, double? newPrice, double? newStopPirce)
        {
            var oldPrice = Price;
            var oldType = Type;
            var oldAmount = RemainingAmount;
            var oldStopPrice = StopPrice;

            Type = newType;
            RemainingAmount = newAmount;
            Price = newPrice;
            StopPrice = newStopPirce;

            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, oldPrice, oldStopPrice, oldType, false));
        }

        public double GetMaxSlippagePrice(double currentPrice)
        {
            if (SymbolInfo != null)
            {
                var koef = Side.IsBuy() ? 1 : -1;
                var slippage = SymbolInfo.Slippage.DefaultValue ?? 0.0;

                switch (SymbolInfo.Slippage.Type)
                {
                    case SlippageInfo.Types.Type.Pips:
                        return currentPrice + koef * (slippage * SymbolInfo.Point);
                    case SlippageInfo.Types.Type.Percent:
                        return currentPrice * (1.0 + koef * slippage);
                }
            }

            return currentPrice;
        }
    }

    public static class OrderOptionsExtensions
    {
        public static string GetString(this OrderOptions options) => options != OrderOptions.None ? (options ^ OrderOptions.None).ToString() : string.Empty;
    }

    public interface IOrderCalcInfo : IOrderCommonInfo
    {
        ISymbolCalculator Calculator { get; set; }

        double CashMargin { get; set; }

        SymbolInfo SymbolInfo { get; }

        bool IgnoreCalculation { get; }

        double? Slippage { get; }

        event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        event Action<OrderPropArgs<double>> SwapChanged;
        event Action<OrderPropArgs<double>> CommissionChanged;


        string GetSnapshotString();
    }

    public interface IOrderUpdateInfo : IOrderCommonInfo
    {
        string InstanceId { get; }
        double RequestedAmount { get; }

        double? MaxVisibleAmount { get; }

        Domain.OrderInfo.Types.Type InitialType { get; }

        Timestamp Created { get; }

        UtcTicks? Expiration { get; }

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
        string OcoRelatedOrderId { get; }
        ContingentOrderTrigger OtoTrigger { get; }
    }

    public interface IOrderCommonInfo : IMarginProfitCalc
    {
        string Id { get; }
        string Symbol { get; }
        double? StopPrice { get; }
        double Commission { get; }
        double Swap { get; }
    }

    public interface IMarginProfitCalc
    {
        double Price { get; }
        Domain.OrderInfo.Types.Side Side { get; }
        Domain.OrderInfo.Types.Type Type { get; }
        double RemainingAmount { get; }
        bool IsHidden { get; }
    }

    public interface IOrderLogDetailsInfo
    {
        string OrderId { get; }

        string Symbol { get; }

        //double SymbolLotSize { get; set; }

        Domain.OrderInfo.Types.Type Type { get; }

        Domain.OrderInfo.Types.Side Side { get; }

        double? Amount { get; }

        double? Price { get; }

        double? StopPrice { get; }

        double? StopLoss { get; }

        double? TakeProfit { get; }

        double? Slippage { get; }
    }

    public struct OrderPropArgs<T>
    {
        public OrderPropArgs(IOrderCalcInfo order, T oldVal, T newVal)
        {
            Order = order;
            OldVal = oldVal;
            NewVal = newVal;
        }

        public IOrderCalcInfo Order { get; }
        public T OldVal { get; }
        public T NewVal { get; }
    }

    public struct OrderEssentialsChangeArgs
    {
        public OrderEssentialsChangeArgs(IOrderCalcInfo order, double oldRemAmount, double? oldPrice, double? oldStopPrice, Domain.OrderInfo.Types.Type oldType, bool oldIsHidden)
        {
            Order = order;
            OldRemAmount = oldRemAmount;
            OldPrice = oldPrice;
            OldStopPrice = oldStopPrice;
            OldType = oldType;
            OldIsHidden = oldIsHidden;
        }

        public IOrderCalcInfo Order { get; }
        public double OldRemAmount { get; }
        public double? OldPrice { get; }
        public double? OldStopPrice { get; }
        public Domain.OrderInfo.Types.Type OldType { get; }
        public bool OldIsHidden { get; }
    }
}
