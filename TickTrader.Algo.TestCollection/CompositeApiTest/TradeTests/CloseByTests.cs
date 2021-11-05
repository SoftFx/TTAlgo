using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class CloseByTests : TestGroupBase
    {
        protected override string GroupName => nameof(CloseByTests);


        protected async override Task RunTestGroup(OrderBaseSet set)
        {
            await CloseBigBySmallTest(set);
            await CloseSmallByBigTest(set);
            await CloseByEvenTest(set);
        }


        private Task CloseBigBySmallTest(OrderBaseSet set) => RunCloseByTest(set, OrderBaseSet.BaseOrderVolume / 2);

        private Task CloseSmallByBigTest(OrderBaseSet set) => RunCloseByTest(set, OrderBaseSet.BaseOrderVolume * 2);

        private Task CloseByEvenTest(OrderBaseSet set) => RunCloseByTest(set, null);


        private async Task RunCloseByTest(OrderBaseSet set, double? volume, [CallerMemberName] string testInfo = null)
        {
            await RunTest(t => RunCloseByTest(t, volume), set, testInfo: testInfo);
        }

        private async Task RunCloseByTest(OrderStateTemplate template, double? closeVolume)
        {
            var inversed = !template.Copy(closeVolume);

            await OpenOrderAndWaitExecution(template.ForGrossPositionPending(4, "First"));
            await OpenOrderAndWaitExecution(inversed.ForGrossPositionPending(5, "Second"));

            var resultTemplate = await TestCloseByOrders(template, inversed);

            if (!resultTemplate.IsNull)
                await RemoveOrder(resultTemplate);
        }
    }
}
