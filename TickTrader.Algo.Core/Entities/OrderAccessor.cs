using System;
using TickTrader.Algo.Api;
using BO = TickTrader.BusinessObjects;
using BL = TickTrader.BusinessLogic;

namespace TickTrader.Algo.Core
{
    public class OrderAccessor : Order, BL.IOrderModel
    {
        private OrderEntity _entity;

        internal OrderAccessor(OrderEntity entity)
        {
            _entity = entity;
            Margin = 0;
            Profit = 0;
        }

        internal void Update(OrderEntity entity)
        {
            _entity = entity;
            EssentialParametersChanged?.Invoke(this);
        }

        public OrderEntity Entity => _entity;

        public OrderAccessor Clone()
        {
            return new OrderAccessor(new OrderEntity(_entity));
        }

        #region API Order

        public string Id => _entity.Id;
        public string Symbol => _entity.Symbol;
        public double RequestedVolume => _entity.RequestedVolume.Lots;
        public double RemainingVolume => _entity.RemainingVolume.Lots;
        public double MaxVisibleVolume => _entity.MaxVisibleVolume.Lots;
        public OrderType Type => _entity.Type;
        public OrderSide Side => _entity.Side;
        public double Price => _entity.Price;
        public double StopPrice => _entity.StopPrice;
        public double StopLoss => _entity.StopLoss;
        public double TakeProfit => _entity.TakeProfit;
        public bool IsNull => false;
        public string Comment => _entity.Comment;
        public string Tag => _entity.UserTag;
        public string InstanceId => _entity.InstanceId;
        public DateTime Modified => _entity.Modified;
        public DateTime Created => _entity.Created;
        public double ExecPrice => _entity.ExecPrice;
        public double ExecVolume => _entity.ExecVolume.Lots;
        public double LastFillPrice => _entity.LastFillPrice;
        public double LastFillVolume => _entity.LastFillVolume.Lots;
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
        public decimal Amount { get => (decimal)RequestedVolume; set => throw new NotImplementedException(); }
        public decimal RemainingAmount { get => (decimal)_entity.RemainingVolume.Units; set => throw new NotImplementedException(); }
        decimal? BL.IOrderModel.Profit { get => (decimal)Profit; set => Profit = (double)value; }
        decimal? BL.IOrderModel.Margin { get => (decimal)Margin; set => Margin = (double)value; }
        BO.OrderTypes BL.ICommonOrder.Type { get => _entity.GetBlOrderType(); set => throw new NotImplementedException(); }
        BO.OrderSides BL.ICommonOrder.Side { get => _entity.GetBlOrderSide(); set => throw new NotImplementedException(); }
        decimal? BL.ICommonOrder.Price { get => (decimal)((Type == OrderType.Stop || Type == OrderType.StopLimit) ? StopPrice :  Price); set => throw new NotImplementedException(); }

        public event Action<BL.IOrderModel> EssentialParametersChanged;

        #endregion
    }
}
