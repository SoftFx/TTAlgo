using System;
using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
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
                if ((order.Info.Type == Domain.OrderInfo.Types.Type.Stop) || (order.Info.Type == Domain.OrderInfo.Types.Type.StopLimit))
                    return order.Info.StopPrice ?? 0;
                else
                    return order.Info.Price ?? 0;
            }
            else if (type == ActivationType.TakeProfit)
                return order.Info.TakeProfit ?? 0;
            else if (type == ActivationType.StopLoss)
                return order.Info.StopLoss ?? 0;
            else
                throw new ArgumentException("Invalid activation type:" + type);
        }

        public OrderAccessor Order { get; private set; }
        public ActivationType ActivationType { get; private set; }
        public double ActivationPrice { get; set; }
        public double Price { get; private set; }
        public string OrderId { get { return Order.Info.Id; } }

        public DateTime? LastNotifyTime { get; set; }
    }
}
