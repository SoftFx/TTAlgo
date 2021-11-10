using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class OCOTests : TestGroupBase
    {
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
            await RunOCOTest(OpenLinkedOCO, set, ocoSet);
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

            if (ocoSet.IsSupportedStopPrice)
                await RunOCOTest(RejectOCOWithStopLimit, set, ocoSet);
            else
                await RunOCOTest(RejectOCOLimitWithLimit, set, ocoSet);

            //for pair limit/stop price for Buy must be less than Sell
            if (IsLimitStopPair(set, ocoSet) && set.Side != ocoSet.Side)
                await RunOCOTest(RejectIncorrecLimitPrice, set, ocoSet);
        }

        private async Task RunModifyRejectTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(RejectModifyWithNullRelatedId, set, ocoSet);
            await RunOCOTest(RejectModifyWithIncorrectRelatedId, set, ocoSet);
            await RunOCOTest(RejectModify3OCO, set, ocoSet);

            if (ocoSet.IsSupportedStopPrice)
                await RunOCOTest(RejectModifyOCOWithStopLimit, set, ocoSet);
            else
                await RunOCOTest(RejectModifyOCOLimitWithLimit, set, ocoSet);

            //for pair limit/stop price for Buy must be less than Sell
            if (IsLimitStopPair(set, ocoSet) && set.Side != ocoSet.Side)
                await RunOCOTest(RejectModifyWithIncorrectPrices, set, ocoSet);
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

        private async Task RejectIncorrecLimitPrice(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
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

        private async Task RejectOCOWithStopLimit(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder = ocoOrder.BuildOrder(OrderType.StopLimit);

            await RunOpenOCORejectTest(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
        }

        private async Task RejectOCOLimitWithLimit(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            mainOrder = mainOrder.BuildOrder(OrderType.Limit);

            await RunOpenOCORejectTest(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
        }

        private async Task RunOpenOCORejectTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedError)
        {
            await TestOpenOrder(mainOrder);
            await TestOpenReject(ocoOrder.WithOCO(mainOrder), expectedError);

            await TestCancelOrder(mainOrder);
        }


        private Task RejectModifyWithNullRelatedId(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return RejectModifyRelatedId(mainOrder, ocoOrder, null);
        }

        private Task RejectModifyWithIncorrectRelatedId(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            return RejectModifyRelatedId(mainOrder, ocoOrder, "0");
        }

        private async Task RejectModifyRelatedId(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, string value)
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

            await RunRejectModifyOCOTest(mainOrder, ocoOrder, OrderCmdResultCodes.IncorrectPrice);
        }

        private async Task RejectModify3OCO(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            var thirdOrder = ocoOrder.Copy();

            await OpenOCO(mainOrder, ocoOrder);
            await TestOpenOrder(thirdOrder.ForPending());

            await RunRejectModifyOCOTest(thirdOrder, mainOrder, OrderCmdResultCodes.OCOAlreadyExists);
        }

        private async Task RejectModifyOCOWithStopLimit(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            ocoOrder = ocoOrder.BuildOrder(OrderType.StopLimit).ForPending();

            await TestOpenOrders(mainOrder, ocoOrder);
            await RunRejectModifyOCOTest(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
            await TestCancelOrder(ocoOrder);
        }

        private async Task RejectModifyOCOLimitWithLimit(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            mainOrder = mainOrder.BuildOrder(OrderType.Limit).ForPending();

            await TestOpenOrders(mainOrder, ocoOrder);
            await RunRejectModifyOCOTest(mainOrder, ocoOrder, OrderCmdResultCodes.Unsupported);
        }

        private async Task RunRejectModifyOCOTest(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, OrderCmdResultCodes expectedCode)
        {
            await TestModifyReject(ocoOrder.WithOCO(mainOrder), expectedCode);
            await TestCancelOrder(mainOrder);
        }


        private async Task RunOCOTest(Func<OrderStateTemplate, OrderStateTemplate, Task> test, OrderBaseSet set, OrderBaseSet ocoSet, bool equalVolume = false)
        {
            async Task OCOTestEnviroment(OrderStateTemplate mainOrder)
            {
                var ocoOrder = ocoSet.BuildOrder().ForPending(ocoSet.Type == OrderType.Limit ? 4 : 2); //for pair limit/stop price for Buy must be less than Sell

                ocoOrder.OcoEqualVolume = equalVolume;

                await test(mainOrder, ocoOrder);
                await Task.Yield(); //wait modification real Oco and Main order

                if (!string.IsNullOrEmpty(ocoOrder.Id))
                    await RemoveOrder(ocoOrder);
            }

            await RunTest(OCOTestEnviroment, set, testInfo: $"{test.Method.Name} OCO=({ocoSet}) equalVolume={equalVolume}");
        }

        private static bool IsLimitStopPair(OrderBaseSet mainOrder, OrderBaseSet ocoOrder)
        {
            return mainOrder.IsSupportedStopPrice ^ ocoOrder.IsSupportedStopPrice;
        }
    }
}
