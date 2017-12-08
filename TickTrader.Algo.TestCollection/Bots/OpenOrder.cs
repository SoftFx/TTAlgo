using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Order Script", Version = "2.6", Category = "Test Orders",
        Description = "Opens order for current chart symbol with specified volume, price, side, type, options, tag, SL, TP, expiration. " +
                      "Prints order execution result to bot status window.")]
    public class OpenOrder : TradeBotCommon
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = null)]
        public double? MaxVisibleVolume { get; set; }

        [Parameter(DefaultValue = null)]
        public double? Price { get; set; }

        [Parameter(DefaultValue = null)]
        public double? StopPrice { get; set; }

        [Parameter(DefaultValue = OrderSide.Buy)]
        public OrderSide Side { get; set; }

        [Parameter(DefaultValue = OrderType.Limit)]
        public OrderType Type { get; set; }

        [Parameter(DefaultValue = OrderExecOptions.None)]
        public OrderExecOptions Options { get; set; }

        [Parameter(DefaultValue = "OpenOrderBot0")]
        public string Tag { get; set; }

        [Parameter(DisplayName = "Stop Loss", DefaultValue = null, IsRequired = false)]
        public double? StopLoss { get; set; }

        [Parameter(DisplayName = "Take Profit", DefaultValue = null, IsRequired = false)]
        public double? TakeProfit { get; set; }

        [Parameter(DisplayName = "Expiration Timeout(ms)", DefaultValue = null, IsRequired = false)]
        public int? ExpirationTimeout { get; set; }

        protected override void OnStart()
        {
            var res = OpenOrder(Symbol.Name, Type, Side, Volume, MaxVisibleVolume, Price, StopPrice, StopLoss, TakeProfit, "Open Order Bot " + DateTime.Now, Options, Tag,
                ExpirationTimeout.HasValue ? DateTime.Now + TimeSpan.FromMilliseconds(ExpirationTimeout.Value) : (DateTime?)null);
            Status.WriteLine($"ResultCode = {res.ResultCode}");
            if (res.ResultingOrder != null)
                Status.WriteLine(ToObjectPropertiesString(typeof(Order), res.ResultingOrder));

            Exit();
        }
    }
}