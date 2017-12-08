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
            ClientOrderId = src.ClientOrderId;
            RequestedVolume = src.RequestedVolume;
            RemainingVolume = src.RemainingVolume;
            MaxVisibleVolume = src.MaxVisibleVolume;
            Symbol = src.Symbol;
            Type = src.Type;
            Side = src.Side;
            Price = src.Price;
            StopPrice = src.StopPrice;
            StopLoss = src.StopLoss;
            TakeProfit = src.TakeProfit;
            Comment = src.Comment;
            Created = src.Created;
            Modified = src.Modified;
            Expiration = src.Expiration;
            UserTag = src.UserTag;
            InstanceId = src.InstanceId;
            ExecPrice = src.ExecPrice;
            ExecVolume = src.ExecVolume;
            LastFillPrice = src.LastFillPrice;
            LastFillVolume = src.LastFillVolume;
            Swap = src.Swap;
            MarginCurrency = src.MarginCurrency;
            ProfitCurrency = src.ProfitCurrency;
            Commission = src.Commission;
        }

        public string Id { get; private set; }
        public string ClientOrderId { get; set; }
        public double? RequestedVolume { get; set; }
        public double RemainingVolume { get; set; }
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double? Price { get; set; }
        public double? StopLoss { get; set; }
        public double? TakeProfit { get; set; }
        public string Comment { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string UserTag { get; set; }
        public string InstanceId { get; set; }
        public bool IsNull { get { return false; } }
        public double? ExecPrice { get; set; }
        public double? ExecVolume { get; set; }
        public double? LastFillPrice { get; set; }
        public double? LastFillVolume { get; set; }
        public double Swap { get; set; }
        public double Commission { get; set; }
        public static Order Null { get; private set; }
        public string MarginCurrency { get; set; }
        public string ProfitCurrency { get; set; }
        public double? StopPrice { get; set; }
        public double? MaxVisibleVolume { get; set; }
        public DateTime? Expiration { get; set; }
        public OrderExecOptions Options { get; set; }
        public bool ImmediateOrCancel => Options.HasFlag(OrderExecOptions.ImmediateOrCancel);

        static OrderEntity() { Null = new NullOrder(); }

        #region FDK compatibility
        public string OrderId => Id;
        public double Volume => RemainingVolume;
        public double? InitialVolume => RequestedVolume;
        #endregion
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

        public override string ToString()
        {
            return $"{Lots} lots ({Units} units)";
        }
    }

    [Serializable]
    public class NullOrder : Order
    {
        public string Id { get { return ""; } }
        public double RequestedVolume { get { return double.NaN; } }
        public double RemainingVolume { get { return double.NaN; } }
        public double MaxVisibleVolume { get { return double.NaN; } }
        public string Symbol { get { return ""; } }
        public OrderType Type { get { return OrderType.Market; } }
        public OrderSide Side { get { return OrderSide.Buy; } }
        public double Price { get { return double.NaN; } }
        public double StopPrice { get { return double.NaN; } }
        public double StopLoss { get { return double.NaN; } }
        public double TakeProfit { get { return double.NaN; } }
        public string Comment { get { return ""; } }
        public string Tag { get { return ""; } }
        public string InstanceId { get { return ""; } }
        public DateTime Created { get { return DateTime.MinValue; } }
        public DateTime Modified { get { return DateTime.MinValue; } }
        public DateTime Expiration { get { return DateTime.MinValue; } }
        public bool IsNull { get { return true; } }
        public double ExecPrice { get { return double.NaN; } }
        public double ExecVolume { get { return double.NaN; } }
        public double LastFillPrice { get { return double.NaN; } }
        public double LastFillVolume { get { return double.NaN; } }
        public double Margin => double.NaN;
        public double Profit => double.NaN;
    }
}
