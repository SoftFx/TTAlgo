using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.CoreV1
{
    public sealed class OrderAccessor : Order
    {
        public OrderInfo Info { get; }

        internal WriteEntity Entity { get; }

        internal SymbolInfo SymbolInfo { get; set; }

        internal double LotSize => SymbolInfo?.LotSize ?? 1;

        internal OrderAccessor(SymbolInfo symbol, OrderInfo info = null)
        {
            SymbolInfo = symbol;
            Info = info ?? new OrderInfo() { Symbol = symbol.Name };

            if (Info.SymbolInfo == null)
                Info.SetSymbol(symbol);

            if (info != null)
                Info.Update(info);

            Entity = new WriteEntity(info.SymbolInfo);
            ContingentTrigger = info.OtoTrigger?.ToApi();
        }

        string Order.Id => Info.Id;

        string Order.Symbol => Info.Symbol;

        double Order.RequestedVolume => Info.RequestedAmount / LotSize;

        double Order.RemainingVolume => Info.RemainingAmount / LotSize;

        double Order.MaxVisibleVolume => Info.MaxVisibleAmount / LotSize ?? double.NaN;

        OrderType Order.Type => Info.Type.ToApiEnum();

        OrderSide Order.Side => Info.Side.ToApiEnum();

        double Order.Price => Info.Price ?? double.NaN;

        double Order.StopPrice => Info.StopPrice ?? double.NaN;

        double Order.StopLoss => Info.StopLoss ?? double.NaN;

        double Order.TakeProfit => Info.TakeProfit ?? double.NaN;

        double Order.Slippage => Info.Slippage ?? double.NaN;

        bool Order.IsNull => false;

        string Order.Comment => Info.Comment;

        string Order.Tag => Info.UserTag;

        string Order.InstanceId => Info.InstanceId;

        DateTime Order.Modified => Info.Modified?.ToDateTime().ToLocalTime() ?? DateTime.MinValue;

        DateTime Order.Created => Info.Created?.ToDateTime().ToLocalTime() ?? DateTime.MinValue;

        DateTime Order.Expiration => Info.Expiration?.ToDateTime().ToLocalTime() ?? DateTime.MinValue;

        double Order.ExecPrice => Info.ExecPrice ?? double.NaN;

        double Order.ExecVolume => Info.ExecAmount / LotSize ?? double.NaN;

        double Order.LastFillPrice => Info.LastFillPrice ?? double.NaN;

        double Order.LastFillVolume => Info.LastFillAmount / LotSize ?? double.NaN;

        double Order.Margin => ProcessResponse(Info.Calculator?.Margin?.Calculate(Info));

        double Order.Profit => ProcessResponse(Info.Calculator?.Profit.Calculate(Info));

        Api.OrderOptions Order.Options => Info.Options.ToApiEnum();

        string Order.OcoRelatedOrderId => Info.OcoRelatedOrderId;

        internal bool IsSameOrderId(OrderAccessor other) => other != null && string.Equals(Info.Id, other.Info.Id);

        internal bool IsSameOrder(OrderAccessor other) => IsSameOrderId(other) && Info.Type == other.Info.Type;

        public override string ToString() => $"#{Info.Id} {Info.Symbol} {Info.Side} {Info.RemainingAmount}";

        public OrderAccessor Clone() => new OrderAccessor(SymbolInfo, Info);

        public static bool IsHiddenOrder(double? maxVisibleVolume) => maxVisibleVolume != null && maxVisibleVolume.Value.E(0.0);

        public IContingentOrderTrigger ContingentTrigger { get; }


        private static double ProcessResponse(ICalculateResponse<double> response)
        {
            return response != null && response.IsCompleted ? response.Value : double.NaN;
        }

        internal sealed class WriteEntity : Order, IOrderCalcInfo, IMarginCalculateRequest, IProfitCalculateRequest
        {
            public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
            public event Action<OrderPropArgs<double>> SwapChanged;
            public event Action<OrderPropArgs<double>> CommissionChanged;

            public WriteEntity(SymbolInfo info)
            {
                SymbolInfo = info;
            }

            internal WriteEntity Clone() => (WriteEntity)MemberwiseClone();

            internal OrderInfo GetInfo()
            {
                return new OrderInfo
                {
                    Id = Id,
                    Symbol = Symbol,
                    RequestedAmount = RequestedAmount,
                    RemainingAmount = RemainingAmount,
                    MaxVisibleAmount = MaxVisibleAmount,
                    Type = Type,
                    Side = Side,
                    Price = Price,
                    StopPrice = StopPrice,
                    StopLoss = StopLoss,
                    TakeProfit = TakeProfit,
                    Slippage = Slippage,
                    Comment = Comment,
                    UserTag = UserTag,
                    InstanceId = InstanceId,
                    Expiration = Expiration?.ToTimestamp(),
                    Created = Created?.ToTimestamp(),
                    Modified = Modified?.ToTimestamp(),
                    ExecPrice = ExecPrice,
                    ExecAmount = (double)ExecAmount,
                    LastFillPrice = LastFillPrice,
                    LastFillAmount = (double)LastFillAmount,
                    Options = Options,
                    Swap = Swap,
                    Commission = Commission,
                    InitialType = InitialType,
                };
            }

            public string GetSnapshotString() => GetInfo().GetSnapshotString();

            public string Id { get; internal set; }

            public string Symbol { get; internal set; }

            public double RequestedAmount { get; internal set; }
            public double RemainingAmount { get; internal set; }
            public double? MaxVisibleAmount { get; internal set; }

            public OrderInfo.Types.Type Type { get; internal set; }
            public OrderInfo.Types.Side Side { get; internal set; }

            public double? Price { get; internal set; }
            double IMarginProfitCalc.Price => Price.Value;

            public double? StopPrice { get; internal set; }

            public double? StopLoss { get; internal set; }
            public double? TakeProfit { get; internal set; }

            public double? Slippage { get; internal set; }

            public string Comment { get; internal set; }
            public string UserTag { get; internal set; }
            public string InstanceId { get; internal set; }

            public DateTime? Expiration { get; internal set; }
            public DateTime? Modified { get; internal set; }
            public DateTime? Created { get; internal set; }

            public double? ExecPrice { get; internal set; }
            public double ExecAmount { get; internal set; }
            public double? LastFillPrice { get; internal set; }
            public double LastFillAmount { get; internal set; }

            public Domain.OrderOptions Options { get; internal set; }

            public double Margin => ProcessResponse(Calculator?.Margin?.Calculate(this));
            public double Profit => ProcessResponse(Calculator?.Profit.Calculate(this));

            public double Swap { get; internal set; }
            public double Commission { get; internal set; }

            public bool IsPending => Type == OrderInfo.Types.Type.Limit || Type == OrderInfo.Types.Type.StopLimit || Type == OrderInfo.Types.Type.Stop;

            public bool IsHidden => MaxVisibleAmount.HasValue && MaxVisibleAmount.Value < 1e-9;

            public OrderInfo.Types.Type InitialType { get; internal set; }
            public short ActionNo { get; internal set; }
            public double? OpenConversionRate { get; internal set; }
            public double? ClosePrice { get; set; }
            internal DateTime PositionCreated { get; set; }
            public string OcoRelatedOrderId { get; internal set; }
            #region API Order

            public bool IsNull => false;

            OrderType Order.Type => Type.ToApiEnum();
            OrderSide Order.Side => Side.ToApiEnum();

            double Order.Price => Price ?? double.NaN;
            double Order.StopPrice => Price ?? double.NaN;

            double Order.StopLoss => StopLoss ?? double.NaN;
            double Order.TakeProfit => TakeProfit ?? double.NaN;
            double Order.Slippage => Slippage ?? double.NaN;

            double Order.RequestedVolume => RequestedAmount / SymbolInfo.LotSize;
            double Order.RemainingVolume => RemainingAmount / SymbolInfo.LotSize;
            double Order.MaxVisibleVolume => MaxVisibleAmount / SymbolInfo.LotSize ?? double.NaN;

            DateTime Order.Created => Created?.ToLocalTime() ?? DateTime.MinValue;
            DateTime Order.Modified => Modified?.ToLocalTime() ?? DateTime.MinValue;
            DateTime Order.Expiration => Expiration?.ToLocalTime() ?? DateTime.MinValue;

            string Order.Tag => UserTag;

            double Order.ExecPrice => ExecPrice ?? double.NaN;
            double Order.ExecVolume => (double)ExecAmount / SymbolInfo.LotSize;
            double Order.LastFillPrice => LastFillPrice ?? double.NaN;
            double Order.LastFillVolume => (double)LastFillAmount / SymbolInfo.LotSize;

            Api.OrderOptions Order.Options => Options.ToApiEnum();

            public ISymbolCalculator Calculator { get; set; }

            public double CashMargin { get; set; }

            public SymbolInfo SymbolInfo { get; }


            OrderInfo.Types.Type IMarginCalculateRequest.Type => Type;

            double IMarginCalculateRequest.Volume => RemainingAmount;

            bool IMarginCalculateRequest.IsHiddenLimit => IsHidden && Type.IsLimit();


            double IProfitCalculateRequest.Price => Price ?? 0.0;

            double IProfitCalculateRequest.Volume => RemainingAmount;

            OrderInfo.Types.Side IProfitCalculateRequest.Side => Side;

            public IContingentOrderTrigger ContingentTrigger => null;

            public bool IgnoreCalculation => false;

            #endregion

            internal void SetSwap(double swap)
            {
                var oldSwap = Swap;
                Swap = swap;
                SwapChanged?.Invoke(new OrderPropArgs<double>(this, oldSwap, swap));
            }

            internal void ChangeCommission(double newCommision)
            {
                var oldCom = Commission;
                Commission = newCommision;
                CommissionChanged?.Invoke(new OrderPropArgs<double>(this, oldCom, newCommision));
            }

            internal void ChangeRemAmount(double newAmount)
            {
                var oldAmount = RemainingAmount;
                RemainingAmount = newAmount;
                EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, Price, StopPrice, Type, false));
            }

            internal void ChangeEssentials(OrderInfo.Types.Type newType, double newAmount, double? newPrice, double? newStopPirce)
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
        }
    }
}
