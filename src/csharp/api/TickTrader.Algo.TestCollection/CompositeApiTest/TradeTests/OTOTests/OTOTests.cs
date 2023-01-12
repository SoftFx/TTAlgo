using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TriggerType = TickTrader.Algo.Api.ContingentOrderTrigger.TriggerType;


namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed partial class OTOTests : OCOTests
    {
        private readonly OnTimeTriggerTests _onTimeTests;

        private readonly bool _useOCO;

        private delegate Task TriggerTimeTest(OrderStateTemplate order);
        private delegate Task TriggerTest(OrderStateTemplate order, OrderStateTemplate trigger);

        protected override string GroupName => nameof(OTOTests);


        internal OTOTests(bool useOCO, bool useAD) : base(useAD)
        {
            _onTimeTests = new OnTimeTriggerTests(this);

            _useOCO = useOCO;
        }


        protected override async Task RunTestGroup(OrderBaseSet set)
        {
            await _onTimeTests.RunTests(set);

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
            await TestCancelOrder(order, Events.Cancel);
            await trigger.Canceled.Task;
        }

        private Task OpenExpiredTriggerWithExpiration(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            return OpenAndCancelExpiredTrigger(order, trigger.WithExpiration(10));
        }

        private async Task OpenAndTriggeredExpiredTrigger(OrderStateTemplate order, OrderStateTemplate trigger)
        {
            await TestOpenOrder(order.WithExpiration(5));
            await TestOpenOrder(trigger.WithOnExpiredTrigger(order), Events.Expire, Events.Cancel, Events.Open);
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
            await TestModifyOrder(order, Events.Cancel);
            await trigger.Canceled.Task;
        }

        private async Task RunModifyTriggerType(OrderBaseSet set, TriggerType currentTriggerType)
        {
            //if (currentTriggerType != TriggerType.OnTime)
            //    await ModifyToOnTimeTrigger();

            //if (currentTriggerType != TriggerType.OnPendingOrderExpired)
            //{
            //    await RunOnTimeTest(ModifyToPendingOrderExpired, set);
            //    await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithoutExpiration, set);
            //    await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithoutTriggerId, set);
            //    await RunOnTimeTest(RejectModifyToPendingOrderExpiredWithIncorrectid, set);
            //}

            //if (currentTriggerType != TriggerType.OnPendingOrderPartiallyFilled)
            //{
            //    await RunOnTimeTest(ModifyToPendingOrderPartiallyFilled, set);
            //    await RunOnTimeTest(RejectModifyToPendingOrderPartiallyFilledWithoutTriggerId, set);
            //    await RunOnTimeTest(RejectModifyToPendingOrderPartiallyFilledWithIncorrectId, set);
            //}
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