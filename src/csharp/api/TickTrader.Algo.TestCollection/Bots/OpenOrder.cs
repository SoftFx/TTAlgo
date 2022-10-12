using System;
using System.Globalization;
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
        public OtoTriggerOpenType OtoTriggerType { get; set; }

        [Parameter(DisplayName = "OtoTriggerTime", DefaultValue = "2022/01/01 00:00:00.000", IsRequired = false)]
        public string OtoTriggerTime { get; set; }

        [Parameter]
        public string OtoTriggeredById { get; set; }

        [Parameter(DefaultValue = true)]
        public bool ExitAfterOpen { get; set; }


        protected override void OnStart()
        {
            ContingentOrderTrigger otoTrigger = null;

            if (OtoTriggerType != OtoTriggerOpenType.None)
            {
                DateTime? otoTime = null;

                if (!string.IsNullOrEmpty(OtoTriggerTime))
                {
                    otoTime = DateTime.ParseExact(OtoTriggerTime, "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    otoTime = DateTime.SpecifyKind(otoTime.Value, DateTimeKind.Utc);

                    //otoTime = otoTime.Value.AddMilliseconds(-otoTime.Value.Millisecond);
                }

                otoTrigger = ContingentOrderTrigger.Create(Convert(OtoTriggerType))
                                                   .WithTriggerTime(otoTime)
                                                   .WithOrderIdTriggeredBy(OtoTriggeredById);
            }

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

            if (ExitAfterOpen)
                Exit();
        }

        public static ContingentOrderTrigger.TriggerType Convert(OtoTriggerOpenType type)
        {
            switch (type)
            {
                case OtoTriggerOpenType.OnPendingOrderExpired:
                    return ContingentOrderTrigger.TriggerType.OnPendingOrderExpired;
                case OtoTriggerOpenType.OnPendingOrderPartiallyFilled:
                    return ContingentOrderTrigger.TriggerType.OnPendingOrderPartiallyFilled;
                case OtoTriggerOpenType.OnTime:
                    return ContingentOrderTrigger.TriggerType.OnTime;

                default:
                    throw new Exception($"Unsupported type: {type}");
            }
        }

        public enum OtoTriggerOpenType
        {
            None,
            OnPendingOrderExpired,
            OnPendingOrderPartiallyFilled,
            OnTime,
        }
    }
}