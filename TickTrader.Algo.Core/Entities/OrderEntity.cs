using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class OrderEntity : Order
    {
        public OrderEntity(long orderId)
        {
            this.Id = orderId;
        }

        public long Id { get; private set; }
        public decimal RemainingAmount { get; set; }
        public string Symbol { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderTypes Type { get; set; }
    }
}
