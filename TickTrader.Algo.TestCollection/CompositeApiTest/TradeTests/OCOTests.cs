using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class OCOTests : TestGroupBase
    {
        private enum RejectType { Open, Modify };

        private delegate Task RejectTestHandler(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError);

        private readonly bool _useAD;


        protected override string GroupName => nameof(OCOTests);


        internal OCOTests(bool useAD)
        {
            _useAD = useAD;
        }


        protected async override Task RunTestGroup(OrderBaseSet set)
        {
            async Task RunOpenOCOWithDiffOCOSide(OrderType ocoType)
            {
                await AllTestActionsRun(set, ocoType, false);
                await AllTestActionsRun(set, ocoType, true);
            }

            await RunOpenOCOWithDiffOCOSide(OrderType.Stop);

            if (set.IsSupportedStopPrice)
                await RunOpenOCOWithDiffOCOSide(OrderType.Limit);
        }


        private async Task AllTestActionsRun(OrderBaseSet set, OrderType ocoType, bool invertSide)
        {
            var ocoSet = new OrderBaseSet(ocoType, invertSide ? set.Side.Inversed() : set.Side);

            await RunOpenTests(set, ocoSet);
            await RunModifyTests(set, ocoSet);
            await RunCancelTests(set, ocoSet);

            await RunOpenRejectTests(set, ocoSet);
            await RunModifyRejectTests(set, ocoSet);

            if (_useAD)
                await RunFillTests(set, ocoSet);
        }

        private async Task RunOpenTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(OpenOCO, set, ocoSet, true);
            await RunOCOTest(OpenOCO, set, ocoSet, false);
            await RunOCOTest(OpenLinkedOCO, set, ocoSet, true);
        }

        private async Task RunFillTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(FullFillOCOOrder, set, ocoSet, true);
            await RunOCOTest(PartialFillOCOOrder, set, ocoSet);
            await RunOCOTest(OCOWithPartialFillOrder, set, ocoSet, true);
        }

        private async Task RunModifyTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(CreateOCOLinkViaModify, set, ocoSet, true);
            await RunOCOTest(CreateOCOLinkViaModify, set, ocoSet, false);

            await RunOCOTest(BreakOCOLinkViaModify, set, ocoSet, true);
            await RunOCOTest(BreakOCOLinkViaModify, set, ocoSet, false);
        }

        private async Task RunCancelTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(OCOCancelOrder, set, ocoSet);
            await RunOCOTest(OCOCancelExpiration, set, ocoSet);
            await RunOCOTest(OCODoubleCancel, set, ocoSet);
        }

        private async Task RunOpenRejectTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(RejectOCOWithIoC, set, ocoSet);

            await RejectCommonTests(set, ocoSet, RejectType.Open, RunOpenRejectTest, RunOpenOpenRejectTest, RunOpenOpenRejectTest);
        }

        private async Task RunModifyRejectTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RejectModifyRelatedId(set, ocoSet, null);
            await RejectModifyRelatedId(set, ocoSet, "0");

            await RejectCommonTests(set, ocoSet, RejectType.Modify, RunModifyReject, RunOpenModifyReject, RunModifyReject);
        }

        private async Task RejectCommonTests(OrderBaseSet set, OrderBaseSet ocoSet, RejectType type, params RejectTestHandler[] handlers)
        {
            await RejectWith3OCO(set, ocoSet, type, handlers[0]);
            await RejectWithIncorrectOrderType(set, ocoSet, type, handlers[1]);
            await RejectWithIncorrectPrice(set, ocoSet, type, handlers[2]);
        }

        private Task OpenOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return OpenOCO(mainOrder, ocoOrder, OrderEvents.Modify);
        }

        private Task OpenLinkedOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return TestOpenOrder(mainOrder.WithLinkedOCO(ocoOrder), OrderEvents.Open);
        }

        private async Task OpenOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, params Type[] events)
        {
            await TestOpenOrder(mainOrder);
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder), events);
            await mainOrder.WithOCO(ocoOrder).Modified.Task;
        }


        private Task FullFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder.Comment = ADComments.ADCommentsList.WithActivate;

            return OpenOcoOrderWithActivation(mainOrder, ocoOrder);
        }

        private async Task PartialFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(ocoOrder.Volume * 0.8);

            await OpenOcoOrderWithActivation(mainOrder, ocoOrder);
            await TestCancelOrder(ocoOrder);
        }

        private async Task OpenOcoOrderWithActivation(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder);
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder), OrderEvents.Modify, OrderEvents.Fill, OrderEvents.Cancel);
            await mainOrder.Canceled.Task;
        }

        private async Task OCOWithPartialFillOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            mainOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(mainOrder.Volume * 0.4);

            await TestOpenOrder(mainOrder, OrderEvents.Fill);
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder), OrderEvents.Modify);
        }


        private async Task CreateOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrders(mainOrder, ocoOrder);
            await TestModifyOrder(ocoOrder.WithOCO(mainOrder), OrderEvents.Modify);
        }

        private async Task BreakOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await CreateOCOLinkViaModify(mainOrder, ocoOrder);
            await TestModifyOrder(ocoOrder.WithRemovedOCO(), OrderEvents.Modify);
            await TestCancelOrders(mainOrder, ocoOrder);
        }


        private async Task OCOCancelOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenOCO(mainOrder, ocoOrder);
            await TestCancelOrder(mainOrder, OrderEvents.Cancel);
            await ocoOrder.Canceled.Task;
        }

        private async Task OCOCancelExpiration(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenOCO(mainOrder.WithExpiration(4), ocoOrder, OrderEvents.Modify, OrderEvents.Expire, OrderEvents.Cancel);
            await ocoOrder.Canceled.Task;
        }

        private async Task OCODoubleCancel(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenOCO(mainOrder, ocoOrder);
            await TestCancelOrder(mainOrder, OrderEvents.Cancel);
            await TestCancelReject(ocoOrder, OrderCmdResultCodes.OrderNotFound);
            await ocoOrder.Canceled.Task;
        }


        private Task RejectOCOWithIoC(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder.Options = OrderExecOptions.ImmediateOrCancel;

            return RunOpenOpenRejectTest(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
        }

        private async Task RunOpenOpenRejectTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError)
        {
            await TestOpenOrder(mainOrder);
            await RunOpenRejectTest(mainOrder, ocoOrder, expectedError);
        }

        private async Task RunOpenRejectTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError)
        {
            await TestOpenReject(ocoOrder.WithOCO(mainOrder), expectedError);
            await RemoveOrder(mainOrder);
        }


        private Task RejectModifyRelatedId(OrderBaseSet set, OrderBaseSet ocoSet, string value)
        {
            async Task RejectModifyRelatedId(OrderStateTemplate mainOrder, OrderStateTemplate _)
            {
                await TestOpenOrder(mainOrder);

                mainOrder.Options = OrderExecOptions.OneCancelsTheOther;
                mainOrder.OcoRelatedOrderId = value;

                await TestModifyReject(mainOrder, OrderCmdResultCodes.OCORelatedIdNotFound);
                await TestCancelOrder(mainOrder);
            }

            return RunOCOTest(RejectModifyRelatedId, set, ocoSet, testInfo: $"{nameof(RejectModifyRelatedId)}={value}");
        }

        private async Task RunOpenModifyReject(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            await TestOpenOrders(mainOrder, ocoOrder);
            await RunModifyReject(mainOrder, ocoOrder, expectedCode);
        }

        private async Task RunModifyReject(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            await TestModifyReject(ocoOrder.WithOCO(mainOrder), expectedCode);
            await RemoveOrder(mainOrder);
        }


        private Task RejectWith3OCO(OrderBaseSet set, OrderBaseSet ocoSet, RejectType type, RejectTestHandler testHandler)
        {
            async Task RejectWith3OCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                var thirdOrder = ocoOrder.Copy();

                if (type == RejectType.Modify)
                    await TestOpenOrder(thirdOrder);

                await OpenOCO(mainOrder, ocoOrder);
                await testHandler(mainOrder, thirdOrder, OrderCmdResultCodes.OCOAlreadyExists);

                if (type == RejectType.Modify)
                    await TestCancelOrder(thirdOrder);
            }

            return RunOCOTest(RejectWith3OCO, set, ocoSet, testInfo: $"{type} {nameof(RejectWith3OCO)}");
        }

        private Task RejectWithIncorrectOrderType(OrderBaseSet set, OrderBaseSet ocoSet, RejectType type, RejectTestHandler testHandler)
        {
            Task RejectWithIncorrectOrderType(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                return testHandler(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
            }

            OrderType changeType = OrderType.Limit;

            if (ocoSet.IsSupportedStopPrice)
            {
                changeType = OrderType.StopLimit;
                ocoSet = new OrderBaseSet(changeType, ocoSet.Side);
            }
            else
                set = new OrderBaseSet(changeType, set.Side);

            return RunOCOTest(RejectWithIncorrectOrderType, set, ocoSet, testInfo: $"{type} RejectOCOWith{changeType}");
        }

        private async Task RejectWithIncorrectPrice(OrderBaseSet set, OrderBaseSet ocoSet, RejectType type, RejectTestHandler testHandler)
        {
            async Task RejectWithIncorrectPrice(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                if (type == RejectType.Modify)
                    await TestOpenOrders(mainOrder, ocoOrder);

                ocoOrder = ocoOrder.ForPending(ocoOrder.Type == OrderType.Limit ? 2 : 4); //chech that for pair limit/stop price for Buy must be less than Sell

                await testHandler(mainOrder, ocoOrder, OrderCmdResultCodes.IncorrectPrice);
            }

            if (IsLimitStopPair(set, ocoSet) && set.Side != ocoSet.Side)
                await RunOCOTest(RejectWithIncorrectPrice, set, ocoSet, testInfo: $"{type} {nameof(RejectWithIncorrectPrice)}");
        }


        private Task RunOCOTest(Func<OrderStateTemplate, OrderStateTemplate, Task> test, OrderBaseSet set, OrderBaseSet ocoSet,
                                bool equalVolume = false, string testInfo = null)
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

            return RunTest(OCOTestEnviroment, set, testInfo: $"{testInfo ?? test.Method.Name} OCO=({ocoSet}) equalVolume={equalVolume}");
        }

        private static bool IsLimitStopPair(OrderBaseSet mainOrder, OrderBaseSet ocoOrder)
        {
            return mainOrder.IsSupportedStopPrice ^ ocoOrder.IsSupportedStopPrice;
        }
    }
}