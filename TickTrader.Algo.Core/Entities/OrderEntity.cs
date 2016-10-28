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

        public OrderEntity(Order src)
        {
            this.Id = src.Id;
            this.ClientOrderId = ((OrderEntity)src).ClientOrderId;
            this.RequestedVolume = src.RequestedVolume;
            this.RemainingVolume = src.RemainingVolume;
            this.Symbol = src.Symbol;
            this.Type = src.Type;
            this.Side = src.Side;
            this.Price = src.Price;
            this.StopLoss = src.StopLoss;
            this.TakeProfit = src.TakeProfit;
            this.Comment = src.Comment;
            this.Created = src.Created;
            this.Modified = src.Modified;
        }

        public string Id { get; private set; }
        public string ClientOrderId { get; set; }
        public double RequestedVolume { get; set; }
        public double RemainingVolume { get; set; }
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double Price { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool IsNull { get { return false; } }

        public static Order Null { get; private set; }
        static OrderEntity() { Null = new NullOrder(); }
    }

    [Serializable]
    public class NullOrder : Order
    {
        public string Id { get { return ""; } }
        public double RequestedVolume { get { return double.NaN; } }
        public double RemainingVolume { get { return double.NaN; } }
        public string Symbol { get { return ""; } }
        public OrderType Type { get { return OrderType.Market; } }
        public OrderSide Side { get { return OrderSide.Buy; } }
        public double Price { get { return double.NaN; } }
        public double StopLoss { get { return double.NaN; } }
        public double TakeProfit { get { return double.NaN; } }
        public string Comment { get { return ""; } }
        public DateTime Created { get { return DateTime.MinValue; } }
        public DateTime Modified { get { return DateTime.MinValue; } }
        public bool IsNull { get { return true; } }
    }
}
