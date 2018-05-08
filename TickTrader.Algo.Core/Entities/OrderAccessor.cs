using System;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessLogic;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.Core
{
    public class OrderAccessor : Order, BL.IOrderModel
    {
        private OrderEntity _entity;
        private Symbol _symbol;
        private double _lotSize;

        internal OrderAccessor(OrderEntity entity, Func<string, Symbol> symbolProvider)
            : this(entity, symbolProvider(entity.Symbol))
        {
        }

        internal OrderAccessor(OrderEntity entity, Symbol symbol)
        {
            _entity = entity ?? throw new ArgumentNullException("entity");
            Margin = 0;
            Profit = 0;

            _symbol = symbol;
            _lotSize = symbol?.ContractSize ?? 1;
        }

        internal static Order GetAccessor(OrderEntity entity, Symbol symbol)
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
        public double Margin { get; set; }
        public double Profit { get; set; }

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
        public decimal Amount { get => (decimal)_entity.RequestedVolume; set => throw new NotImplementedException(); }
        public decimal RemainingAmount { get => (decimal)_entity.RemainingVolume; set => throw new NotImplementedException(); }
        decimal? BL.IOrderModel.Profit { get => (decimal)Profit; set => Profit = (double)value; }
        decimal? BL.IOrderModel.Margin { get => (decimal)Margin; set => Margin = (double)value; }
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
    }
}
