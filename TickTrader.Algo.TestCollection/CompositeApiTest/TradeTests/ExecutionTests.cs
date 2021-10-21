using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ExecutionTests : TestGroupBase
    {
        private readonly TimeSpan TimeToExpire = TimeSpan.FromSeconds(4);


        protected override string GroupName => nameof(ExecutionTests);


        protected override async Task RunTestGroup(TestParamsSet set)
        {
            await RunTest(FillExecutionTest, set);

            if (set.IsSupportedIoC)
                await RunTest(FillIoCExecutionTest, set);
            //add rejectIocTest

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

        private async Task FillExecutionTest(OrderTemplate template)
        {
            await OpenExecutionOrder(template);
        }

        private async Task FillIoCExecutionTest(OrderTemplate template)
        {
            template.Options = Api.OrderExecOptions.ImmediateOrCancel;

            await OpenExecutionOrder(template);
        }

        private async Task FillByModifyExecutionTest(OrderTemplate template)
        {
            await TestOpenOrder(template.ForPending());
            await ModifyForExecutionOrder(template);
        }

        private async Task ExpirationExecutionTest(OrderTemplate template)
        {
            template.Expiration = DateTime.Now + TimeToExpire;

            await TestOpenOrder(template.ForPending(), OrderEvents.Expire);
        }

        private async Task CancelExecutionTest(OrderTemplate template)
        {
            await TestOpenOrder(template.ForPending());
            await TestCancelOrder(template);
        }

        private async Task TakeProfitExecutionTest(OrderTemplate template)
        {
            template.TP = template.CalculatePrice(-2);

            await OpenExecutionOrder(template);
        }

        private async Task StopLossExecutionTest(OrderTemplate template)
        {
            template.SL = template.CalculatePrice(2);

            await OpenExecutionOrder(template);
        }
    }
}