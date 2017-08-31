using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Modify Order Script", Version = "1.0", Category = "Test Orders",
        Description = "Modify Order by OrderId" +
                      "Prints order execution result to bot status window. ")]
    public class ModifyOrder : TradeBotCommon
    {
        [Parameter(DefaultValue = "")]
        public string OrderId { get; set; }
        [Parameter(DefaultValue = 0.0)]
        public double Price { get; set; }
        [Parameter(DefaultValue = 0.0)]
        public double StopPrice { get; set; }
        [Parameter(DefaultValue = -1.0)]
        public double MaxVisibleVolume { get; set; }
        [Parameter(DefaultValue = "")]
        public string Comment { get; set; }
        [Parameter(DefaultValue = "")]
        public string Tag { get; set; }
        [Parameter(DisplayName = "Stop Loss", DefaultValue = 0.0, IsRequired = false)]
        public double StopLoss { get; set; }
        [Parameter(DisplayName = "Take Profit", DefaultValue = 0.0, IsRequired = false)]
        public double TakeProfit { get; set; }

        protected override void OnStart()
        {
            var price = Price == 0 ? default(double?) : Price;
            var stopPrice = StopPrice == 0 ? default(double?) : StopPrice;
            var tag = string.IsNullOrWhiteSpace(Tag) ? null : Tag;
            var comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment;
            var sl = StopLoss.Gt(Symbol.Point) ? StopLoss : default(double?);
            var tp = TakeProfit.Gt(Symbol.Point) ? TakeProfit : default(double?);
            var maxVisibleVolume = MaxVisibleVolume < 0 ? default(double?) : MaxVisibleVolume;

            var result = ModifyOrder(OrderId, price, stopPrice, maxVisibleVolume, sl, tp, comment);
            Status.WriteLine($"ResultCode = {result.ResultCode}");
            if (result.ResultingOrder != null)
                Status.WriteLine(ToObjectPropertiesString(typeof(Order), result.ResultingOrder));

            Exit();
        }
    }
}