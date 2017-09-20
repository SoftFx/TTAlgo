using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Modify Order Script", Version = "1.1", Category = "Test Orders",
        Description = "Modify Order by OrderId" +
                      "Prints order execution result to bot status window. ")]
    public class ModifyOrder : TradeBotCommon
    {
        [Parameter(DefaultValue = "")]
        public string OrderId { get; set; }

        [Parameter(DefaultValue = null)]
        public double? Price { get; set; }

        [Parameter(DefaultValue = null)]
        public double? StopPrice { get; set; }

        [Parameter(DefaultValue = null)]
        public double? MaxVisibleVolume { get; set; }

        [Parameter(DefaultValue = "")]
        public string Comment { get; set; }

        [Parameter(DisplayName = "Stop Loss", DefaultValue = null, IsRequired = false)]
        public double? StopLoss { get; set; }

        [Parameter(DisplayName = "Take Profit", DefaultValue = null, IsRequired = false)]
        public double? TakeProfit { get; set; }

        protected override void OnStart()
        {
            var comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment;

            var result = ModifyOrder(OrderId, Price, StopPrice, MaxVisibleVolume, StopLoss, TakeProfit, comment);
            Status.WriteLine($"ResultCode = {result.ResultCode}");
            if (result.ResultingOrder != null)
                Status.WriteLine(ToObjectPropertiesString(typeof(Order), result.ResultingOrder));

            Exit();
        }
    }
}