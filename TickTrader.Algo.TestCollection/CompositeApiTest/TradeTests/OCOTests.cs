using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class OCOTests : TestGroupBase
    {
        private enum RejectType { Open, Modify };

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
            await RunOCOTest(RejectOpen3OCO, set, ocoSet);

            await RunCommonRejectTests(set, ocoSet, RejectType.Open, RunOpenOCORejectTest);

            //for pair limit/ stop price for Buy must be less than Sell
            if (IsLimitStopPair(set, ocoSet) && set.Side != ocoSet.Side)
                await RunOCOTest(RejectIncorrectLimitPrice, set, ocoSet);
        }

        private async Task RunModifyRejectTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            Task RunRejectModifyRelatedId(string value)
            {
                return RunOCOTest((mainOrder, _) => RejectModifyRelatedId(mainOrder, value),
                                  set, ocoSet, testInfo: $"{nameof(RejectModifyRelatedId)}={value}");
            }

            await RunRejectModifyRelatedId(null);
            await RunRejectModifyRelatedId("0");
            await RunOCOTest(RejectModify3OCO, set, ocoSet);

            await RunCommonRejectTests(set, ocoSet, RejectType.Modify, RunOpenModifyReject);

            //for pair limit/stop price for Buy must be less than Sell
            if (IsLimitStopPair(set, ocoSet) && set.Side != ocoSet.Side)
                await RunOCOTest(RejectModifyWithIncorrectPrices, set, ocoSet);
        }

        private async Task RunCommonRejectTests(OrderBaseSet set, OrderBaseSet ocoSet, RejectType type,
                                                Func<OrderStateTemplate, OrderStateTemplate, OrderCmdResultCodes, Task> testHandler)
        {
            if (ocoSet.IsSupportedStopPrice)
                await RunRejectOCOTest(RejectOCOWithStopLimit, testHandler, set, ocoSet, type);
            else
                await RunRejectOCOTest(RejectOCOLimitWithLimit, testHandler, set, ocoSet, type);

            ////for pair limit/stop price for Buy must be less than Sell
            //if (IsLimitStopPair(set, ocoSet) && set.Side != ocoSet.Side)
            //    await RunRejectOCOTest(RejectOCOLimitWithLimit, testHandler, set, ocoSet, type);
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
            await mainOrder.Modified.Task;
        }


        private async Task FullFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder.Comment = ADComments.ADCommentsList.WithActivate;

            await OpenOcoOrderWithActivation(mainOrder, ocoOrder);
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

            await TestCancelOrder(mainOrder);
            await TestCancelOrder(ocoOrder);
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


        private async Task RejectOCOWithIoC(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder.Options = OrderExecOptions.ImmediateOrCancel;

            await RunOpenOCORejectTest(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
        }

        private async Task RejectIncorrectLimitPrice(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder = ocoOrder.ForPending(ocoOrder.Type == OrderType.Limit ? 2 : 4);

            await RunOpenOCORejectTest(mainOrder, ocoOrder, OrderCmdResultCodes.IncorrectPrice);
        }

        private async Task RejectOpen3OCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            var thirdOrder = ocoOrder.Copy();

            await OpenOCO(mainOrder, ocoOrder);
            await TestOpenReject(thirdOrder.WithOCO(mainOrder), OrderCmdResultCodes.OCOAlreadyExists);
        }

        private Task<(OrderStateTemplate, OrderStateTemplate)> RejectOCOWithStopLimit(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return Task.FromResult((mainOrder, ocoOrder.BuildOrder(OrderType.StopLimit)));
        }

        private Task<(OrderStateTemplate, OrderStateTemplate)> RejectOCOLimitWithLimit(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return Task.FromResult((mainOrder.BuildOrder(OrderType.Limit), ocoOrder));
        }

        private async Task RunOpenOCORejectTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError)
        {
            await TestOpenOrder(mainOrder);
            await TestOpenReject(ocoOrder.WithOCO(mainOrder), expectedError);

            await TestCancelOrder(mainOrder);
        }


        private async Task RejectModifyRelatedId(OrderStateTemplate mainOrder, string value)
        {
            await TestOpenOrder(mainOrder);

            mainOrder.Options = OrderExecOptions.OneCancelsTheOther;
            mainOrder.OcoRelatedOrderId = value;

            await TestModifyReject(mainOrder, OrderCmdResultCodes.OCORelatedIdNotFound);
            await TestCancelOrder(mainOrder);
        }

        private async Task RejectModifyWithIncorrectPrices(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrders(mainOrder, ocoOrder);

            ocoOrder = ocoOrder.ForPending(ocoOrder.Type == OrderType.Limit ? 2 : 4);

            await RunModifyReject(mainOrder, ocoOrder, OrderCmdResultCodes.IncorrectPrice);
        }

        private async Task RejectModify3OCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            var thirdOrder = ocoOrder.Copy();

            await OpenOCO(mainOrder, ocoOrder);
            await TestOpenOrder(thirdOrder.ForPending());

            await TestModifyReject(ocoOrder.WithOCO(mainOrder), OrderCmdResultCodes.OCOAlreadyExists);
            await TestCancelOrder(thirdOrder);
        }

        private async Task RunOpenModifyReject(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            await TestOpenOrders(mainOrder, ocoOrder);
            await RunModifyReject(mainOrder, ocoOrder, expectedCode);
        }

        private async Task RunModifyReject(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            await TestModifyReject(ocoOrder.WithOCO(mainOrder), expectedCode);
            await TestCancelOrder(mainOrder);
            await TestCancelOrder(ocoOrder);
        }


        private Task RunRejectOCOTest(Func<OrderStateTemplate, OrderStateTemplate, Task<(OrderStateTemplate, OrderStateTemplate)>> rejectTest,
                                      Func<OrderStateTemplate, OrderStateTemplate, OrderCmdResultCodes, Task> testHandler,
                                      OrderBaseSet set, OrderBaseSet ocoSet, RejectType type,
                                      OrderCmdResultCodes expectedError = OrderCmdResultCodes.Unsupported)
        {
            async Task RejectTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
            {
                (mainOrder, ocoOrder) = await rejectTest(mainOrder, ocoOrder);
                await testHandler(mainOrder, ocoOrder, expectedError);
            }

            return RunOCOTest(RejectTest, set, ocoSet, testInfo: $"{type} {rejectTest.Method.Name}");
        }

        private async Task RunOCOTest(Func<OrderStateTemplate, OrderStateTemplate, Task> test, OrderBaseSet set, OrderBaseSet ocoSet,
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

            await RunTest(OCOTestEnviroment, set, testInfo: $"{testInfo ?? test.Method.Name} OCO=({ocoSet}) equalVolume={equalVolume}");
        }

        private static bool IsLimitStopPair(OrderBaseSet mainOrder, OrderBaseSet ocoOrder)
        {
            return mainOrder.IsSupportedStopPrice ^ ocoOrder.IsSupportedStopPrice;
        }
    }
}
