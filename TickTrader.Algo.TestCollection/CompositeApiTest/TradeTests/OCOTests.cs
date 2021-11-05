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
            await RunOCOTest(PartialFillOCOOrder, set, ocoSet, false);
            await RunOCOTest(OCOWithPartialFillOrder, set, ocoSet, true);
        }

        private async Task RunModifyTests(OrderBaseSet set, OrderBaseSet ocoSet)
        {
            await RunOCOTest(CreateOCOLinkViaModify, set, ocoSet, true);
            await RunOCOTest(CreateOCOLinkViaModify, set, ocoSet, false);

            await RunOCOTest(BreakOCOLinkViaModify, set, ocoSet, true);
            await RunOCOTest(BreakOCOLinkViaModify, set, ocoSet, false);
        }


        private async Task OpenOCO(OrderStateTemplate template, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await TestOpenOrder(template.ForPending());
            await TestOpenOrder(ocoOrder.WithOCO(template, equalVolume), OrderEvents.Modify);
        }

        private Task OpenLinkedOCO(OrderStateTemplate template, OrderStateTemplate ocoOrder, bool _)
        {
            return TestOpenOrder(template.WithLinkedOCO(ocoOrder).ForPending(), OrderEvents.Open);
        }


        private async Task FullFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            ocoOrder.Comment = ADComments.ADCommentsList.WithActivate;

            await TestOpenOcoOrderWithActivation(mainOrder, ocoOrder, equalVolume);
        }

        private async Task PartialFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            ocoOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(ocoOrder.Volume * 0.8);

            await TestOpenOcoOrderWithActivation(mainOrder, ocoOrder, equalVolume);
            await TestCancelOrder(ocoOrder);
        }

        private async Task TestOpenOcoOrderWithActivation(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await TestOpenOrder(mainOrder.ForPending());
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder, equalVolume), OrderEvents.Modify, OrderEvents.Fill, OrderEvents.Cancel);
            await mainOrder.Canceled.Task;
        }

        private async Task OCOWithPartialFillOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            mainOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(mainOrder.Volume * 0.4);

            await TestOpenOrder(mainOrder.ForPending(), OrderEvents.Fill);
            await TestOpenOrder(ocoOrder.WithOCO(mainOrder, equalVolume), OrderEvents.Modify);
        }


        private async Task CreateOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await TestOpenOrder(mainOrder.ForPending());
            await TestOpenOrder(ocoOrder);

            await TestModifyOrder(ocoOrder.WithOCO(mainOrder, equalVolume), OrderEvents.Modify);
        }

        private async Task BreakOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await CreateOCOLinkViaModify(mainOrder, ocoOrder, equalVolume);

            await TestModifyOrder(ocoOrder.WithRemovedOCO(), OrderEvents.Modify);

            await TestCancelOrder(mainOrder);
            await TestCancelOrder(ocoOrder);
        }


        private async Task RunOCOTest(Func<OrderStateTemplate, OrderStateTemplate, bool, Task> test, OrderBaseSet set, OrderBaseSet ocoSet, bool equalVolume = false)
        {
            var ocoOrder = ocoSet.BuildOrder().ForPending(ocoSet.Type == OrderType.Limit ? 4 : 2); //for pair limit/stop price for Buy must be less than Sell

            await RunTest(t => test(t, ocoOrder, equalVolume), set, testInfo: $"{test.Method.Name} OCO=({ocoSet}) equalVolume={equalVolume}");
            await RemoveOrder(ocoOrder);
        }
    }
}
