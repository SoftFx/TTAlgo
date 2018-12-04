using System;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessLogic;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.Core
{
    public class OrderAccessor : Order, BL.IOrderModel, BO.IOrder
    {
        private OrderEntity _entity;
        private SymbolAccessor _symbol;
        private double _lotSize;
        private decimal? _profit;
        private decimal? _margin;

        internal OrderAccessor(OrderEntity entity, Func<string, SymbolAccessor> symbolProvider)
            : this(entity, symbolProvider(entity.Symbol))
        {
        }

        internal OrderAccessor(OrderEntity entity, SymbolAccessor symbol)
        {
            _entity = entity ?? throw new ArgumentNullException("entity");

            _symbol = symbol;
            _lotSize = symbol?.ContractSize ?? 1;
        }

        internal static Order GetAccessor(OrderEntity entity, SymbolAccessor symbol)
        {
            if (entity == null)
                return Null.Order;

            return new OrderAccessor(entity, symbol);
        }

        internal void Update(OrderEntity entity)
        {
            _entity = entity;
            EssentialParametersChanged?.Invoke(this);
        }

        public OrderEntity Entity => _entity;

        public OrderAccessor Clone()
        {
            return new OrderAccessor(new OrderEntity(_entity), _symbol);
        }

        #region API Order

        public string Id => _entity.Id;
        public string Symbol => _entity.Symbol;
        public double RequestedVolume => _entity.RequestedVolume / _lotSize ?? double.NaN;
        public double RemainingVolume => _entity.RemainingVolume / _lotSize;
        public double MaxVisibleVolume => _entity.MaxVisibleVolume / _lotSize ?? double.NaN;
        public OrderType Type => _entity.Type;
        public OrderSide Side => _entity.Side;
        public double Price => _entity.Price ?? double.NaN;
        public double StopPrice => _entity.StopPrice ?? double.NaN;
        public double StopLoss => _entity.StopLoss ?? double.NaN;
        public double TakeProfit => _entity.TakeProfit ?? double.NaN;
        public bool IsNull => false;
        public string Comment => _entity.Comment;
        public string Tag => _entity.UserTag;
        public string InstanceId => _entity.InstanceId;
        public DateTime Expiration => _entity.Expiration?? DateTime.MinValue;
        public DateTime Modified => _entity.Modified ?? DateTime.MinValue;
        public DateTime Created => _entity.Created ?? DateTime.MinValue;
        public double ExecPrice => _entity.ExecPrice ?? double.NaN;
        public double ExecVolume => _entity.ExecVolume / _lotSize ?? double.NaN;
        public double LastFillPrice => _entity.LastFillPrice ?? double.NaN;
        public double LastFillVolume => _entity.LastFillVolume / _lotSize ?? double.NaN;
        public double Margin => (double)(_margin ?? 0);
        public double Profit => (double)(_profit ?? 0);

        public bool IsPending => Type == OrderType.Limit || Type == OrderType.StopLimit || Type == OrderType.Stop;

        #endregion

        #region BL IOrderModel

        public decimal? AgentCommision => 0;

        public BL.OrderError CalculationError { get; set; }
        public BL.OrderCalculator Calculator { get; set; }
        public bool IsCalculated => CalculationError == null;
        public decimal? MarginRateCurrent { get; set; }
        public decimal? Swap => (decimal)_entity.Swap;
        public decimal? Commission => (decimal)_entity.Commission;
        public decimal? CurrentPrice { get; set; }
        public long OrderId => long.Parse(Id);
        public decimal Amount { get => (decimal)_entity.RequestedVolume; set => _entity.RequestedVolume = (double)value; }
        public decimal RemainingAmount { get => (decimal)_entity.RemainingVolume; set => _entity.RemainingVolume = (double)value; }
        decimal? BL.IOrderModel.Profit { get => _profit; set => _profit = value; }
        decimal? BL.IOrderModel.Margin { get => (decimal)Margin; set => _margin = value; }
        BO.OrderTypes BL.ICommonOrder.Type { get => _entity.GetBlOrderType(); set => throw new NotImplementedException(); }
        BO.OrderSides BL.ICommonOrder.Side { get => _entity.GetBlOrderSide(); set => throw new NotImplementedException(); }
        decimal? BL.ICommonOrder.Price { get => (decimal?)_entity.Price; set => throw new NotImplementedException(); }
        decimal? BL.ICommonOrder.StopPrice { get => (decimal?)_entity.StopPrice; set => throw new NotImplementedException(); }
        bool BL.ICommonOrder.IsHidden => !double.IsNaN(MaxVisibleVolume) && MaxVisibleVolume.E(0);
        bool BL.ICommonOrder.IsIceberg => !double.IsNaN(MaxVisibleVolume) && MaxVisibleVolume.Gt(0);
        string BL.ICommonOrder.MarginCurrency { get => _symbol?.BaseCurrency; set => throw new NotImplementedException(); }
        string BL.ICommonOrder.ProfitCurrency { get => _symbol?.CounterCurrency; set => throw new NotImplementedException(); }

        public event Action<BL.IOrderModel> EssentialParametersChanged;

        #endregion

        private decimal? GetDecPrice()
        {
            double? price = (Type == OrderType.Stop) ? _entity.StopPrice : _entity.Price;
            return (decimal?)price;
        }

        internal bool HasOption(OrderExecOptions option)
        {
            return Entity.Options.HasFlag(option);
        }

        #region Emulation

        internal short ActionNo { get; set; }
        internal OrderType InitialType { get; set; }
        internal decimal? OpenConversionRate { get;  set; }
        internal SymbolAccessor SymbolInfo => _symbol;
        public decimal? ClosePrice { get; set; }
        internal DateTime PositionCreated { get; set; }

        internal void FireChanged()
        {
            EssentialParametersChanged?.Invoke(this);
        }

        #endregion

        #region TickTrader.BusinessObjects.IOrder

        int BO.IOrder.RangeId => throw new NotImplementedException();
        long BO.IOrder.AccountId => throw new NotImplementedException();
        //string BO.IOrder.Symbol => throw new NotImplementedException();
        string BO.IOrder.SymbolAlias => Symbol;
        //long BO.IOrder.OrderId => throw new NotImplementedException();
        string BO.IOrder.ClientOrderId => null;
        long? BO.IOrder.ParentOrderId => throw new NotImplementedException();
        decimal? BO.IOrder.Price => (decimal?)Entity.Price;
        decimal? BO.IOrder.StopPrice => (decimal?)Entity.StopPrice;
        BO.OrderSides BO.IOrder.Side => _entity.GetBlOrderSide();
        BO.OrderTypes BO.IOrder.Type => _entity.GetBlOrderType();
        BO.OrderTypes BO.IOrder.InitialType => throw new NotImplementedException();
        BO.OrderStatuses BO.IOrder.Status => throw new NotImplementedException();
        //decimal BO.IOrder.Amount => throw new NotImplementedException();
        //decimal BO.IOrder.RemainingAmount => throw new NotImplementedException();
        decimal BO.IOrder.HiddenAmount => throw new NotImplementedException();
        decimal? BO.IOrder.MaxVisibleAmount => throw new NotImplementedException();
        DateTime BO.IOrder.Created => Entity.Created ?? DateTime.MinValue;
        DateTime? BO.IOrder.Modified => Entity.Modified;
        DateTime? BO.IOrder.Filled => throw new NotImplementedException();
        DateTime? BO.IOrder.PositionCreated => throw new NotImplementedException();
        decimal? BO.IOrder.StopLoss => (decimal?)Entity.StopLoss;
        decimal? BO.IOrder.TakeProfit => (decimal?)Entity.TakeProfit;
        decimal? BO.IOrder.Profit => _profit;
        decimal? BO.IOrder.Margin => _margin;
        decimal BO.IOrder.AggrFillPrice => throw new NotImplementedException();
        decimal BO.IOrder.AverageFillPrice => throw new NotImplementedException();
        decimal? BO.IOrder.TransferringCoefficient => throw new NotImplementedException();
        string BO.IOrder.UserComment => Comment;
        string BO.IOrder.ManagerComment => throw new NotImplementedException();
        string BO.IOrder.UserTag => Entity.UserTag;
        string BO.IOrder.ManagerTag => throw new NotImplementedException();
        int BO.IOrder.Magic => throw new NotImplementedException();
        decimal? BO.IOrder.Commission => (decimal)Entity.Commission;
        decimal? BO.IOrder.AgentCommision => throw new NotImplementedException();
        decimal? BO.IOrder.Swap => (decimal)Entity.Swap;
        DateTime? BO.IOrder.Expired => Entity.Expiration;
        //decimal? BO.IOrder.ClosePrice => throw new NotImplementedException();
        //decimal? BO.IOrder.CurrentPrice => throw new NotImplementedException();
        decimal? BO.IOrder.MarginRateInitial => throw new NotImplementedException();
        decimal? BO.IOrder.MarginRateCurrent => throw new NotImplementedException();
        BO.ActivationTypes BO.IOrder.Activation => throw new NotImplementedException();
        decimal? BO.IOrder.OpenConversionRate => throw new NotImplementedException();
        decimal? BO.IOrder.CloseConversionRate => throw new NotImplementedException();
        bool BO.IOrder.IsReducedOpenCommission => throw new NotImplementedException();
        bool BO.IOrder.IsReducedCloseCommission => throw new NotImplementedException();
        int BO.IOrder.Version => throw new NotImplementedException();
        BO.OrderExecutionOptions BO.IOrder.Options => throw new NotImplementedException();
        BO.CustomProperties BO.IOrder.Properties => throw new NotImplementedException();
        decimal? BO.IOrder.Taxes => throw new NotImplementedException();
        decimal? BO.IOrder.ReqOpenPrice => throw new NotImplementedException();
        decimal? BO.IOrder.ReqOpenAmount => throw new NotImplementedException();
        string BO.IOrder.ClientApp => throw new NotImplementedException();
        int? BO.IOrder.SymbolPrecision => _symbol?.Digits;

        internal bool IsSameOrder(OrderAccessor order)
        {
            return (order != null && OrderId == order.OrderId && Type == order.Type);
        }

        #endregion

        public override string ToString()
        {
            return $"#{Id} {Symbol} {Side} {RemainingAmount}";
        }
    }
}
