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
        public double RequestedAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string Symbol { get; set; }
        public OrderTypes Type { get; set; }
        public OrderSides Side { get; set; }
        public double Price { get; set; }

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
        public OrderTypes Type { get { return OrderTypes.Market; } }
        public OrderSides Side { get { return OrderSides.Buy; } }
        public double Price { get { return double.NaN; } }
    }
}
