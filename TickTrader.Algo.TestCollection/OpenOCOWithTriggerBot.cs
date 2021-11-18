using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection
{
    [TradeBot(DisplayName = "[T] Open OCO With Trigger Script", Version = "2.8", Category = "Test Orders")]
    sealed class OpenOCOWithTriggerBot : TradeBot
    {

        protected async override void Init()
        {
            var utcTime = DateTime.UtcNow;
            var otoTrigger = ContingentOrderTrigger.Create(utcTime.AddDays(1).AddMilliseconds(-utcTime.Millisecond));

            var openMainOrderRequest = OpenOrderRequest.Template.Create().WithParams(Symbol.Name, OrderSide.Buy, OrderType.Limit, 1, 1, null).WithContingentOrderTrigger(otoTrigger);
            var openOCOOrderRequest = OpenOrderRequest.Template.Create().WithParams(Symbol.Name, OrderSide.Buy, OrderType.Stop, 1, null, 2).WithContingentOrderTrigger(otoTrigger);

            var res = await OpenOrderAsync(openMainOrderRequest.MakeRequest());

            openOCOOrderRequest.WithOCORelatedOrderId(res.ResultingOrder.Id);

            await OpenOrderAsync(openOCOOrderRequest.MakeRequest());

            Exit();
        }
    }
}
