using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ModificationTests : TestGroupBase
    {
        private enum TestAction { Add, Modify, Delete };

        private readonly TimeSpan WaitEventsTimeout = TimeSpan.FromMilliseconds(500);


        protected override string GroupName => nameof(ModificationTests);


        protected override async Task RunTestGroup(TestParamsSet set)
        {
            var template = set.BuildOrder().ForPending();

            await TestOpenOrder(template).WithTimeoutAfter(WaitEventsTimeout);

            await VolumeModifyTests(template, 2);
            await RunTagModifyTests(template);
            await RunCommentModifyTests(template);
            await RunExpirationModifyTests(template);

            if (template.IsSupportedMaxVisibleVolume)
            {
                await RunMaxVisibleVolumeModifyTests(template);
                await PriceModifyTests(template, 4);
            }

            if (template.IsSupportedStopPrice)
                await StopPriceModifyTests(template, 4);

            if (template.IsGrossAcc)
            {
                await RunTakeProfitModifyTests(template);
                await RunStopLossModifyTests(template);
            }

            if (template.IsStopLimit)
                await RunOptionsModifyTests(template);

            if (template.IsSupportedSlippage) //should be last, if slippage = 0 server behavior is unpredictable
                await RunSlippageModifyTest(template);

            await RemoveOrder(template).WithTimeoutAfter(WaitEventsTimeout); ;
        }


        private async Task RunMaxVisibleVolumeModifyTests(OrderTemplate template)
        {
            await MaxVisibleVolumeTest(template, TestAction.Add, TestParamsSet.BaseOrderVolume);
            await MaxVisibleVolumeTest(template, TestAction.Modify, TestParamsSet.Symbol.MinTradeVolume);
            await MaxVisibleVolumeTest(template, TestAction.Delete, -1);
        }

        private async Task RunExpirationModifyTests(OrderTemplate template)
        {
            await ExpirationTest(template, TestAction.Add, DateTime.Now.AddYears(1));
            await ExpirationTest(template, TestAction.Modify, DateTime.Now.AddYears(2));
            await ExpirationTest(template, TestAction.Delete, DateTime.MinValue);
        }

        private async Task RunTakeProfitModifyTests(OrderTemplate template)
        {
            await TakeProfitTest(template, TestAction.Add, 4);
            await TakeProfitTest(template, TestAction.Modify, 5);
            await TakeProfitTest(template, TestAction.Delete, 0);
        }

        private async Task RunStopLossModifyTests(OrderTemplate template)
        {
            await StopLossTest(template, TestAction.Add, -4);
            await StopLossTest(template, TestAction.Modify, -5);
            await StopLossTest(template, TestAction.Delete, 0);
        }

        private async Task RunCommentModifyTests(OrderTemplate template)
        {
            await CommentTest(template, TestAction.Add, "New_comment");
            await CommentTest(template, TestAction.Modify, "Replace_Comment");
            await CommentTest(template, TestAction.Delete, string.Empty);
        }

        private async Task RunTagModifyTests(OrderTemplate template)
        {
            await TagTest(template, TestAction.Add, "New_tag");
            await TagTest(template, TestAction.Modify, "Replace_tag");
            await TagTest(template, TestAction.Delete, string.Empty);
        }

        private async Task RunOptionsModifyTests(OrderTemplate template)
        {
            await OptionsTest(template, TestAction.Add, OrderExecOptions.ImmediateOrCancel);
            await OptionsTest(template, TestAction.Delete, OrderExecOptions.None);
        }

        private async Task RunSlippageModifyTest(OrderTemplate template)
        {
            await SlippageTest(template, TestAction.Add, TestParamsSet.Symbol.Slippage / 2);
            await SlippageTest(template, TestAction.Modify, TestParamsSet.Symbol.Slippage * 2);
            await SlippageTest(template, TestAction.Delete, 0);
        }


        private async Task VolumeModifyTests(OrderTemplate template, int coef)
        {
            template.Volume *= coef;

            await RunModifyTest(template);
        }

        private async Task PriceModifyTests(OrderTemplate template, int coef)
        {
            template.Price = template.ForPending(coef).Price;

            await RunModifyTest(template);
        }

        private async Task StopPriceModifyTests(OrderTemplate template, int coef)
        {
            template.StopPrice = template.ForPending(coef).StopPrice;

            await RunModifyTest(template);
        }

        private async Task ExpirationTest(OrderTemplate template, TestAction action, DateTime? value)
        {
            template.Expiration = value;

            await RunModifyTest(template, action);
        }

        private async Task MaxVisibleVolumeTest(OrderTemplate template, TestAction action, double value)
        {
            template.MaxVisibleVolume = value;

            await RunModifyTest(template, action);
        }

        private async Task TakeProfitTest(OrderTemplate template, TestAction action, int coef)
        {
            template.TP = template.CalculatePrice(coef);

            await RunModifyTest(template, action);
        }

        private async Task StopLossTest(OrderTemplate template, TestAction action, int coef)
        {
            template.SL = template.CalculatePrice(coef);

            await RunModifyTest(template, action);
        }

        private async Task CommentTest(OrderTemplate template, TestAction action, string comment)
        {
            template.Comment = comment;

            await RunModifyTest(template, action);
        }

        private async Task TagTest(OrderTemplate template, TestAction action, string tag)
        {
            template.Tag = tag;

            await RunModifyTest(template, action);
        }

        private async Task OptionsTest(OrderTemplate template, TestAction action, OrderExecOptions value)
        {
            template.Options = value;

            await RunModifyTest(template, action);
        }

        private async Task SlippageTest(OrderTemplate template, TestAction action, double? value)
        {
            template.Slippage = value;

            await RunModifyTest(template, action);
        }

        private async Task RunModifyTest(OrderTemplate template, TestAction action = TestAction.Modify, [CallerMemberName] string testName = "")
        {
            await RunTest(TestModifyOrder, null, template, $"{action} {testName}");
        }
    }
}