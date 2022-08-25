using System.Threading.Tasks;
using TickTrader.Algo.TestCollection.CompositeApiTest.ADComments;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ADTests : TestGroupBase
    {
        protected override string GroupName => nameof(ADTests);


        protected async override Task RunTestGroup(OrderBaseSet set)
        {
            await RunTest(ADReject, set);

            if (set.IsSupportedRejectedIoC)
                await RunTest(ADConfirm, set);

            if (!set.IsInstantOrder && !set.IsStopLimit)
            {
                await RunTest(ADActivate, set);

                if (set.IsSupportedSlippage)
                    await ADSlippageTests(set);
            }
        }

        private async Task ADSlippageTests(OrderBaseSet set)
        {
            Task RunActivateWithSlippage(double? slippage)
            {
                return RunTest(t => ADActivateWithSlippage(t, slippage), set, testInfo: nameof(ADActivateWithSlippage));
            }

            await RunActivateWithSlippage(null);
            await RunActivateWithSlippage(0.0);
            await RunActivateWithSlippage(Symbol.Slippage / 2);
            await RunActivateWithSlippage(Symbol.Slippage * 2);
        }



        private async Task ADActivateWithSlippage(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = slippage;

            await ADActivate(template);
        }

        private async Task ADReject(OrderStateTemplate template)
        {
            template.Comment = ADCommentsList.WithReject;

            await TestOpenReject(template.ForExecuting());
        }

        private Task ADConfirm(OrderStateTemplate template)
        {
            template.Price = template.CalculatePrice(10);
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;
            template.Comment = ADCommentsList.WithConfirm;

            return OpenExecutionOrder(template); //change wait event logic
        }

        private async Task ADActivate(OrderStateTemplate template)
        {
            template.Comment = ADCommentsList.WithPartialToFullActivate(0.2 * OrderBaseSet.BaseOrderVolume);

            await OpenAndWaitExecution(template, Events.Order.PartialFill);
            await template.FilledParts[0].OpenedGrossPosition.Task;
            await RemoveOrder(template);
        }
    }
}