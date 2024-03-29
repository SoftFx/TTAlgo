﻿using System.Runtime.CompilerServices;
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


        private Task RunCloseByTest(OrderBaseSet set, double? volume, [CallerMemberName] string testInfo = null)
        {
            return RunTest(t => RunCloseByTest(t, volume), set, testInfo: testInfo);
        }

        private async Task RunCloseByTest(OrderStateTemplate template, double? closeVolume)
        {
            var inversed = !template.Copy(closeVolume);

            await OpenAndWaitExecution(template.ForGrossPositionPending(40, "First"));
            await OpenAndWaitExecution(inversed.ForGrossPositionPending(50, "Second"));

            var resultTemplate = await TestCloseByOrders(template, inversed);

            if (!resultTemplate.IsNull)
            {
                await resultTemplate.Opened.Task;
                await RemoveOrder(resultTemplate);
            }
        }
    }
}
