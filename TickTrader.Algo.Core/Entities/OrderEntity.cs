using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OrderEntity : Order
    {
        private string _userTag;
        private string _tag;

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
        public string Tag
        {
            get { return _tag; }
            set
            {
                _tag = value;

                if (CompositeTag.TryParse(_tag, out CompositeTag compositeTag))
                    _userTag = compositeTag.Tag;
                else
                    _userTag = _tag;
            }
        }
        public string InstanceId { get; set; }
        public bool IsNull { get { return false; } }
        public double ExecPrice { get; set; }
        public double ExecVolume { get; set; }
        public double LastFillPrice { get; set; }
        public double LastFillVolume { get; set; }

        string Order.Tag => _userTag;

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
        public string Tag { get { return ""; } }
        public string InstanceId { get { return ""; } }
        public DateTime Created { get { return DateTime.MinValue; } }
        public DateTime Modified { get { return DateTime.MinValue; } }
        public bool IsNull { get { return true; } }
        public double ExecPrice { get { return double.NaN; } }
        public double ExecVolume { get { return double.NaN; } }
        public double LastFillPrice { get { return double.NaN; } }
        public double LastFillVolume { get { return double.NaN; } }
    }
}
