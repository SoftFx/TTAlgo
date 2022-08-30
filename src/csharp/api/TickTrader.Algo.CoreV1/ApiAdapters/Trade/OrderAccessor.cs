using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.CoreV1
{
    public sealed class OrderAccessor : Order
    {
        public OrderInfo Info { get; private set; }

        internal WriteEntity Entity { get; private set; }

        internal SymbolInfo SymbolInfo { get; set; }

        internal double LotSize => SymbolInfo?.LotSize ?? 1;


        internal OrderAccessor() { } //for DeepCopy method

        internal OrderAccessor(SymbolInfo symbol, OrderInfo info = null)
        {
            SymbolInfo = symbol;

            if (info == null)
            {
                info = new OrderInfo() { Symbol = symbol.Name };
                info.SetSymbol(symbol);
            }
            else
            {
                if (info.SymbolInfo == null)
                    info.SetSymbol(symbol);
            }
            Info = info;

            Entity = new WriteEntity();
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

        DateTime Order.Expiration => Info.Expiration?.ToLocalDateTime() ?? DateTime.MinValue;

        double Order.ExecPrice => Info.ExecPrice ?? double.NaN;

        double Order.ExecVolume => Info.ExecAmount / LotSize ?? double.NaN;

        double Order.LastFillPrice => Info.LastFillPrice ?? double.NaN;

        double Order.LastFillVolume => Info.LastFillAmount / LotSize ?? double.NaN;

        double Order.Margin => ProcessResponse(Info.Calculator?.Margin?.Calculate(Info));

        double Order.Profit => ProcessResponse(Info.Calculator?.Profit.Calculate(Info));

        Api.OrderOptions Order.Options => Info.Options.ToApiEnum();

        string Order.OcoRelatedOrderId => Info.OcoRelatedOrderId;

        Order Order.DeepCopy() => new OrderAccessor { SymbolInfo = SymbolInfo, Info = Info, ContingentTrigger = ContingentTrigger, Entity = Entity.Clone() };

        internal bool IsSameOrderId(OrderAccessor other) => other != null && string.Equals(Info.Id, other.Info.Id);

        internal bool IsSameOrder(OrderAccessor other) => IsSameOrderId(other) && Info.Type == other.Info.Type;

        public override string ToString() => $"#{Info.Id} {Info.Symbol} {Info.Side} {Info.RemainingAmount}";

        public OrderAccessor Clone() => new OrderAccessor(SymbolInfo, Info.Clone());

        public static bool IsHiddenOrder(double? maxVisibleVolume) => maxVisibleVolume != null && maxVisibleVolume.Value.E(0.0);

        public IContingentOrderTrigger ContingentTrigger { get; private set; }


        private static double ProcessResponse(ICalculateResponse<double> response)
        {
            return response != null && response.IsCompleted ? response.Value : double.NaN;
        }


        internal sealed class WriteEntity
        {
            public short ActionNo { get; internal set; }
            public double? OpenConversionRate { get; internal set; }
            public double? ClosePrice { get; set; }
            internal DateTime PositionCreated { get; set; }


            public WriteEntity Clone()
            {
                return new WriteEntity()
                {
                    ActionNo = ActionNo,
                    OpenConversionRate = OpenConversionRate,
                    ClosePrice = ClosePrice,
                    PositionCreated = PositionCreated,
                };
            }
        }
    }
}
