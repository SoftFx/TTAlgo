using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class OrderAccessor : IOrderInfo
    {
        private SymbolAccessor _symbol;
        private double _lotSize;
        private int _leverage;
        private ReadEntity _readEntity;
        private WriteEntity _writeEntity;
        private Order _apiOrder;
        private IOrderCalcInfo _calcInfo;

        internal OrderAccessor(OrderInfo info, Func<string, SymbolAccessor> symbolProvider, int leverage)
            : this(info, symbolProvider(info.Symbol), leverage)
        {

        }

        internal OrderAccessor(OrderInfo info, SymbolAccessor symbol, int leverage)
        {
            Init(symbol, leverage);

            _readEntity = new ReadEntity(this, info);
            _apiOrder = _readEntity;
            _calcInfo = _readEntity;
        }

        internal OrderAccessor(SymbolAccessor symbol, int leverage)
        {
            Init(symbol, leverage);

            _writeEntity = new WriteEntity(this);
            _apiOrder = _writeEntity;
            _calcInfo = _writeEntity;
        }

        private OrderAccessor() { }

        private void Init(SymbolAccessor symbol, int leverage)
        {
            _symbol = symbol;
            _lotSize = _symbol?.ContractSize ?? 1;
            _leverage = leverage;
        }

        public OrderAccessor Clone()
        {
            var clone = new OrderAccessor();
            clone.Init(_symbol, _leverage);
            if (_readEntity != null)
            {
                clone._readEntity = _readEntity.Clone(clone);
                clone._apiOrder = clone._readEntity;
                clone._calcInfo = clone._readEntity;
            }
            else
            {
                clone._writeEntity = _writeEntity.Clone(clone);
                clone._apiOrder = clone._writeEntity;
                clone._calcInfo = clone._writeEntity;
            }
            clone.Calculator = Calculator;
            return clone;
        }

        internal void Update(OrderInfo info)
        {
            var oldPrice = _calcInfo.Price;
            var oldStopPrice = _calcInfo.StopPrice;
            var oldAmount = _calcInfo.RemainingAmount;
            var oldType = _calcInfo.Type;
            var oldIsHidden = _calcInfo.IsHidden;
            var oldCommission = _calcInfo.Commission ?? 0;
            var oldSwap = _calcInfo.Swap ?? 0;
            _readEntity.Update(info);
            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, oldPrice, oldStopPrice, oldType, oldIsHidden));
            CommissionChanged?.Invoke(new OrderPropArgs<decimal>(this, oldCommission, _calcInfo.Commission ?? 0));
            SwapChanged?.Invoke(new OrderPropArgs<decimal>(this, oldSwap, _calcInfo.Swap ?? 0));
        }

        public Order ApiOrder => _apiOrder;

        internal WriteEntity Entity => _writeEntity;

        public string Id => _apiOrder.Id;
        public string Symbol => _apiOrder.Symbol;
        public double RequestedVolume => _apiOrder.RequestedVolume;
        public double RemainingVolume => _apiOrder.RemainingVolume;
        public OrderInfo.Types.Type Type => _calcInfo.Type;
        public OrderInfo.Types.Side Side => _calcInfo.Side;
        public double Price => _apiOrder.Price;
        public double StopPrice => _apiOrder.StopPrice;
        public double StopLoss => _apiOrder.StopLoss;
        public double TakeProfit => _apiOrder.TakeProfit;
        public double Slippage => _apiOrder.Slippage;
        public bool IsNull => false;
        public string Comment => _apiOrder.Comment;
        public string InstanceId => _apiOrder.InstanceId;
        public DateTime Expiration => _apiOrder.Expiration;
        public DateTime Modified => _apiOrder.Modified.ToUniversalTime();
        public bool IsHidden => _calcInfo.IsHidden;

        #region IOrderModel2

        public decimal RemainingAmount => _calcInfo.RemainingAmount;
        //double? IOrderCalcInfo.Price => _calcInfo.Price;
        double? IOrderCalcInfo.StopPrice => _calcInfo.StopPrice;
        decimal? IOrderCalcInfo.Swap => _calcInfo.Swap;
        decimal? IOrderCalcInfo.Commission => _calcInfo.Commission;

        ISymbolInfo IOrderInfo.SymbolInfo => _symbol;
        public decimal CashMargin { get; set; }
        #endregion

        #region BL IOrderModel

        public IOrderCalculator Calculator { get; set; }
        //public bool IsCalculated => CalculationError == null;
        public double? MarginRateCurrent { get; set; }

        public event Action<OrderEssentialsChangeArgs> EssentialsChanged;
        public event Action<OrderPropArgs<decimal>> SwapChanged;
        public event Action<OrderPropArgs<decimal>> CommissionChanged;

        #endregion

        internal bool HasOption(Domain.OrderOptions option)
        {
            return _apiOrder.Options.HasFlag(option);
        }

        #region Emulation

        internal SymbolAccessor SymbolInfo => _symbol;

        internal void SetSwap(decimal swap)
        {
            var oldSwap = _writeEntity.Swap ?? 0;
            _writeEntity.Swap = swap;
            SwapChanged?.Invoke(new OrderPropArgs<decimal>(this, oldSwap, swap));
        }

        internal void ChangeCommission(decimal newCommision)
        {
            var oldCom = _writeEntity.Commission ?? 0;
            _writeEntity.Commission = newCommision;
            CommissionChanged?.Invoke(new OrderPropArgs<decimal>(this, oldCom, newCommision));
        }

        internal void ChangeRemAmount(decimal newAmount)
        {
            var oldAmount = _writeEntity.RemainingAmount;
            _writeEntity.RemainingAmount = newAmount;
            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, _writeEntity.Price, _writeEntity.StopPrice, Type, false));
        }

        internal void ChangeEssentials(Domain.OrderInfo.Types.Type newType, decimal newAmount, double? newPrice, double? newStopPirce)
        {
            var oldPrice = _writeEntity.Price;
            var oldType = _writeEntity.Type;
            var oldAmount = _writeEntity.RemainingAmount;
            var oldStopPrice = _writeEntity.StopPrice;

            _writeEntity.Type = newType;
            _writeEntity.RemainingAmount = newAmount;
            _writeEntity.Price = newPrice;
            _writeEntity.StopPrice = newStopPirce;

            EssentialsChanged?.Invoke(new OrderEssentialsChangeArgs(this, oldAmount, oldPrice, oldStopPrice, oldType, false));
        }
        #endregion


        internal bool IsSameOrderId(OrderAccessor other)
        {
            return other != null && string.CompareOrdinal(Id, other.Id) == 0;
        }

        internal bool IsSameOrder(OrderAccessor other)
        {
            return IsSameOrderId(other) && Type == other.Type;
        }

        public override string ToString()
        {
            return $"#{Id} {Symbol} {Side} {_apiOrder.RemainingVolume}";
        }

        private double CalculateMargin() => Calculator?.CalculateMargin(this) ?? double.NaN;

        private double CalculateProfit() => Type == OrderInfo.Types.Type.Position ? Calculator?.CalculateProfit(this) ?? double.NaN : double.NaN;

        private static bool IsHiddenOrder(decimal? maxVisibleVolume)
        {
            return maxVisibleVolume.HasValue && maxVisibleVolume.Value == 0;
        }

        private class ReadEntity : Order, IOrderCalcInfo
        {
            private readonly OrderAccessor _accessor;
            private readonly double _lotSize;
            private OrderInfo _info;

            public ReadEntity(OrderAccessor accessor, OrderInfo info)
            {
                _accessor = accessor;
                _lotSize = _accessor._lotSize;
                _info = info;
            }

            internal void Update(OrderInfo info)
            {
                _info = info;
            }

            internal ReadEntity Clone(OrderAccessor newAccessor)
            {
                return new ReadEntity(newAccessor, _info);
            }

            #region API Order

            public string Id => _info.Id;
            public string Symbol => _info.Symbol;
            public double RequestedVolume => (double)_info.RequestedAmount / _lotSize;
            public double RemainingVolume => (double)_info.RemainingAmount / _lotSize;
            public double MaxVisibleVolume => _info.MaxVisibleAmount / _lotSize ?? double.NaN;
            public Domain.OrderInfo.Types.Type Type => _info.Type;
            OrderType Order.Type => _info.Type.ToApiEnum();
            OrderSide Order.Side => _info.Side.ToApiEnum();
            public Domain.OrderInfo.Types.Side Side => _info.Side;
            public double Price => _info.Price ?? double.NaN;
            public double StopPrice => _info.StopPrice ?? double.NaN;
            public double StopLoss => _info.StopLoss ?? double.NaN;
            public double Slippage => _info.Slippage ?? double.NaN;
            public double TakeProfit => _info.TakeProfit ?? double.NaN;
            public bool IsNull => false;
            public string Comment => _info.Comment;
            public string Tag => _info.UserTag;
            public string InstanceId => _info.InstanceId;
            public DateTime Expiration => _info.Expiration?.ToDateTime().ToLocalTime() ?? DateTime.MinValue;
            public DateTime Modified => _info.Modified?.ToDateTime().ToLocalTime() ?? DateTime.MinValue;
            public DateTime Created => _info.Created?.ToDateTime().ToLocalTime() ?? DateTime.MinValue;
            public double ExecPrice => _info.ExecPrice ?? double.NaN;
            public double ExecVolume => _info.ExecAmount / _lotSize ?? double.NaN;
            public double LastFillPrice => _info.LastFillPrice ?? double.NaN;
            public double LastFillVolume => _info.LastFillAmount / _lotSize ?? double.NaN;
            public double Margin => _accessor.CalculateMargin();
            public double Profit => _accessor.CalculateProfit();
            public Api.OrderOptions Options => _info.Options.ToApiEnum();

            #endregion

            #region IOrderCalcInfo
            double? IOrderCalcInfo.StopPrice => _info.StopPrice;
            decimal? IOrderCalcInfo.Commission => (decimal)_info.Commission;
            decimal? IOrderCalcInfo.Swap => (decimal)_info.Swap;
            bool IMarginProfitCalc.IsHidden => _info.IsHidden();

            decimal IMarginProfitCalc.RemainingAmount => (decimal)_info.RemainingAmount;

            #endregion
        }

        internal class WriteEntity : Order, IOrderCalcInfo
        {
            private readonly OrderAccessor _accessor;
            private readonly double _lotSize;

            public WriteEntity(OrderAccessor accessor)
            {
                _accessor = accessor;
                _lotSize = _accessor._lotSize;
            }

            internal WriteEntity Clone(OrderAccessor newAccessor)
            {
                var clone = new WriteEntity(newAccessor)
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
                    Expiration = Expiration,
                    Created = Created,
                    Modified = Modified,
                    ExecPrice = ExecPrice,
                    ExecAmount = ExecAmount,
                    LastFillPrice = LastFillPrice,
                    LastFillAmount = LastFillAmount,
                    Options = Options,
                    Swap = Swap,
                    Commission = Commission,
                    ActionNo = ActionNo,
                    InitialType = InitialType,
                    ClosePrice = ClosePrice,
                    PositionCreated = PositionCreated,
                    OpenConversionRate = OpenConversionRate,
                };
                return clone;
            }

            internal OrderInfo GetInfo()
            {
                return new OrderInfo
                {
                    Id = Id,
                    Symbol = Symbol,
                    RequestedAmount = (double)RequestedAmount,
                    RemainingAmount = (double)RemainingAmount,
                    MaxVisibleAmount = (double?)MaxVisibleAmount,
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
                    Swap = (double)(Swap ?? 0),
                    Commission = (double)(Commission ?? 0),
                    InitialType = InitialType,
                };
            }

            public string Id { get; internal set; }
            public string Symbol { get; internal set; }

            public decimal RequestedAmount { get; internal set; }
            public decimal RemainingAmount { get; internal set; }
            public decimal? MaxVisibleAmount { get; internal set; }

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
            public decimal ExecAmount { get; internal set; }
            public double? LastFillPrice { get; internal set; }
            public decimal LastFillAmount { get; internal set; }

            public Domain.OrderOptions Options { get; internal set; }

            public double Margin => _accessor.CalculateMargin();
            public double Profit => _accessor.CalculateProfit();

            public decimal? Swap { get; internal set; }
            public decimal? Commission { get; internal set; }

            public bool IsPending => Type == OrderInfo.Types.Type.Limit || Type == OrderInfo.Types.Type.StopLimit || Type == OrderInfo.Types.Type.Stop;

            public bool IsHidden => IsHiddenOrder(MaxVisibleAmount);

            public OrderInfo.Types.Type InitialType { get; internal set; }
            public short ActionNo { get; internal set; }
            public double? OpenConversionRate { get; internal set; }
            public double? ClosePrice { get; set; }
            internal DateTime PositionCreated { get; set; }

            #region API Order

            public bool IsNull => false;

            OrderType Order.Type => Type.ToApiEnum();
            OrderSide Order.Side => Side.ToApiEnum();

            double Order.Price => Price ?? double.NaN;
            double Order.StopPrice => Price ?? double.NaN;

            double Order.StopLoss => StopLoss ?? double.NaN;
            double Order.TakeProfit => TakeProfit ?? double.NaN;
            double Order.Slippage => Slippage ?? double.NaN;

            double Order.RequestedVolume => (double)RequestedAmount / _lotSize;
            double Order.RemainingVolume => (double)RemainingAmount / _lotSize;
            double Order.MaxVisibleVolume => (double?)MaxVisibleAmount / _lotSize ?? double.NaN;

            DateTime Order.Created => Created?.ToLocalTime() ?? DateTime.MinValue;
            DateTime Order.Modified => Modified?.ToLocalTime() ?? DateTime.MinValue;
            DateTime Order.Expiration => Expiration?.ToLocalTime() ?? DateTime.MinValue;

            string Order.Tag => UserTag;

            double Order.ExecPrice => ExecPrice ?? double.NaN;
            double Order.ExecVolume => (double)ExecAmount / _lotSize;
            double Order.LastFillPrice => LastFillPrice ?? double.NaN;
            double Order.LastFillVolume => (double)LastFillAmount / _lotSize;

            Api.OrderOptions Order.Options => Options.ToApiEnum();
            #endregion
        }
    }
}
