using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ExecutionTests : TestGroupBase
    {
        protected override string GroupName => nameof(ExecutionTests);


        protected override async Task RunTestGroup(OrderBaseSet set)
        {
            await RunTest(FillExecutionTest, set);

            if (set.IsSupportedIoC)
                await RunTest(FillIoCExecutionTest, set);

            if (set.IsSupportedRejectedIoC)
                await RunTest(RejectIoCExecutionTest, set);

            if (!set.IsInstantOrder)
            {
                await RunTest(FillByModifyExecutionTest, set);
                await RunTest(ExpirationExecutionTest, set);
                await RunTest(CancelExecutionTest, set);
            }

            if (set.IsGrossAcc)
            {
                await RunTest(TakeProfitExecutionTest, set);
                await RunTest(StopLossExecutionTest, set);
            }
        }

        private async Task FillExecutionTest(OrderStateTemplate template)
        {
            await OpenExecutionOrder(template);
        }

        private async Task FillIoCExecutionTest(OrderStateTemplate template)
        {
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;

            await OpenExecutionOrder(template);
        }

        private async Task RejectIoCExecutionTest(OrderStateTemplate template)
        {
            template.Price = template.CalculatePrice(10);
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;

            await TestOpenReject(template);
        }

        private async Task FillByModifyExecutionTest(OrderStateTemplate template)
        {
            await TestOpenOrder(template);
            await ModifyForExecutionOrder(template);
        }

        private async Task ExpirationExecutionTest(OrderStateTemplate template)
        {
            await TestOpenOrder(template.WithExpiration(4), OrderEvents.Expire);
        }

        private async Task CancelExecutionTest(OrderStateTemplate template)
        {
            await TestOpenOrder(template);
            await TestCancelOrder(template);
        }

        private async Task TakeProfitExecutionTest(OrderStateTemplate template)
        {
            template.TP = template.CalculatePrice(-2);

            await OpenExecutionOrder(template);
        }

        private async Task StopLossExecutionTest(OrderStateTemplate template)
        {
            template.SL = template.CalculatePrice(2);

            await OpenExecutionOrder(template);
        }
    }
}