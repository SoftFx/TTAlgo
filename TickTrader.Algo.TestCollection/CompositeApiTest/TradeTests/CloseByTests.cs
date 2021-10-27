using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class CloseByTests : TestGroupBase
    {
        protected override string GroupName => nameof(CloseByTests);


        protected async override Task RunTestGroup(TestParamsSet set)
        {
            await CloseBigBySmallTest(set);
            await CloseSmallByBigTest(set);
            await CloseByEvenTest(set);
        }


        private Task CloseBigBySmallTest(TestParamsSet set) => RunCloseByTest(set, TestParamsSet.BaseOrderVolume / 2);

        private Task CloseSmallByBigTest(TestParamsSet set) => RunCloseByTest(set, TestParamsSet.BaseOrderVolume * 2);

        private Task CloseByEvenTest(TestParamsSet set) => RunCloseByTest(set, null);


        private async Task RunCloseByTest(TestParamsSet set, double? volume, [CallerMemberName] string testInfo = null)
        {
            await RunTest(t => RunCloseByTest(t, volume), set, testInfo: testInfo);
        }

        private async Task RunCloseByTest(OrderTemplate template, double? closeVolume)
        {
            var inversed = RegisterAdditionalTemplate(template.InversedCopy(closeVolume));

            await OpenOrderAndWaitExecution(template.ForGrossPositionPending(4, "First"));
            await OpenOrderAndWaitExecution(inversed.ForGrossPositionPending(5, "Second"));

            var resultTemplate = await TestCloseByOrders(template, inversed);

            if (!resultTemplate.IsNull)
                await RemoveOrder(resultTemplate);
        }
    }
}
