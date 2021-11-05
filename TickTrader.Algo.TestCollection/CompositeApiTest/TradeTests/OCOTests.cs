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


        protected async override Task RunTestGroup(TestParamsSet set)
        {
            async Task RunOpenOCOWithDiffSide(OrderType ocoType)
            {
                await RunOpenOCOTest(set, ocoType, false);
                await RunOpenOCOTest(set, ocoType, true);

                await ModifyOCOTests(set, ocoType, false);
                await ModifyOCOTests(set, ocoType, true);

                if (_useAD)
                {
                    await FillWithOCOTests(set, ocoType, false);
                    await FillWithOCOTests(set, ocoType, true);
                }
            }

            await RunOpenOCOWithDiffSide(OrderType.Stop);

            if (set.IsSupportedStopPrice)
                await RunOpenOCOWithDiffSide(OrderType.Limit);
        }

        private async Task RunOpenOCOTest(TestParamsSet set, OrderType ocoType, bool invertedSide)
        {
            var ocoOrder = BuildOCOOrder(set, ocoType, invertedSide);

            await RunOpenOCOWithDiffVolume(set, ocoOrder);
            await RunTest(t => OpenLinkedOCO(t, ocoOrder.Copy()), set, testInfo: GetTestInfo(nameof(OpenLinkedOCO), ocoOrder));
        }

        private async Task RunOpenOCOWithDiffVolume(TestParamsSet set, OrderStateTemplate ocoOrder)
        {
            Task RunOpenOCOWithEqualVolume(bool equalVolume)
            {
                return RunTest(t => OpenOCO(t, ocoOrder.Copy(), equalVolume), set, testInfo: $"{GetTestInfo(nameof(RunOpenOCOWithEqualVolume), ocoOrder)} equalVolume={equalVolume}");
            }

            await RunOpenOCOWithEqualVolume(false);
            await RunOpenOCOWithEqualVolume(true);
        }

        private async Task OpenOCO(OrderStateTemplate template, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await TestOpenOrder(template.ForPending());

            ocoOrder.Options = OrderExecOptions.OneCancelsTheOther;
            ocoOrder.OcoEqualVolume = equalVolume;
            ocoOrder.OcoRelatedOrderId = template.Id;

            await TestOpenOrder(ocoOrder, OrderEvents.Modify);
            await TestCancelOrder(template, OrderEvents.Cancel);
        }

        private async Task OpenLinkedOCO(OrderStateTemplate template, OrderStateTemplate ocoOrder)
        {
            template.Options = OrderExecOptions.OneCancelsTheOther;
            template.LinkedOrders.Add(ocoOrder);

            await TestOpenOrder(template.ForPending(), OrderEvents.Open);
            await TestCancelOrder(template, OrderEvents.Cancel);
        }

        private async Task FillWithOCOTests(TestParamsSet set, OrderType ocoType, bool invertedSide)
        {
            var mainOrder = set.BuildOrder();
            var ocoOrder = BuildOCOOrder(set, ocoType, invertedSide);

            await RunTest(t => FullFillOCOOrder(t, ocoOrder.Copy()), set, testInfo: GetTestInfo(nameof(FullFillOCOOrder), ocoOrder));
            await RunTest(t => PartialFillOCOOrder(t, ocoOrder.Copy()), set, testInfo: GetTestInfo(nameof(PartialFillOCOOrder), ocoOrder));
            await RunTest(t => OCOWithPartialFillOrder(t, ocoOrder.Copy()), set, testInfo: GetTestInfo(nameof(OCOWithPartialFillOrder), ocoOrder));
        }

        private async Task FullFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.ForPending());

            ocoOrder.Options = OrderExecOptions.OneCancelsTheOther;
            ocoOrder.OcoEqualVolume = true;
            ocoOrder.OcoRelatedOrderId = mainOrder.Id;
            ocoOrder.Comment = ADComments.ADCommentsList.WithActivate;

            await TestOpenOrder(ocoOrder, OrderEvents.Modify, OrderEvents.Fill, OrderEvents.Cancel);
            await mainOrder.Canceled.Task;
        }

        private async Task PartialFillOCOOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            await TestOpenOrder(mainOrder.ForPending());

            ocoOrder.Options = OrderExecOptions.OneCancelsTheOther;
            ocoOrder.OcoEqualVolume = false;
            ocoOrder.OcoRelatedOrderId = mainOrder.Id;
            ocoOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(ocoOrder.Volume * 0.8);

            await TestOpenOrder(ocoOrder, OrderEvents.Modify, OrderEvents.Fill, OrderEvents.Cancel);
            await mainOrder.Canceled.Task;

            await RemoveOrder(ocoOrder);
        }

        private async Task OCOWithPartialFillOrder(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder)
        {
            mainOrder.Comment = ADComments.ADCommentsList.WithPartialActivate(mainOrder.Volume * 0.4);

            await TestOpenOrder(mainOrder.ForPending(), OrderEvents.Fill);

            ocoOrder.Options = OrderExecOptions.OneCancelsTheOther;
            ocoOrder.OcoEqualVolume = true;
            ocoOrder.OcoRelatedOrderId = mainOrder.Id;

            await TestOpenOrder(ocoOrder, OrderEvents.Modify);
            await TestCancelOrder(mainOrder, OrderEvents.Cancel);
        }

        private async Task ModifyOCOTests(TestParamsSet set, OrderType ocoType, bool invertedSide)
        {
            async Task RunModifyOCOTest(Func<OrderStateTemplate, OrderStateTemplate, bool, Task> test, bool equalVolume)
            {
                var mainOrder = set.BuildOrder().ForPending();
                var ocoOrder = BuildOCOOrder(set, ocoType, invertedSide);

                await RunTest(_ => test(mainOrder, ocoOrder, equalVolume), null, mainOrder, testInfo: $"{GetTestInfo(test.Method.Name, ocoOrder)} equalVolume={equalVolume}");

                await TestCancelOrder(ocoOrder, OrderEvents.Cancel);

                if (string.IsNullOrEmpty(ocoOrder.OcoRelatedOrderId))
                    await TestCancelOrder(mainOrder);
                else
                    await mainOrder.Canceled.Task;
            }

            await RunModifyOCOTest(CreateOCOLinkViaModify, equalVolume: true);
            await RunModifyOCOTest(CreateOCOLinkViaModify, equalVolume: false);

            await RunModifyOCOTest(BreakOCOLinkViaModify, equalVolume: true);
            await RunModifyOCOTest(BreakOCOLinkViaModify, equalVolume: false);
        }

        private async Task CreateOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await TestOpenOrder(mainOrder.ForPending());
            await TestOpenOrder(ocoOrder);

            ocoOrder.OcoRelatedOrderId = mainOrder.Id;
            ocoOrder.OcoEqualVolume = equalVolume;
            ocoOrder.Options = OrderExecOptions.OneCancelsTheOther;

            await TestModifyOrder(ocoOrder, OrderEvents.Modify);
        }

        private async Task BreakOCOLinkViaModify(OrderStateTemplate mainOrder, OrderStateTemplate ocoOrder, bool equalVolume)
        {
            await CreateOCOLinkViaModify(mainOrder, ocoOrder, equalVolume);

            ocoOrder.Options = OrderExecOptions.None;
            ocoOrder.OcoRelatedOrderId = null;

            await TestModifyOrder(ocoOrder, OrderEvents.Modify);
        }

        private static OrderStateTemplate BuildOCOOrder(TestParamsSet set, OrderType ocoType, bool invertedSide)
        {
            var ocoOrder = set.BuildOrder(ocoType, TestParamsSet.BaseOrderVolume / 2).ForPending();

            if (invertedSide) //for pair limit/stop price for Buy must be less than Sell
                ocoOrder = (!ocoOrder).ForPending(ocoType == OrderType.Limit ? 4 : 2);

            return ocoOrder;
        }

        private static string GetTestInfo(string methodName, OrderStateTemplate ocoOrder)
        {
            return $"{methodName} OCO={ocoOrder.Type}-{ocoOrder.Side}";
        }
    }
}
