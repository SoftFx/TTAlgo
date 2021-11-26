using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TriggerType = TickTrader.Algo.Api.ContingentOrderTrigger.TriggerType;


namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class OTOTests : TestGroupBase
    {
        private delegate Task OTOTest(OrderStateTemplate order);
        private delegate Task OCOwithOTOTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder);

        protected override string GroupName => nameof(OTOTests);


        protected override async Task RunTestGroup(OrderBaseSet set)
        {
            await RunOnTimeTriggerTests(set);
        }


        private async Task RunOnTimeTriggerTests(OrderBaseSet set)
        {
            if (set.IsInstantOrder)
                await RunOnTimeTest(OpenInstantOnTime, set, 2);
            else
                await RunOnTimeTest(OpenPendingOnTime, set);

            await RunOnTimeTest(RejectWithIncorrectTime, set, -10);
            await RunOnTimeTest(RejectWithNullTime, set, 10);

            if (!set.IsInstantOrder)
                await RunOnTimeTest(RejectWithExpirationLessThanOnTime, set);

            if (set.Type == OrderType.Limit)
                await RejectWithIncorrectType(set);

            if (set.IsSupportedOCO)
            {
                await RunOpenOCOWithTrigger(set);
                await RunRejectOCOWithTrigger(set);
            }

            await RunOnTimeCancelTests(set);

            await RunOnTimeModifyTests(set);
        }

        private async Task RunOnTimeModifyTests(OrderBaseSet set)
        {
            //await RunOnTimeTest(ModifyOnTimeTest, set, 30); wait fix on TTS side
            await RunOnTimeTest(ModifyPropertyTest, set, 30);

            //await RunOnTimeTest(RejectModifyOnTime, set, 30); wait fix on TTS side

            if (!set.IsInstantOrder)
                await RunOnTimeTest(RejectModifyExpiration, set, 30);

            //if (set.IsSupportedOCO)
            //    await RunOnTimeOCOModifyTests(set); # wait TTs fix

            await RunModifyTriggerType(set, TriggerType.OnTime);
        }

        private async Task RunModifyTriggerType(OrderBaseSet set, TriggerType currentTriggerType)
        {
            //if (currentTriggerType != TriggerType.OnTime)
            //    await ModifyToOnTimeTrigger();

            if (currentTriggerType != TriggerType.OnPendingOrderExpired)
            {
                await RunOnTimeTest(ModifyToPendingOrderExpired, set, 30);
                await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithoutExpiration, set, 30);
                await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithoutTriggerId, set, 30);
                await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithIncorrectid, set, 30);
            }

            if (currentTriggerType != TriggerType.OnPendingOrderPartiallyFilled)
            {
                await RunOnTimeTest(ModifyToPendingOrderPartiallyFilled, set, 30);
                await RunOnTimeTest(RejectModifyToPendingOrderPartiallyFilledWithoutTriggerId, set, 30);
                await RunOnTimeTest(RejectModifyToPendingOrderPartiallyFilledWithIncorrectId, set, 30);
            }
        }

        private async Task ModifyToOnTimeTrigger()
        {

        }

        private async Task ModifyToPendingOrderExpired(OrderStateTemplate order)
        {
            var secondOrder = new OrderBaseSet(OrderType.Limit, order.Side).BuildOrder().ForPending().WithExpiration(60);

            await TestOpenOrder(order);
            await TestOpenOrder(secondOrder);

            order.TriggerType = TriggerType.OnPendingOrderExpired;
            order.OrderIdTriggeredBy = secondOrder.Id;

            await TestModifyOrder(order);
            await TestCancelOrder(order);
            await TestCancelOrder(secondOrder);
        }

        private async Task RejectModifyToPendingOrderExpiredWithoutExpiration(OrderStateTemplate order)
        {
            var secondOrder = new OrderBaseSet(OrderType.Limit, order.Side).BuildOrder().ForPending();

            await TestOpenOrder(order);
            await TestOpenOrder(secondOrder);

            order.TriggerType = TriggerType.OnPendingOrderExpired;
            order.OrderIdTriggeredBy = secondOrder.Id;

            await TestModifyReject(order, OrderCmdResultCodes.IncorrectExpiration);
            await TestCancelOrder(order);
            await TestCancelOrder(secondOrder);
        }

        private async Task RejectModifyToPendingOrderExpiredWithoutTriggerId(OrderStateTemplate order)
        {
            await TestOpenOrder(order);

            order.TriggerType = TriggerType.OnPendingOrderExpired;
            order.OrderIdTriggeredBy = null;

            await TestModifyReject(order, OrderCmdResultCodes.IncorrectTriggerOrderId);
            await TestCancelOrder(order);
        }

        private async Task RejectModifyToPendingOrderExpiredWithIncorrectid(OrderStateTemplate order)
        {
            await TestOpenOrder(order);

            order.TriggerType = TriggerType.OnPendingOrderExpired;
            order.OrderIdTriggeredBy = "0";

            await TestModifyReject(order, OrderCmdResultCodes.OrderNotFound);
            await TestCancelOrder(order);
        }


        private async Task ModifyToPendingOrderPartiallyFilled(OrderStateTemplate order)
        {
            var secondOrder = new OrderBaseSet(OrderType.Limit, order.Side).BuildOrder().ForPending();

            await TestOpenOrder(order);
            await TestOpenOrder(secondOrder);

            order.TriggerType = TriggerType.OnPendingOrderPartiallyFilled;
            order.OrderIdTriggeredBy = secondOrder.Id;

            await TestModifyOrder(order);
            await TestCancelOrder(order);
            await TestCancelOrder(secondOrder);
        }

        private async Task RejectModifyToPendingOrderPartiallyFilledWithoutTriggerId(OrderStateTemplate order)
        {
            await TestOpenOrder(order);

            order.TriggerType = TriggerType.OnPendingOrderPartiallyFilled;
            order.OrderIdTriggeredBy = null;

            await TestModifyReject(order, OrderCmdResultCodes.IncorrectTriggerOrderId);
            await TestCancelOrder(order);
        }

        private async Task RejectModifyToPendingOrderPartiallyFilledWithIncorrectId(OrderStateTemplate order)
        {
            await TestOpenOrder(order);

            order.TriggerType = TriggerType.OnPendingOrderPartiallyFilled;
            order.OrderIdTriggeredBy = "0";

            await TestModifyReject(order, OrderCmdResultCodes.OrderNotFound);
            await TestCancelOrder(order);
        }


        private async Task RunOnTimeOCOModifyTests(OrderBaseSet set)
        {
            var ocoSet = new OrderBaseSet(OrderType.Stop, set.Side);

            await RunOCOTriggerTest(CreateOnTimeOCOLinkViaModify, set, ocoSet, false);
            await RunOCOTriggerTest(CreateOnTimeOCOLinkViaModify, set, ocoSet, true);

            await RunOCOTriggerTest(BreakOnTimeOCOLinkViaModify, set, ocoSet, false);
            await RunOCOTriggerTest(BreakOnTimeOCOLinkViaModify, set, ocoSet, true);
        }

        private async Task CreateOnTimeOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.WithOnTimeTrigger(60));
            await TestOpenOrder(ocoOrder.WithOnTimeTrigger(mainOrder.TriggerTime));

            await TestModifyOrder(mainOrder.WithOCO(ocoOrder), OrderEvents.Modify);
        }

        private async Task BreakOnTimeOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await CreateOnTimeOCOLinkViaModify(mainOrder, ocoOrder);
            await TestModifyOrder(mainOrder.WithRemovedOCO(), OrderEvents.Modify);
            await TestCancelOrder(mainOrder);
            await TestCancelOrder(ocoOrder);
        }

        private async Task RunOnTimeCancelTests(OrderBaseSet set)
        {
            await RunOnTimeTest(OpenCancelOnTime, set, 1000);

            if (!set.IsInstantOrder)
                await RunOnTimeTest(CancelWithExpiration, set);

            if (set.IsSupportedOCO)
                await RunOCOTriggerTest(OpenCancelOnTimeOCO, set, new OrderBaseSet(OrderType.Stop, set.Side));
        }


        private async Task OpenInstantOnTime(OrderStateTemplate order)
        {
            await TestOpenOrder(order, OrderEvents.Cancel, OrderEvents.Open, OrderEvents.Fill);
            await order.OnTimeTriggerReceived.Task;
        }

        private async Task OpenPendingOnTime(OrderStateTemplate order)
        {
            await TestOpenOrder(order, OrderEvents.Cancel, OrderEvents.Open);
            await order.OnTimeTriggerReceived.Task;
            await order.Opened.Task;
            await TestCancelOrder(order);
        }

        private Task CancelWithExpiration(OrderStateTemplate order)
        {
            return TestOpenOrder(order.WithExpiration(8), OrderEvents.Cancel, OrderEvents.Open, OrderEvents.Expire);
        }

        private Task RejectWithIncorrectTime(OrderStateTemplate order)
        {
            return TestOpenReject(order, Api.OrderCmdResultCodes.IncorrectTriggerTime);
        }

        private Task RejectWithNullTime(OrderStateTemplate order)
        {
            order.TriggerTime = null;

            return TestOpenReject(order, Api.OrderCmdResultCodes.IncorrectTriggerTime);
        }

        private Task RejectWithExpirationLessThanOnTime(OrderStateTemplate order)
        {
            return TestOpenReject(order.WithExpiration(4).WithOnTimeTrigger(8), OrderCmdResultCodes.IncorrectTriggerTime);
        }

        private Task RejectWithIncorrectType(OrderBaseSet set)
        {
            Task RejectWithIncorrectType(OrderStateTemplate order)
            {
                return TestOpenReject(order, OrderCmdResultCodes.IncorrectTriggerOrderType);
            }


            set = new OrderBaseSet(OrderType.StopLimit, set.Side);

            return RunOnTimeTest(RejectWithIncorrectType, set, testInfo: nameof(RejectWithIncorrectTime));
        }

        private async Task OpenCancelOnTime(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestCancelOrder(order);
        }

        private async Task OpenCancelOnTimeOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenLinkedOCOWithTimeTrigger(mainOrder, ocoOrder);
            await TestCancelOrder(mainOrder, OrderEvents.Cancel);
            await ocoOrder.Canceled.Task;
        }

        private async Task RunOpenOCOWithTrigger(OrderBaseSet set)
        {
            var ocoSet = new OrderBaseSet(OrderType.Stop, set.Side);

            //await RunOCOTriggerTest(OpenOCOWithTrigger, set, ocoSet, true); # wait fix on server side
            //await RunOCOTriggerTest(OpenOCOWithTrigger, set, ocoSet, false);
            await RunOCOTriggerTest(OpenLinkedOCOWithTimeTrigger, set, ocoSet);
        }

        private async Task RunRejectOCOWithTrigger(OrderBaseSet set)
        {
            var incorrectSet = new OrderBaseSet(OrderType.Market, set.Side);
            var ocoSet = new OrderBaseSet(OrderType.Stop, set.Side);

            await RunOCOTriggerTest(OpenOCOWithTriggerAndIncorrectType, set, incorrectSet);
            await RunOCOTriggerTest(OpenLinkedOCOWithTriggerAndIncorrectType, set, incorrectSet);
            await RunOCOTriggerTest(OpenOCOWithoutTrigger, set, ocoSet);
        }

        private async Task OpenOCOWithTrigger(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.WithOnTimeTrigger(3000));
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder).WithOnTimeTrigger(mainOrder.TriggerTime));
        }

        private Task OpenLinkedOCOWithTimeTrigger(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return TestOpenOrder(mainOrder.WithOnTimeTrigger(30).WithLinkedOCO(ocoOrder), OrderEvents.Open);
        }

        private async Task OpenOCOWithTriggerAndIncorrectType(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.WithOnTimeTrigger(30));
            await TestOpenReject(ocoOrder.WithOCO(mainOrder).WithOnTimeTrigger(mainOrder.TriggerTime), OrderCmdResultCodes.Unsupported);
            await TestCancelOrder(mainOrder);
        }

        private async Task OpenOCOWithoutTrigger(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.WithOnTimeTrigger(30));
            await TestOpenReject(ocoOrder.WithOCO(mainOrder), OrderCmdResultCodes.OCORelatedOrderIncorrectOptions);
            await TestCancelOrder(mainOrder);
        }

        private Task OpenLinkedOCOWithTriggerAndIncorrectType(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return TestOpenReject(mainOrder.WithOnTimeTrigger(30).WithLinkedOCO(ocoOrder), OrderCmdResultCodes.Unsupported);
        }

        private async Task ModifyOnTimeTest(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyOrder(order.WithOnTimeTrigger(60));

            if (order.WithOnTimeTrigger(4).IsInstantOrder)
            {
                await TestModifyOrder(order, OrderEvents.Cancel, OrderEvents.Open, OrderEvents.Fill);
                await order.OnTimeTriggerReceived.Task;
            }
            else
            {
                await TestModifyOrder(order, OrderEvents.Cancel, OrderEvents.Open);
                await order.OnTimeTriggerReceived.Task;
                await order.Opened.Task;
                await TestCancelOrder(order);
            }
        }

        private async Task ModifyPropertyTest(OrderStateTemplate order)
        {
            await TestOpenOrder(order);

            if (!order.IsInstantOrder)
                order.WithExpiration(120);

            await TestModifyOrder(order.WithOnTimeTrigger(60));

            order.Volume *= 0.9;
            order.Comment = "Modified OnTime trigger comment";

            if (order.IsSupportedMaxVisibleVolume)
                order.MaxVisibleVolume = order.Volume * 0.9;

            if (order.IsSupportedSlippage)
                order.Slippage = OrderBaseSet.Symbol.Slippage * 0.9;

            if (order.WithOnTimeTrigger(4).IsInstantOrder)
            {
                await TestModifyOrder(order, OrderEvents.Cancel, OrderEvents.Open, OrderEvents.Fill);
                await order.OnTimeTriggerReceived.Task;
            }
            else
            {
                await TestModifyOrder(order.ForPending(5).WithExpiration(20), OrderEvents.Cancel, OrderEvents.Open);
                await order.OnTimeTriggerReceived.Task;
                await order.Opened.Task;
                await TestCancelOrder(order);
            }
        }

        private async Task RejectModifyTriggerTime(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyReject(order.WithOnTimeTrigger(DateTime.UtcNow.AddSeconds(-1)), OrderCmdResultCodes.IncorrectTriggerTime);
            await TestCancelOrder(order);
        }

        private async Task RejectModifyExpiration(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyReject(order.WithExpiration(5), OrderCmdResultCodes.IncorrectTriggerTime);
            await TestCancelOrder(order);
        }


        private Task RunOCOTriggerTest(OCOwithOTOTest test, OrderBaseSet set, OrderBaseSet ocoSet, bool equalVolume = false, string testInfo = null)
        {
            async Task OCOTestEnviroment(OrderStateTemplate mainOrder)
            {
                var ocoOrder = ocoSet.BuildOrder(newVolume: mainOrder.Volume / 2)
                                     .ForPending(ocoSet.Type == OrderType.Limit ? 4 : 2); //for pair limit/stop price for Buy must be less than Sell

                ocoOrder.OcoEqualVolume = equalVolume;

                await test(mainOrder, ocoOrder);
                await Task.Yield(); //wait modification real Oco and Main order

                if (!string.IsNullOrEmpty(ocoOrder.Id))
                    await RemoveOrder(ocoOrder);
            }

            return RunTest(OCOTestEnviroment, set, testInfo: $"{testInfo ?? test.Method.Name} TriggerType={TriggerType.OnTime} OCO =({ocoSet}) equalVolume={equalVolume}");
        }

        private Task RunOnTimeTest(OTOTest test, OrderBaseSet set, int seconds = 4, string testInfo = null)
        {
            Task OTOTest(OrderStateTemplate order)
            {
                if (order.IsSupportedMaxVisibleVolume)
                    order.MaxVisibleVolume = order.Volume * 0.9;

                if (order.IsSupportedSlippage)
                    order.Slippage = OrderBaseSet.Symbol.Slippage * 0.9;

                order.Comment = $"TriggerOrder Type={TriggerType.OnTime}";

                return test(order.WithOnTimeTrigger(seconds));
            }

            return RunTest(OTOTest, set, testInfo: $"{testInfo ?? test.Method.Name} TriggerType={TriggerType.OnTime} Seconds={seconds}");
        }
    }
}