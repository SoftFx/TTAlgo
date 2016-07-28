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
        public OrderVolume TotalAmount { get; set; }
        public OrderVolume RemainingAmount { get; set; }
        public string Symbol { get; set; }
        public OrderTypes Type { get; set; }

        public static Order Null { get; private set; }
        static OrderEntity() { Null = new NullOrder(); }
    }

    public class NullOrder : Order
    {
        public long Id { get { return -1; } }
        public OrderVolume TotalAmount { get { return new OrderVolume(double.NaN, VolumeUnits.CurrencyUnits); } }
        public OrderVolume RemainingAmount { get { return new OrderVolume(double.NaN, VolumeUnits.CurrencyUnits); } }
        public string Symbol { get { return ""; } }
        public OrderTypes Type { get { return OrderTypes.Market; } }
    }
}
