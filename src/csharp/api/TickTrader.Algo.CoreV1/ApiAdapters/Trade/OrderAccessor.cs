using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;
using TickTrader.Algo.Core.Lib;
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

            Entity = new WriteEntity(info.SymbolInfo, info);
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

        Order Order.DeepCopy() => this.DeepCopy();

        internal bool IsSameOrderId(OrderAccessor other) => other != null && string.Equals(Info.Id, other.Info.Id);

        internal bool IsSameOrder(OrderAccessor other) => IsSameOrderId(other) && Info.Type == other.Info.Type;

        public override string ToString() => $"#{Info.Id} {Info.Symbol} {Info.Side} {Info.RemainingAmount}";

        public OrderAccessor Clone() => new OrderAccessor(SymbolInfo, Info.Clone());

        public static bool IsHiddenOrder(double? maxVisibleVolume) => maxVisibleVolume != null && maxVisibleVolume.Value.E(0.0);

        public IContingentOrderTrigger ContingentTrigger { get; }


        private static double ProcessResponse(ICalculateResponse<double> response)
        {
            return response != null && response.IsCompleted ? response.Value : double.NaN;
        }


        internal sealed class WriteEntity : IOrderCalcInfo, IMarginCalculateRequest, IProfitCalculateRequest
        {
            private OrderInfo _order;

            public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
            public event Action<OrderPropArgs<double>> SwapChanged;
            public event Action<OrderPropArgs<double>> CommissionChanged;

            public WriteEntity(SymbolInfo info, OrderInfo order)
            {
                SymbolInfo = info;
                _order = order;
            }

            public string Id => _order.Id;
            public string Symbol => _order.Symbol;
            public OrderInfo.Types.Side Side => _order.Side;
            public OrderInfo.Types.Type Type => _order.Type;
            public double? StopPrice => _order.StopPrice;
            public double RemainingAmount => _order.RemainingAmount;
            public double Commission => _order.Commission;
            public double Swap => _order.Swap;
            double IMarginProfitCalc.Price => _order.Price.Value;
            public string GetSnapshotString() => _order.GetSnapshotString();

            public double Margin => ProcessResponse(Calculator?.Margin?.Calculate(this));
            public double Profit => ProcessResponse(Calculator?.Profit.Calculate(this));

            public bool IsPending => _order.Type == OrderInfo.Types.Type.Limit || _order.Type == OrderInfo.Types.Type.StopLimit || _order.Type == OrderInfo.Types.Type.Stop;

            public bool IsHidden => _order.MaxVisibleAmount.HasValue && _order.MaxVisibleAmount.Value < 1e-9;

            public short ActionNo { get; internal set; }
            public double? OpenConversionRate { get; internal set; }
            public double? ClosePrice { get; set; }
            internal DateTime PositionCreated { get; set; }
            public string OcoRelatedOrderId { get; internal set; }
            #region API Order

            public ISymbolCalculator Calculator { get; set; }

            public double CashMargin { get; set; }

            public SymbolInfo SymbolInfo { get; }


            OrderInfo.Types.Type IMarginCalculateRequest.Type => _order.Type;

            double IMarginCalculateRequest.Volume => _order.RemainingAmount;

            bool IMarginCalculateRequest.IsHiddenLimit => IsHidden && _order.Type.IsLimit();


            double IProfitCalculateRequest.Price => _order.Price ?? 0.0;

            double IProfitCalculateRequest.Volume => _order.RemainingAmount;

            OrderInfo.Types.Side IProfitCalculateRequest.Side => _order.Side;

            public IContingentOrderTrigger ContingentTrigger => null;

            public bool IgnoreCalculation => false;

            #endregion

            internal void SetSwap(double swap)
            {
                var oldSwap = _order.Swap;
                _order.Swap = swap;
                SwapChanged?.Invoke(new OrderPropArgs<double>(this, oldSwap, swap));
            }

            internal void ChangeCommission(double newCommision)
            {
                var oldCom = _order.Commission;
                _order.Commission = newCommision;
                CommissionChanged?.Invoke(new OrderPropArgs<double>(this, oldCom, newCommision));
            }

            internal void ChangeRemAmount(double newAmount)
            {
                var order = _order;
                var oldAmount = order.RemainingAmount;
                order.RemainingAmount = newAmount;
                EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, order.Price, order.StopPrice, order.Type, false));
            }

            internal void ChangeEssentials(OrderInfo.Types.Type newType, double newAmount, double? newPrice, double? newStopPirce)
            {
                var order = _order;
                var oldPrice = order.Price;
                var oldType = order.Type;
                var oldAmount = order.RemainingAmount;
                var oldStopPrice = order.StopPrice;

                order.Type = newType;
                order.RemainingAmount = newAmount;
                order.Price = newPrice;
                order.StopPrice = newStopPirce;

                EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, oldPrice, oldStopPrice, oldType, false));
            }

            public Order DeepCopy()
            {
                throw new NotImplementedException();
            }
        }
    }
}
