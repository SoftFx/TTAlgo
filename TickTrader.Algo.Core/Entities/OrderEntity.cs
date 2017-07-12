using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OrderEntity
    {
        public OrderEntity(string orderId)
        {
            Id = orderId;
        }

        public OrderEntity(OrderEntity src)
        {
            Id = src.Id;
            ClientOrderId = ((OrderEntity)src).ClientOrderId;
            RequestedVolume = src.RequestedVolume;
            RemainingVolume = src.RemainingVolume;
            Symbol = src.Symbol;
            Type = src.Type;
            Side = src.Side;
            Price = src.Price;
            StopLoss = src.StopLoss;
            TakeProfit = src.TakeProfit;
            Comment = src.Comment;
            Created = src.Created;
            Modified = src.Modified;
            UserTag = src.UserTag;
            InstanceId = src.InstanceId;
            ExecPrice = src.ExecPrice;
            ExecVolume = src.ExecVolume;
            LastFillPrice = src.LastFillPrice;
            LastFillVolume = src.LastFillVolume;
            Swap = src.Swap;
            Commision = src.Commision;
        }

        public string Id { get; private set; }
        public string ClientOrderId { get; set; }
        public TradeVolume RequestedVolume { get; set; }
        public TradeVolume RemainingVolume { get; set; }
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double Price { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string UserTag { get; set; }
        public string InstanceId { get; set; }
        public bool IsNull { get { return false; } }
        public double ExecPrice { get; set; }
        public TradeVolume ExecVolume { get; set; }
        public double LastFillPrice { get; set; }
        public double LastFillVolume { get; set; }
        public double Swap { get; set; }
        public double Commision { get; set; }
        public static Order Null { get; private set; }
        static OrderEntity() { Null = new NullOrder(); }
    }

    [Serializable]
    public struct TradeVolume
    {
        public TradeVolume(double units, double lots)
        {
            Lots = lots;
            Units = units;
        }

        public double Lots { get; private set; }
        public double Units { get; private set; }
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
        public double Margin => double.NaN;
        public double Profit => double.NaN;
    }
}
