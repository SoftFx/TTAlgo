using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Margin = double.NaN;
            Profit = double.NaN;
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
        public double RequestedVolume => _entity.RequestedVolume;
        public double RemainingVolume => _entity.RemainingVolume;
        public OrderType Type => _entity.Type;
        public OrderSide Side => _entity.Side;
        public double Price => _entity.Price;
        public double StopLoss => _entity.StopLoss;
        public double TakeProfit => _entity.TakeProfit;
        public bool IsNull => false;
        public string Comment => _entity.Comment;
        public string Tag => _entity.Tag;
        public DateTime Modified => _entity.Modified;
        public DateTime Created => _entity.Created;
        public double ExecPrice => _entity.ExecPrice;
        public double ExecVolume => _entity.ExecVolume;
        public double LastFillPrice => _entity.LastFillPrice;
        public double LastFillVolume => _entity.LastFillVolume;
        public double Margin { get; set; }
        public double Profit { get; set; }

        #endregion

        #region BL IOrderModel

        public decimal? AgentCommision => 0;

        public BL.OrderError CalculationError { get; set; }
        public BL.OrderCalculator Calculator { get; set; }
        public bool IsCalculated => CalculationError != null;
        public decimal? MarginRateCurrent { get; set; }
        public decimal? Swap => (decimal)_entity.Swap;
        public decimal? Commission => (decimal)_entity.Commision;
        public decimal? CurrentPrice { get; set; }
        public long OrderId => long.Parse(Id);
        public decimal Amount { get => (decimal)RequestedVolume; set => throw new NotImplementedException(); }
        public decimal RemainingAmount { get => (decimal)_entity.RemainingVolume; set => throw new NotImplementedException(); }
        decimal? BL.IOrderModel.Profit { get => (decimal)Profit; set => Profit = (double)value; }
        decimal? BL.IOrderModel.Margin { get => (decimal)Margin; set => Margin = (double)value; }
        BO.OrderTypes BL.ICommonOrder.Type { get => Convert(_entity.Type); set => throw new NotImplementedException(); }
        BO.OrderSides BL.ICommonOrder.Side { get => Convert(_entity.Side); set => throw new NotImplementedException(); }
        decimal? BL.ICommonOrder.Price { get => (decimal)Price; set => throw new NotImplementedException(); }

        public event Action<BL.IOrderModel> EssentialParametersChanged;

        #endregion

        private static BO.OrderTypes Convert(OrderType apiType)
        {
            switch (apiType)
            {
                case OrderType.Limit: return BO.OrderTypes.Limit;
                case OrderType.StopLimit: return BO.OrderTypes.StopLimit;
                case OrderType.Market: return BO.OrderTypes.Market;
                case OrderType.Position: return BO.OrderTypes.Position;
                case OrderType.Stop: return BO.OrderTypes.Stop;
                default: throw new NotImplementedException("Unknown order type: " + apiType);
            }
        }

        private static BO.OrderSides Convert(OrderSide apiSide)
        {
            switch (apiSide)
            {
                case OrderSide.Buy: return BO.OrderSides.Buy;
                case OrderSide.Sell: return BO.OrderSides.Sell;
                default: throw new NotImplementedException("Unknown order side: " + apiSide);
            }
        }
    }
}
