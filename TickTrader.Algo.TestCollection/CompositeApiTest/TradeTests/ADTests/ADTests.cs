using System.Threading.Tasks;
using TickTrader.Algo.TestCollection.CompositeApiTest.ADComments;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ADTests : TestGroupBase
    {
        private readonly double _partialFillVolume;

        protected override string GroupName => nameof(ADTests);


        internal ADTests()
        {
            _partialFillVolume = 0.2 * TestParamsSet.BaseOrderVolume * TestParamsSet.Symbol.ContractSize;
        }


        protected async override Task RunTestGroup(TestParamsSet set)
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

        private async Task ADSlippageTests(TestParamsSet set)
        {
            await RunActivateWithSlippage(set, null);
            await RunActivateWithSlippage(set, 0.0);
            await RunActivateWithSlippage(set, TestParamsSet.Symbol.Slippage / 2);
            await RunActivateWithSlippage(set, TestParamsSet.Symbol.Slippage * 2);
        }

        private async Task RunActivateWithSlippage(TestParamsSet set, double? slippage)
        {
            await RunTest(t => ADActivateWithSlippage(t, slippage), set, testInfo: nameof(ADActivateWithSlippage));
        }

        private async Task ADActivateWithSlippage(OrderStateTemplate template, double? slippage)
        {
            template.Slippage = slippage;

            await ADActivate(template);
        }

        private async Task ADReject(OrderStateTemplate template)
        {
            template.Comment = ADCommentsList.WithReject;

            await OpenRejectOrder(template.ForExecuting());
        }

        private async Task ADConfirm(OrderStateTemplate template)
        {
            template.Price = template.CalculatePrice(10);
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;
            template.Comment = ADCommentsList.WithConfirm;

            await OpenOrderAndWaitExecution(template);
            await ClearTestEnviroment(template);
        }

        private async Task ADActivate(OrderStateTemplate template)
        {
            template.Comment = ADCommentsList.WithActivate(_partialFillVolume);

            await OpenOrderAndWaitExecution(template.ForPending(), template.IsGrossAcc ?
                  OrderEvents.PartialFillGrossOrderEvents : OrderEvents.PartialFillOrderEvents);
            await ClearTestEnviroment(template);
        }
    }
}
