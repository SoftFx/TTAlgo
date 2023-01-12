using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OCOTests : TestGroupBase
    {
        private enum RejectType { Open, Modify };

        protected readonly bool _useAD;


        private delegate Task RejectTestHandler(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError);

        protected delegate Task OTOTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder);


        protected override string GroupName => nameof(OCOTests);


        protected Action<OrderStateTemplate, OrderStateTemplate> _updateOCOTemplates;


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

            await RunOpenOCOTests(set, ocoSet);
            await RunModifyOCOTests(set, ocoSet);
            await RunFillOCOTests(set, ocoSet);
            await RunCancelTests(set, ocoSet);

            await RunOpenRejectTests(set, ocoSet);
            await RunModifyRejectTests(set, ocoSet);
        }

        protected async Task RunOpenOCOTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(OpenOCO, set, ocoSet, true);
            await RunOCOTest(OpenOCO, set, ocoSet, false);

            await RunOCOTest(OpenLinkedOCO, set, ocoSet);
            await RunOCOTest(OCOCancelOrder, set, ocoSet);
        }

        private async Task RunFillOCOTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            //if (!IsLimitStopPairWithDiffSide(set, ocoSet))
            //    await RunOCOTest(FillLinkedOCOOrders, set, ocoSet);
            //    await FillLinkedOcoOrder
            //    await ExpireFirstLinkedOrder
            //    await ExpireSecondLinkedOrder

            if (_useAD)
            {
                await RunOCOTest(FullFillOCOOrder, set, ocoSet, true);
                await RunOCOTest(PartialFillOCOOrder, set, ocoSet);
                await RunOCOTest(OCOWithPartialFillOrder, set, ocoSet, true);
            }
        }

        protected async Task RunModifyOCOTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(CreateOCOLinkViaModify, set, ocoSet, true);
            await RunOCOTest(CreateOCOLinkViaModify, set, ocoSet, false);

            await RunOCOTest(BreakOCOLinkViaModify, set, ocoSet, true);
            await RunOCOTest(BreakOCOLinkViaModify, set, ocoSet, false);
        }

        private async Task RunCancelTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
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

        protected Task OpenOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return OpenOCO(mainOrder, ocoOrder, Events.Modify);
        }

        protected async Task OpenLinkedOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.WithLinkedOCO(ocoOrder), Events.Open);
            await ocoOrder.Opened.Task;
        }

        private async Task OpenOCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, params Type[] events)
        {
            await TestOpenOrder(mainOrder);
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder), events);
            await mainOrder.Modified.Task;
        }


        private async Task FillLinkedOCOOrders(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.ForExecuting().WithLinkedOCO(ocoOrder), Events.Open, Events.Fill, Events.Cancel);
            await mainOrder.Filled.Task;
            await ocoOrder.Canceled.Task;
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
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder), Events.Modify, Events.Fill, Events.Cancel);
            await mainOrder.Canceled.Task;
        }

        private async Task OCOWithPartialFillOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            mainOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(mainOrder.Volume * 0.4);

            await TestOpenOrder(mainOrder, Events.Fill);
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder), Events.Modify);
        }


        private async Task CreateOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrders(mainOrder, ocoOrder);
            await TestModifyOrder(ocoOrder.WithOCO(mainOrder), Events.Modify);
        }

        private async Task BreakOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await CreateOCOLinkViaModify(mainOrder, ocoOrder);
            await TestModifyOrder(ocoOrder.BreakOCO(), Events.Modify);
        }

        protected async Task OCOCancelOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenOCO(mainOrder, ocoOrder);
            await TestCancelOrder(mainOrder, Events.Cancel);
            await ocoOrder.Canceled.Task;
        }

        private async Task OCOCancelExpiration(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenOCO(mainOrder.WithExpiration(4), ocoOrder, Events.Modify, Events.Expire, Events.Cancel);
            await ocoOrder.Canceled.Task;
        }

        private async Task OCODoubleCancel(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await OpenOCO(mainOrder, ocoOrder);
            await TestCancelOrder(mainOrder, Events.Cancel);
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

        private Task RunOpenRejectTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError)
        {
            return TestOpenReject(ocoOrder.WithOCO(mainOrder), expectedError);
        }


        private Task RejectModifyRelatedId(OrderBaseSet set, OrderBaseSet ocoSet, string value)
        {
            async Task RejectModifyRelatedId(OrderStateTemplate mainOrder, OrderStateTemplate _)
            {
                await TestOpenOrder(mainOrder);

                mainOrder.Options = OrderExecOptions.OneCancelsTheOther;
                mainOrder.OcoRelatedOrderId = value;

                await TestModifyReject(mainOrder, OrderCmdResultCodes.OCORelatedIdNotFound);
            }

            return RunOCOTest(RejectModifyRelatedId, set, ocoSet, testInfo: $"{nameof(RejectModifyRelatedId)}={value}");
        }

        private async Task RunOpenModifyReject(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            await TestOpenOrders(mainOrder, ocoOrder);
            await RunModifyReject(mainOrder, ocoOrder, expectedCode);
        }

        private Task RunModifyReject(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            return TestModifyReject(ocoOrder.WithOCO(mainOrder), expectedCode);
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
                    await TestCancelOrder(thirdOrder.BreakOCO());
            }

            return RunOCOTest(RejectWith3OCO, set, ocoSet, testInfo: $"{type} {nameof(RejectWith3OCO)}");
        }

        private Task RejectWithIncorrectOrderType(OrderBaseSet set, OrderBaseSet ocoSet, RejectType type, RejectTestHandler testHandler)
        {
            async Task RejectWithIncorrectOrderType(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                await testHandler(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);

                ocoOrder.BreakOCO();
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

                ocoOrder = ocoOrder.ForPending(ocoOrder.Type == OrderType.Limit ? 2 : 4); //check that for pair limit/stop price for Buy must be less than Sell

                await testHandler(mainOrder, ocoOrder, OrderCmdResultCodes.IncorrectPrice);

                ocoOrder.BreakOCO();
            }

            if (IsLimitStopPairWithDiffSide(set, ocoSet))
                await RunOCOTest(RejectWithIncorrectPrice, set, ocoSet, testInfo: $"{type} {nameof(RejectWithIncorrectPrice)}");
        }


        protected Task RunOCOTest(OTOTest test, OrderBaseSet set, OrderBaseSet ocoSet, bool equalVolume = false, string testInfo = null)
        {
            async Task OCOTestEnviroment(OrderStateTemplate mainOrder)
            {
                var ocoOrder = ocoSet.BuildOrder(newVolume: mainOrder.Volume / 2)
                                     .ForPending(ocoSet.Type == OrderType.Limit ? 4 : 2); //for pair limit/stop price for Buy must be less than Sell

                ocoOrder.OcoEqualVolume = equalVolume;

                _updateOCOTemplates?.Invoke(mainOrder, ocoOrder);

                await test(mainOrder, ocoOrder);

                await Task.Yield(); //wait modification real Oco and Main order
                await RemoveOrder(ocoOrder);

                await Task.Yield();
                await RemoveOrder(mainOrder);
            }

            return RunTest(OCOTestEnviroment, set, testInfo: $"{testInfo ?? test.Method.Name} OCO=({ocoSet}) equalVolume={equalVolume}");
        }

        private static bool IsLimitStopPairWithDiffSide(OrderBaseSet main, OrderBaseSet oco)
        {
            return (main.IsSupportedStopPrice ^ oco.IsSupportedStopPrice) && main.Side != oco.Side;
        }
    }
}