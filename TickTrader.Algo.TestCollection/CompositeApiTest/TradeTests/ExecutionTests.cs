using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ExecutionTests : TestGroupBase
    {
        private enum TestAcion { Fill, FillByModify, RejectIoC, ExecutionTP, ExecutionSL, Cancel, Expiration }


        protected override string GroupName => nameof(ExecutionTests);

        protected override string CurrentTestDatails { get; set; }


        protected override async Task RunTestGroup(TestParamsSet set)
        {
            var template = set.BuildOrder().ForExecuting();

            await FillTest(template);
        }

        private async Task FillTest(OrderTemplate template)
        {
            //async Task OpenExecutionOrder

            await RunExecutionTest(TestAcion.Fill, () => TestOpenOrder(template));
        }

        private async Task RunExecutionTest(TestAcion action, Func<Task> test, [CallerMemberName] string testName = "")
        {
            CurrentTestDatails = $"{action} {testName}";

            await RunTest(test);
        }
    }
}