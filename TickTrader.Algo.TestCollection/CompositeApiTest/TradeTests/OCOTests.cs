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
            await RunOpenOCOTest(set, OrderType.Stop, false, false);

            if (set.IsSupportedStopPrice)
                await RunOpenOCOTest(set, OrderType.Limit, false, false);
        }

        private async Task RunOpenOCOTest(TestParamsSet set, OrderType ocoType, bool inverserSide, bool equalVolume)
        {
            await RunTest(t => OpenOCO(t, ocoType, inverserSide, equalVolume), set);
        }

        private async Task OpenOCO(OrderTemplate template, OrderType ocoType, bool inversed, bool equalVolume)
        {
            await TestOpenOrder(template.ForPending());

            var ocoOrder = template.BuildOrder(ocoType, template.Volume / 2);

            if (inversed)
                ocoOrder = !ocoOrder;

            ocoOrder.Options = OrderExecOptions.OneCancelsTheOther;
            ocoOrder.OcoEqualVolume = equalVolume;
            ocoOrder.OcoRelatedOrderId = template.Id;

            await TestOpenOrder(ocoOrder.ForPending(), OrderEvents.Modify);
            await TestCancelOrder(template, OrderEvents.Cancel);
        }
    }
}
