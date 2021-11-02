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
            }

            await RunOpenOCOWithDiffSide(OrderType.Stop);

            if (set.IsSupportedStopPrice)
                await RunOpenOCOWithDiffSide(OrderType.Limit);
        }

        private async Task RunOpenOCOTest(TestParamsSet set, OrderType ocoType, bool invertedSide)
        {
            var ocoOrder = set.BuildOrder(ocoType, TestParamsSet.BaseOrderVolume / 2).ForPending();

            if (invertedSide) //for pair limit/stop price for Buy must be less than Sell
                ocoOrder = (!ocoOrder).ForPending(ocoType == OrderType.Limit ? 4 : 2);

            await RunOpenOCOWithDiffVolume(set, ocoOrder);
            await RunTest(t => OpenLinkedOCO(t, ocoOrder.Copy()), set, testInfo: GetTestInfo(nameof(OpenLinkedOCO), ocoOrder));
        }

        private async Task RunOpenOCOWithDiffVolume(TestParamsSet set, OrderStateTemplate ocoOrder)
        {
            Task RunOpenOCOWithEqualVolume(bool equalVolume)
            {
                return RunTest(t => OpenOCO(t, ocoOrder.Copy(), equalVolume), set, testInfo: GetTestInfo(nameof(RunOpenOCOWithEqualVolume), ocoOrder));
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

        private static string GetTestInfo(string methodName, OrderStateTemplate ocoOrder)
        {
            return $"{methodName} OCO={ocoOrder.Type}-{ocoOrder.Side}";
        }
    }
}
