using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection
{
    [TradeBot(DisplayName = "[T] Open OCO With Trigger Script", Version = "1.0", Category = "Test Orders")]
    sealed class OpenOCOWithTriggerBot : TradeBot
    {

        protected async override void Init()
        {
            var utcTime = DateTime.UtcNow;
            var otoTrigger = ContingentOrderTrigger.Create(utcTime.AddDays(1));

            //otoTrigger = ContingentOrderTrigger.Create(new DateTime(2021, 11, 19, 11, 03, 48, 0, DateTimeKind.Utc));

            var openMainOrderRequest = OpenOrderRequest.Template.Create().WithParams(Symbol.Name, OrderSide.Buy, OrderType.Limit, 0.1, 1, null).WithContingentOrderTrigger(otoTrigger);
            var openOCOOrderRequest = OpenOrderRequest.Template.Create().WithParams(Symbol.Name, OrderSide.Buy, OrderType.Stop, 0.05, null, 2);

            var res = await OpenOrderAsync(openMainOrderRequest.MakeRequest());

            openOCOOrderRequest.WithOptions(OrderExecOptions.OneCancelsTheOther);
            openOCOOrderRequest.WithOCORelatedOrderId(res.ResultingOrder.Id);
            openOCOOrderRequest.WithOCOEqualVolume(true);

            await Task.Delay(20);

            await OpenOrderAsync(openOCOOrderRequest.WithContingentOrderTrigger(otoTrigger).MakeRequest());

            Exit();
        }
    }
}
