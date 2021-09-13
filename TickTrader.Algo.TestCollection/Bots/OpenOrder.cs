using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Order Script", Version = "2.8", Category = "Test Orders",
        Description = "Opens order for current chart symbol with specified volume, price, side, stopPrice, type, options, tag, SL," +
        " TP, expiration, slippage, comment, maxVisibleVolume. Prints order execution result to bot status window.")]
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

        [Parameter(DisplayName = "Slippage (fraction)", DefaultValue = null, IsRequired = false)]
        public double? Slippage { get; set; }

        [Parameter(DisplayName = "Take Profit", DefaultValue = null, IsRequired = false)]
        public double? TakeProfit { get; set; }

        [Parameter(DisplayName = "Expiration Timeout(ms)", DefaultValue = null, IsRequired = false)]
        public int? ExpirationTimeout { get; set; }

        [Parameter(DefaultValue = "Open Order Bot Comment")]
        public string Comment { get; set; }


        [Parameter]
        public bool OcoEqualVolume { get; set; }

        [Parameter]
        public string OcoRelatedOrderId { get; set; }


        [Parameter]
        public ContingentOrderTrigger.TriggerType OtoTriggerType { get; set; }

        [Parameter(DisplayName = "OtoTriggerTime Timeout(s)", DefaultValue = null, IsRequired = false)]
        public int? OtoTriggerTimeTimeout { get; set; }

        [Parameter]
        public string OtoTriggeredById { get; set; }


        protected override void OnStart()
        {
            var otoTrigger = ContingentOrderTrigger.Create(OtoTriggerType)
                .WithTriggerTime(OtoTriggerTimeTimeout.HasValue ? DateTime.Now + TimeSpan.FromSeconds(OtoTriggerTimeTimeout.Value) : (DateTime?)null)
                .WithOrderIdTriggeredBy(OtoTriggeredById);

            var request = OpenOrderRequest.Template.Create()
                .WithSymbol(Symbol.Name).WithStopPrice(StopPrice).WithStopLoss(StopLoss)
                .WithSide(Side).WithType(Type).WithVolume(Volume).WithPrice(Price)
                .WithMaxVisibleVolume(MaxVisibleVolume).WithTakeProfit(TakeProfit)
                .WithComment(Comment).WithOptions(Options).WithTag(Tag)
                .WithExpiration(ExpirationTimeout.HasValue ? DateTime.Now + TimeSpan.FromMilliseconds(ExpirationTimeout.Value) : (DateTime?)null)
                .WithSlippage(Slippage).WithOCOEqualVolume(OcoEqualVolume).WithOCORelatedOrderId(OcoRelatedOrderId)
                .WithContingentOrderTrigger(otoTrigger).MakeRequest();

            var res = OpenOrder(request);

            Status.WriteLine($"ResultCode = {res.ResultCode}");

            if (res.ResultingOrder != null)
                Status.WriteLine(ToObjectPropertiesString(res.ResultingOrder));

            Exit();
        }
    }
}