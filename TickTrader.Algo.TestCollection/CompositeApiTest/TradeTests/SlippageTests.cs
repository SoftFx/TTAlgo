using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class SlippageTests : TestGroupBase
    {
        protected override string GroupName => nameof(SlippageTests);


        protected async override Task RunTestGroup(TestParamsSet set)
        {
            if (set.IsInstantOrder)
                await RunSlippageTests(ExecutionOrderWithSlippageTest, set, nameof(ExecutionOrderWithSlippageTest));
            else
                await RunSlippageTests(OpenPendingWithSlippageTest, set, nameof(OpenPendingWithSlippageTest));
        }


        private async Task RunSlippageTests(Func<OrderStateTemplate, double?, Task> test, TestParamsSet set, string testInfo)
        {
            await RunTest(t => test(t, null), set, testInfo: testInfo);
            await RunTest(t => test(t, 0.0), set, testInfo: testInfo);
            await RunTest(t => test(t, TestParamsSet.Symbol.Slippage / 2), set, testInfo: testInfo);
            await RunTest(t => test(t, TestParamsSet.Symbol.Slippage * 2), set, testInfo: testInfo);
        }

        private async Task ExecutionOrderWithSlippageTest(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = slippage;

            await OpenExecutionOrder(template);
        }

        private async Task OpenPendingWithSlippageTest(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = slippage;

            await TestOpenOrder(template.ForPending());
            await TestCancelOrder(template);
        }
    }
}
