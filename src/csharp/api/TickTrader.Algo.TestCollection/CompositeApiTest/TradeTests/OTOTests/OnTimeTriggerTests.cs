using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TriggerType = TickTrader.Algo.Api.ContingentOrderTrigger.TriggerType;


namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed partial class OTOTests
    {
        private sealed class OnTimeTriggerTests : TriggerTestsBase
        {
            private const int ExecuteTriggerSec = 2;
            private const int WaitTriggerSec = 30;


            internal OnTimeTriggerTests(OTOTests @base) : base(@base) { }


            protected override void UpdateOCOOrders(OrderStateTemplate order, OrderStateTemplate oco)
            {
                order.WithOnTimeTrigger(WaitTriggerSec);
                oco.WithOnTimeTrigger(order.TriggerTime);
            }


            protected override async Task RunOpenTests(OrderBaseSet set)
            {
                if (set.IsInstantOrder)
                    await RunOnTimeTest(OpenInstantOnTime, set, ExecuteTriggerSec);
                else
                    await RunOnTimeTest(OpenPendingOnTime, set, ExecuteTriggerSec);
            }

            protected override async Task RunModifyTests(OrderBaseSet set)
            {
                await RunOnTimeTest(ModifyAllPropertyTest, set);
                //await RunModifyTriggerType(set, TriggerType.OnTime);
            }

            protected override async Task RunCancelTests(OrderBaseSet set)
            {
                async Task OpenCancelOnTime(OrderStateTemplate order)
                {
                    await _base.TestOpenOrder(order);
                    await _base.TestCancelOrder(order);
                }

                Task ActivateWithExpiration(OrderStateTemplate order)
                {
                    return _base.TestOpenOrder(order.WithExpiration(4), Events.Order.PendingOnTimeActivationExpire);
                }

                await RunOnTimeTest(OpenCancelOnTime, set);

                if (set.IsPendingOrder)
                    await RunOnTimeTest(ActivateWithExpiration, set, ExecuteTriggerSec);
            }

            protected override async Task RunOpenRejectTests(OrderBaseSet set)
            {
                await RunOnTimeTest(RejectWithIncorrectTime, set);
                await RunOnTimeTest(RejectWithNullTime, set);

                if (set.IsPendingOrder)
                    await RunOnTimeTest(RejectWithExpirationLessThanOnTime, set);

                if (set.Type == OrderType.Limit)
                    await RunOnTimeTest(RejectWithIncorrectType, new OrderBaseSet(OrderType.StopLimit, set.Side));
            }

            protected override async Task RunModifyRejectTests(OrderBaseSet set)
            {
                await RunOnTimeTest(RejectModifyTriggerTime, set);

                if (set.IsPendingOrder)
                    await RunOnTimeTest(RejectModifyExpiration, set);
            }

            protected override async Task RunOCOTests(OrderBaseSet set)
            {
                var oco = new OrderBaseSet(OrderType.Stop, set.Side);

                await _base.RunOpenOCOTests(set, oco);
                await _base.RunModifyOCOTests(set, oco);

                await RunRejectOCOWithTrigger(set);
            }


            private Task RejectWithIncorrectTime(OrderStateTemplate order)
            {
                return _base.TestOpenReject(order.WithOnTimeTrigger(-WaitTriggerSec), OrderCmdResultCodes.IncorrectTriggerTime);
            }

            private Task RejectWithNullTime(OrderStateTemplate order)
            {
                order.TriggerTime = null;

                return _base.TestOpenReject(order, OrderCmdResultCodes.IncorrectTriggerTime);
            }

            private Task RejectWithExpirationLessThanOnTime(OrderStateTemplate order)
            {
                return _base.TestOpenReject(order.WithExpiration(WaitTriggerSec / 3), OrderCmdResultCodes.IncorrectTriggerTime);
            }

            private Task RejectWithIncorrectType(OrderStateTemplate order)
            {
                return _base.TestOpenReject(order, OrderCmdResultCodes.IncorrectTriggerOrderType);
            }


            private async Task RejectModifyTriggerTime(OrderStateTemplate order)
            {
                await _base.TestOpenOrder(order);
                await _base.TestModifyReject(order.WithOnTimeTrigger(DateTime.UtcNow.AddSeconds(-1)), OrderCmdResultCodes.IncorrectTriggerTime);
                await _base.TestCancelOrder(order);
            }

            private async Task RejectModifyExpiration(OrderStateTemplate order)
            {
                await _base.TestOpenOrder(order);
                await _base.TestModifyReject(order.WithExpiration(5), OrderCmdResultCodes.IncorrectTriggerTime);
                await _base.TestCancelOrder(order);
            }

            private async Task RunRejectOCOWithTrigger(OrderBaseSet set)
            {
                var incorrectSet = new OrderBaseSet(OrderType.Market, set.Side);

                await _base.RunOCOTest(OpenOCOWithTriggerAndIncorrectType, set, incorrectSet);
                await _base.RunOCOTest(OpenLinkedOCOWithTriggerAndIncorrectType, set, incorrectSet);
                await _base.RunOCOTest(OpenOCOWithoutTrigger, set, set);
            }


            private async Task OpenOCOWithTriggerAndIncorrectType(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                await _base.TestOpenOrder(mainOrder.WithOnTimeTrigger(WaitTriggerSec));
                await _base.TestOpenReject(ocoOrder.WithOCO(mainOrder).WithOnTimeTrigger(mainOrder.TriggerTime), OrderCmdResultCodes.Unsupported);
                await _base.TestCancelOrder(mainOrder);
            }

            private async Task OpenOCOWithoutTrigger(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                await _base.TestOpenOrder(mainOrder.WithOnTimeTrigger(WaitTriggerSec));
                await _base.TestOpenReject(ocoOrder.WithOCO(mainOrder), OrderCmdResultCodes.OCORelatedOrderIncorrectOptions);
                await _base.TestCancelOrder(mainOrder);
            }

            private Task OpenLinkedOCOWithTriggerAndIncorrectType(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                return _base.TestOpenReject(mainOrder.WithOnTimeTrigger(WaitTriggerSec).WithLinkedOCO(ocoOrder), OrderCmdResultCodes.Unsupported);
            }


            private async Task ModifyAllPropertyTest(OrderStateTemplate order)
            {
                await _base.TestOpenOrder(order);

                if (!order.IsInstantOrder)
                    order.WithExpiration(WaitTriggerSec * 2);

                await _base.TestModifyOrder(order.WithOnTimeTrigger(WaitTriggerSec));

                order.FillAdditionalProperties();
                order.Volume *= 0.9;
                order.Comment = "Modified OnTime trigger comment";

                if (order.WithOnTimeTrigger(ExecuteTriggerSec).IsInstantOrder)
                    await OpenInstantOnTime(order);
                else
                    await OpenPendingOnTime(order.ForPending(5).WithExpiration(WaitTriggerSec));
            }


            private async Task OpenInstantOnTime(OrderStateTemplate order)
            {
                await _base.TestOpenOrder(order, Events.Order.InstantOnTimeActivation);
                await order.OnTimeTriggerReceived.Task;
            }

            private async Task OpenPendingOnTime(OrderStateTemplate order)
            {
                await _base.TestOpenOrder(order, Events.Order.PendingOnTimeActivation);

                await order.OnTimeTriggerReceived.Task;
                await order.Opened.Task;

                await _base.TestCancelOrder(order);
            }

            private Task RunOnTimeTest(TriggerTimeTest test, OrderBaseSet set, int seconds = WaitTriggerSec)
            {
                Task OnTimeTest(OrderStateTemplate order)
                {
                    return test(order.FillAdditionalProperties().WithOnTimeTrigger(seconds));
                }

                return _base.RunTest(OnTimeTest, set, testInfo: $"{test.Method.Name} TriggerType={TriggerType.OnTime} Seconds={seconds}");
            }
        }
    }
}