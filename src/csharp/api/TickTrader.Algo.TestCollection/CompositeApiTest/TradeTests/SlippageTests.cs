using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class SlippageTests : TestGroupBase
    {
        protected override string GroupName => nameof(SlippageTests);

        private delegate Task SlippageTest(OrderStateTemplate template, double? slippage);


        protected async override Task RunTestGroup(OrderBaseSet set)
        {
            if (set.IsInstantOrder)
                await RunSlippageTests(ExecutionWithSlippageTest, set);
            else
            {
                await RunSlippageTests(OpenStopWithSlippageTest, set);
                await RunSlippageTests(ExecutionWithSlippageTest, set);
                await RunSlippageTests(ExecutionByModifyWithSlippage, set);
            }
        }


        private async Task RunSlippageTests(SlippageTest test, OrderBaseSet set)
        {
            var testName = test.Method.Name;

            await RunTest(t => test(t, null), set, testInfo: testName);
            await RunTest(t => test(t, 0.0), set, testInfo: testName);
            await RunTest(t => test(t, Symbol.Slippage / 2), set, testInfo: testName);
            await RunTest(t => test(t, Symbol.Slippage * 2), set, testInfo: testName);
        }

        private async Task OpenStopWithSlippageTest(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = slippage;

            await TestOpenOrder(template);
            await TestCancelOrder(template);
        }

        private Task ExecutionWithSlippageTest(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = slippage;

            return OpenExecutionOrder(template);
        }


        private async Task ExecutionByModifyWithSlippage(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = 0.0;

            await TestOpenOrder(template);

            template.Slippage = slippage;

            await ExecutionByModifyOrder(template);
        }
    }
}