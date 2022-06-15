using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TriggerType = TickTrader.Algo.Api.ContingentOrderTrigger.TriggerType;


namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class OTOTests : TestGroupBase
    {
        private readonly bool _useOCO, _useAD;

        private delegate Task TriggerTimeTest(OrderStateTemplate order);
        private delegate Task TriggerTest(OrderStateTemplate order, OrderStateTemplate trigger);
        private delegate Task OCOwithOTOTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder);

        protected override string GroupName => nameof(OTOTests);


        internal OTOTests(bool useOCO, bool useAD)
        {
            _useOCO = useOCO;
            _useAD = useAD;
        }


        protected override async Task RunTestGroup(OrderBaseSet set)
        {
            await RunOnTimeTriggerTests(set);
            //await RunOnExpiredTriggerTests(set);
        }


        private async Task RunOnExpiredTriggerTests(OrderBaseSet triggerSet)
        {
            Task RunTests(OrderType orderType)
            {
                var orderSet = new OrderBaseSet(orderType, triggerSet.Side);

                return RunAllExpiredTests(orderSet, triggerSet);
            }

            await RunTests(OrderType.Limit);
            await RunTests(OrderType.Stop);

            var incorrectSet = new OrderBaseSet(OrderType.StopLimit, triggerSet.Side);

            await RunOnExpiredTest(RejectOpenExpiredTriggerWithStopLimit, incorrectSet, triggerSet);

            if (!triggerSet.IsInstantOrder)
                await RunOnExpiredTest(RejectOpenExpiredTriggerWithStopLimit, triggerSet, incorrectSet);
        }

        private async Task RunAllExpiredTests(OrderBaseSet orderSet, OrderBaseSet triggerSet)
        {
            await RunOnExpiredTest(OpenAndCancelExpiredTrigger, orderSet, triggerSet);

            if (!triggerSet.IsInstantOrder)
                await RunOnExpiredTest(OpenExpiredTriggerWithExpiration, orderSet, triggerSet);
            //await RunOnExpiredTest(OpenAndTriggeredExpiredTrigger, orderSet, triggerSet); wait fix on TTS side

            //await RunOnExpiredTest(RejectExpiredTriggerExpirationLessThanOrderExpiration, orderSet, triggerSet); wait fix on TTS side
            await RunOnExpiredTest(RejectedExpiredTriggerWithoutTriggeredId, orderSet, triggerSet);
            await RunOnExpiredTest(RejectExpiredTriggerToOrderWithoutExpiration, orderSet, triggerSet);

            //if (_useAD) #invalid
            //{
            //    await RunOnExpiredTest(FullFillWithOnExpiredTrigger, orderSet, triggerSet);
            //}
        }

        private async Task RejectOpenExpiredTriggerWithStopLimit(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order.WithExpiration(5));
            await TestOpenReject(trigger.WithOnExpiredTrigger(order), OrderCmdResultCodes.IncorrectTriggerOrderType);
            await TestCancelOrder(order);
        }

        private async Task OpenAndCancelExpiredTrigger(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order.WithExpiration(5));
            await TestOpenOrder(trigger.WithOnExpiredTrigger(order));
            await TestCancelOrder(order, OrderEvents.Cancel);
            await trigger.Canceled.Task;
        }

        private Task OpenExpiredTriggerWithExpiration(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            return OpenAndCancelExpiredTrigger(order, trigger.WithExpiration(10));
        }

        private async Task OpenAndTriggeredExpiredTrigger(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order.WithExpiration(5));
            await TestOpenOrder(trigger.WithOnExpiredTrigger(order), OrderEvents.Expire, OrderEvents.Cancel, OrderEvents.Open);
            await TestCancelOrder(trigger);
        }

        private async Task RejectExpiredTriggerExpirationLessThanOrderExpiration(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order.WithExpiration(10));
            await TestOpenReject(trigger.WithOnExpiredTrigger(order).WithExpiration(5), OrderCmdResultCodes.IncorrectConditionsForTrigger);
            await TestCancelOrder(order);
        }

        private Task RejectedExpiredTriggerWithoutTriggeredId(OrderStateTemplate _, OrderStateTemplate trigger)
        {
            trigger.TriggerType = TriggerType.OnPendingOrderExpired;

            return TestOpenReject(trigger, OrderCmdResultCodes.IncorrectTriggerOrderId);
        }

        private async Task RejectExpiredTriggerToOrderWithoutExpiration(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order);
            await TestOpenReject(trigger.WithOnExpiredTrigger(order), OrderCmdResultCodes.IncorrectExpiration);
            await TestCancelOrder(order);
        }

        private async Task FullFillWithOnExpiredTrigger(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order.WithExpiration(20));
            await TestOpenOrder(trigger.WithOnExpiredTrigger(order));

            order.Comment = ADComments.ADCommentsList.WithActivate;
            await TestModifyOrder(order, OrderEvents.Cancel);
            await trigger.Canceled.Task;
        }

        private async Task RunOnTimeTriggerTests(OrderBaseSet set)
        {
            if (set.IsInstantOrder)
                await RunOnTimeTest(OpenInstantOnTime, set, 2);
            else
                await RunOnTimeTest(OpenPendingOnTime, set, 4);

            await RunOnTimeTest(RejectWithIncorrectTime, set, -30);
            await RunOnTimeTest(RejectWithNullTime, set);

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
            await RunOnTimeTest(ModifyPropertyTest, set);

            //await RunOnTimeTest(RejectModifyOnTime, set, 30); wait fix on TTS side

            if (!set.IsInstantOrder)
                await RunOnTimeTest(RejectModifyExpiration, set);

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
                await RunOnTimeTest(ModifyToPendingOrderExpired, set);
                await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithoutExpiration, set);
                await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithoutTriggerId, set);
                await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithIncorrectid, set);
            }

            if (currentTriggerType != TriggerType.OnPendingOrderPartiallyFilled)
            {
                await RunOnTimeTest(ModifyToPendingOrderPartiallyFilled, set);
                await RunOnTimeTest(RejectModifyToPendingOrderPartiallyFilledWithoutTriggerId, set);
                await RunOnTimeTest(RejectModifyToPendingOrderPartiallyFilledWithIncorrectId, set);
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

            await TestModifyOrder(order.WithOnExpiredTrigger(secondOrder));

            await TestCancelOrder(order);
            await TestCancelOrder(secondOrder);
        }

        private async Task RejectModifyToPendingOrderExpiredWithoutExpiration(OrderStateTemplate order)
        {
            var secondOrder = new OrderBaseSet(OrderType.Limit, order.Side).BuildOrder().ForPending();

            await TestOpenOrder(order);
            await TestOpenOrder(secondOrder);

            await TestModifyReject(order.WithOnExpiredTrigger(secondOrder), OrderCmdResultCodes.IncorrectExpiration);

            await TestCancelOrder(order);
            await TestCancelOrder(secondOrder);
        }

        private async Task RejectModifyToPendingOrderExpiredWithoutTriggerId(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyReject(order.WithOnExpiredTrigger(orderTriggeredById: null), OrderCmdResultCodes.IncorrectTriggerOrderId);
            await TestCancelOrder(order);
        }

        private async Task RejectModifyToPendingOrderExpiredWithIncorrectid(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyReject(order.WithOnExpiredTrigger(orderTriggeredById: "0"), OrderCmdResultCodes.OrderNotFound);
            await TestCancelOrder(order);
        }


        private async Task ModifyToPendingOrderPartiallyFilled(OrderStateTemplate order)
        {
            var secondOrder = new OrderBaseSet(OrderType.Limit, order.Side).BuildOrder().ForPending();

            await TestOpenOrder(order);
            await TestOpenOrder(secondOrder);

            await TestModifyOrder(order.WithOnPartialFilledTrigger(secondOrder));

            await TestCancelOrder(order);
            await TestCancelOrder(secondOrder);
        }

        private async Task RejectModifyToPendingOrderPartiallyFilledWithoutTriggerId(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyReject(order.WithOnPartialFilledTrigger(orderTriggeredById: null), OrderCmdResultCodes.IncorrectTriggerOrderId);
            await TestCancelOrder(order);
        }

        private async Task RejectModifyToPendingOrderPartiallyFilledWithIncorrectId(OrderStateTemplate order)
        {
            await TestOpenOrder(order);
            await TestModifyReject(order.WithOnPartialFilledTrigger(orderTriggeredById: "0"), OrderCmdResultCodes.OrderNotFound);
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
            await RunOnTimeTest(OpenCancelOnTime, set);

            if (!set.IsInstantOrder)
                await RunOnTimeTest(ActivateWithExpiration, set, 4);

            if (set.IsSupportedOCO)
                await RunOCOTriggerTest(OpenCancelOnTimeOCO, set, new OrderBaseSet(OrderType.Stop, set.Side));
        }


        private async Task OpenInstantOnTime(OrderStateTemplate order)
        {
            if (order.IsGrossAcc)
                await TestOpenOrder(order, OrderEvents.Activate, OrderEvents.Open, OrderEvents.Fill, OrderEvents.Open);
            else
                await TestOpenOrder(order, OrderEvents.Activate, OrderEvents.Open, OrderEvents.Fill);

            await order.OnTimeTriggerReceived.Task;
        }

        private async Task OpenPendingOnTime(OrderStateTemplate order)
        {
            await TestOpenOrder(order, OrderEvents.Activate, OrderEvents.Open);
            await order.OnTimeTriggerReceived.Task;
            await order.Opened.Task;
            await TestCancelOrder(order);
        }

        private Task ActivateWithExpiration(OrderStateTemplate order)
        {
            return TestOpenOrder(order.WithExpiration(8), OrderEvents.Activate, OrderEvents.Open, OrderEvents.Expire);
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
            return TestOpenReject(order.WithExpiration(10), OrderCmdResultCodes.IncorrectTriggerTime);
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
                await TestModifyOrder(order, OrderEvents.Activate, OrderEvents.Open, OrderEvents.Fill);
                await order.OnTimeTriggerReceived.Task;
            }
            else
            {
                await TestModifyOrder(order, OrderEvents.Activate, OrderEvents.Open);
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

            order.FillAdditionalProperties();
            order.Volume *= 0.9;
            order.Comment = "Modified OnTime trigger comment";

            //if (order.IsSupportedMaxVisibleVolume)
            //    order.MaxVisibleVolume = order.Volume * 0.9;

            //if (order.IsSupportedSlippage)
            //    order.Slippage = OrderBaseSet.Symbol.Slippage * 0.9;

            if (order.WithOnTimeTrigger(4).IsInstantOrder)
            {
                if (order.IsGrossAcc)
                    await TestModifyOrder(order, OrderEvents.Activate, OrderEvents.Open, OrderEvents.Fill, OrderEvents.Open);
                else
                    await TestModifyOrder(order, OrderEvents.Activate, OrderEvents.Open, OrderEvents.Fill);

                await order.OnTimeTriggerReceived.Task;
            }
            else
            {
                await TestModifyOrder(order.ForPending(5).WithExpiration(90), OrderEvents.Activate, OrderEvents.Open);
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

        private Task RunOnTimeTest(TriggerTimeTest test, OrderBaseSet set, int seconds = 30, string testInfo = null)
        {
            Task OnTimeTest(OrderStateTemplate order)
            {
                //if (order.IsSupportedMaxVisibleVolume)
                //    order.MaxVisibleVolume = order.Volume * 0.9;

                //if (order.IsSupportedSlippage)
                //    order.Slippage = OrderBaseSet.Symbol.Slippage * 0.9;

                //order.Comment = $"TriggerOrder Type={TriggerType.OnTime}";

                return test(order.FillAdditionalProperties().WithOnTimeTrigger(seconds));
            }

            return RunTest(OnTimeTest, set, testInfo: $"{testInfo ?? test.Method.Name} TriggerType={TriggerType.OnTime} Seconds={seconds}");
        }

        private Task RunOnExpiredTest(TriggerTest test, OrderBaseSet orderSet, OrderBaseSet triggerSet, string testInfo = null)
        {
            Task OnTriggerTest(OrderStateTemplate order)
            {
                var trigger = triggerSet.BuildOrder();

                return test(order, trigger.FillAdditionalProperties());
            }

            return RunTest(OnTriggerTest, orderSet, testInfo: $"{testInfo ?? test.Method.Name} TriggerType={TriggerType.OnPendingOrderExpired} Order={orderSet} Trigger={triggerSet}");
        }
    }
}