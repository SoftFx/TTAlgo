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

        private Task FillExecutionTest(OrderStateTemplate template) => OpenExecutionOrder(template);

        private Task FillIoCExecutionTest(OrderStateTemplate template)
        {
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;

            return OpenExecutionOrder(template);
        }

        private Task RejectIoCExecutionTest(OrderStateTemplate template)
        {
            template.Price = template.CalculatePrice(10);
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;

            return TestOpenReject(template);
        }

        private async Task FillByModifyExecutionTest(OrderStateTemplate template)
        {
            await TestOpenOrder(template);
            await ModifyForExecutionOrder(template);
        }

        private Task ExpirationExecutionTest(OrderStateTemplate template)
        {
            return TestOpenOrder(template.WithExpiration(4), OrderEvents.Expire);
        }

        private async Task CancelExecutionTest(OrderStateTemplate template)
        {
            await TestOpenOrder(template);
            await TestCancelOrder(template);
        }

        private Task TakeProfitExecutionTest(OrderStateTemplate template)
        {
            template.TP = template.CalculatePrice(-2);

            return OpenExecutionOrder(template);
        }

        private Task StopLossExecutionTest(OrderStateTemplate template)
        {
            template.SL = template.CalculatePrice(2);

            return OpenExecutionOrder(template);
        }
    }
}