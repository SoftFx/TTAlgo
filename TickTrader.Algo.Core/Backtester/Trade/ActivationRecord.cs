using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    public enum ActivationType
    {
        StopoutRollback = -4,
        PendingRollback = -3,
        TakeProfitRollback = -2,
        StopLossRollback = -1,
        None = 0,
        StopLoss = 1,
        TakeProfit = 2,
        Pending = 3,
        Stopout = 4
    }

    public class ActivationRecord
    {
        public ActivationRecord(OrderAccessor order, ActivationType type)
        {
            this.Order = order;
            this.ActivationType = type;
            Price = GetActivationPrice(order, type);
        }

        public static double GetActivationPrice(OrderAccessor order, ActivationType type)
        {
            if (type == ActivationType.Pending)
            {
                if ((order.Type == Domain.OrderInfo.Types.Type.Stop) || (order.Type == Domain.OrderInfo.Types.Type.StopLimit))
                    return order.StopPrice;
                else
                    return order.Price;
            }
            else if (type == ActivationType.TakeProfit)
                return order.TakeProfit;
            else if (type == ActivationType.StopLoss)
                return order.StopLoss;
            else
                throw new ArgumentException("Invalid activation type:" + type);
        }

        public OrderAccessor Order { get; private set; }
        public ActivationType ActivationType { get; private set; }
        public double ActivationPrice { get; set; }
        public double Price { get; private set; }
        public string OrderId { get { return Order.Id; } }

        public DateTime? LastNotifyTime { get; set; }
    }
}
