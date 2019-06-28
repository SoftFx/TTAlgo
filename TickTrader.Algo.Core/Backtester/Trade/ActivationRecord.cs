using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal class ActivationRecord
    {
        public ActivationRecord(OrderAccessor order, ActivationTypes type)
        {
            this.Order = order;
            this.ActivationType = type;
            Price = GetActivationPrice(order, type);
        }

        public static double GetActivationPrice(OrderAccessor order, ActivationTypes type)
        {
            if (type == ActivationTypes.Pending)
            {
                if ((order.Type == OrderType.Stop) || (order.Type == OrderType.StopLimit))
                    return order.StopPrice;
                else
                    return order.Price;
            }
            else if (type == ActivationTypes.TakeProfit)
                return order.TakeProfit;
            else if (type == ActivationTypes.StopLoss)
                return order.StopLoss;
            else
                throw new ArgumentException("Invalid activation type:" + type);
        }

        public OrderAccessor Order { get; private set; }
        public ActivationTypes ActivationType { get; private set; }
        public double ActivationPrice { get; set; }
        public double Price { get; private set; }
        public string OrderId { get { return Order.Id; } }

        public DateTime? LastNotifyTime { get; set; }
    }
}
