using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ModificationTests : TestGroupBase
    {
        private enum TestAction { Add, Modify, Delete };


        protected override string GroupName => nameof(ModificationTests);


        protected override async Task RunTestGroup(OrderBaseSet set)
        {
            var template = set.BuildOrder();

            await TestOpenOrder(template);

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

            await RemoveOrder(template);
        }


        private async Task RunMaxVisibleVolumeModifyTests(OrderStateTemplate template)
        {
            await MaxVisibleVolumeTest(template, TestAction.Add, OrderBaseSet.BaseOrderVolume);
            await MaxVisibleVolumeTest(template, TestAction.Modify, Symbol.MinTradeVolume);
            await MaxVisibleVolumeTest(template, TestAction.Delete, -1);
        }

        private async Task RunExpirationModifyTests(OrderStateTemplate template)
        {
            await ExpirationTest(template, TestAction.Add, Bot.UtcNow.AddYears(1));
            await ExpirationTest(template, TestAction.Modify, Bot.UtcNow.AddYears(2));
            await ExpirationTest(template, TestAction.Delete, DateTime.MinValue);
        }

        private async Task RunTakeProfitModifyTests(OrderStateTemplate template)
        {
            await TakeProfitTest(template, TestAction.Add, 4);
            await TakeProfitTest(template, TestAction.Modify, 5);
            await TakeProfitTest(template, TestAction.Delete, 0);
        }

        private async Task RunStopLossModifyTests(OrderStateTemplate template)
        {
            await StopLossTest(template, TestAction.Add, -4);
            await StopLossTest(template, TestAction.Modify, -5);
            await StopLossTest(template, TestAction.Delete, 0);
        }

        private async Task RunCommentModifyTests(OrderStateTemplate template)
        {
            await CommentTest(template, TestAction.Add, "New_comment");
            await CommentTest(template, TestAction.Modify, "Replace_Comment");
            await CommentTest(template, TestAction.Delete, string.Empty);
        }

        private async Task RunTagModifyTests(OrderStateTemplate template)
        {
            await TagTest(template, TestAction.Add, "New_tag");
            await TagTest(template, TestAction.Modify, "Replace_tag");
            await TagTest(template, TestAction.Delete, string.Empty);
        }

        private async Task RunOptionsModifyTests(OrderStateTemplate template)
        {
            await OptionsTest(template, TestAction.Add, OrderExecOptions.ImmediateOrCancel);
            await OptionsTest(template, TestAction.Delete, OrderExecOptions.None);
        }

        private async Task RunSlippageModifyTest(OrderStateTemplate template)
        {
            await SlippageTest(template, TestAction.Add, Symbol.Slippage / 2);
            await SlippageTest(template, TestAction.Modify, Symbol.Slippage * 2);
            await SlippageTest(template, TestAction.Delete, 0);
        }


        private Task VolumeModifyTests(OrderStateTemplate template, int coef)
        {
            template.Volume *= coef;

            return RunModifyTest(template);
        }

        private Task PriceModifyTests(OrderStateTemplate template, int coef)
        {
            template.Price = template.ForPending(coef).Price;

            return RunModifyTest(template);
        }

        private Task StopPriceModifyTests(OrderStateTemplate template, int coef)
        {
            template.StopPrice = template.ForPending(coef).StopPrice;

            return RunModifyTest(template);
        }

        private Task ExpirationTest(OrderStateTemplate template, TestAction action, DateTime? value)
        {
            template.Expiration = value;

            return RunModifyTest(template, action);
        }

        private Task MaxVisibleVolumeTest(OrderStateTemplate template, TestAction action, double value)
        {
            template.MaxVisibleVolume = value;

            return RunModifyTest(template, action);
        }

        private Task TakeProfitTest(OrderStateTemplate template, TestAction action, int coef)
        {
            template.TP = template.CalculatePrice(coef);

            return RunModifyTest(template, action);
        }

        private Task StopLossTest(OrderStateTemplate template, TestAction action, int coef)
        {
            template.SL = template.CalculatePrice(coef);

            return RunModifyTest(template, action);
        }

        private Task CommentTest(OrderStateTemplate template, TestAction action, string comment)
        {
            template.Comment = comment;

            return RunModifyTest(template, action);
        }

        private Task TagTest(OrderStateTemplate template, TestAction action, string tag)
        {
            template.Tag = tag;

            return RunModifyTest(template, action);
        }

        private Task OptionsTest(OrderStateTemplate template, TestAction action, OrderExecOptions value)
        {
            template.Options = value;

            return RunModifyTest(template, action);
        }

        private Task SlippageTest(OrderStateTemplate template, TestAction action, double? value)
        {
            template.Slippage = value;

            return RunModifyTest(template, action);
        }

        private Task RunModifyTest(OrderStateTemplate template, TestAction action = TestAction.Modify, [CallerMemberName] string testName = "")
        {
            return RunTest(t => TestModifyOrder(t), null, template, $"{action} {testName}");
        }
    }
}