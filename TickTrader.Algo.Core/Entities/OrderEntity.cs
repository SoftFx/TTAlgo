using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OrderEntity : Order
    {
        public OrderEntity(string orderId)
        {
            this.Id = orderId;
        }

        public string Id { get; private set; }
        public string ClientOrderId { get; set; }
        public double RequestedAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double Price { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public string Comment { get; set; }
        public bool IsNull { get { return false; } }

        public static Order Null { get; private set; }
        static OrderEntity() { Null = new NullOrder(); }
    }

    [Serializable]
    public class NullOrder : Order
    {
        public string Id { get { return ""; } }
        public double RequestedAmount { get { return double.NaN; } }
        public double RemainingAmount { get { return double.NaN; } }
        public string Symbol { get { return ""; } }
        public OrderType Type { get { return OrderType.Market; } }
        public OrderSide Side { get { return OrderSide.Buy; } }
        public double Price { get { return double.NaN; } }
        public double StopLoss { get { return double.NaN; } }
        public double TakeProfit { get { return double.NaN; } }
        public string Comment { get { return ""; } }
        public bool IsNull { get { return true; } }
    }
}
