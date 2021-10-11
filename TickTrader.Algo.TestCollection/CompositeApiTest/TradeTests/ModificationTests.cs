using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class ModificationTests : TestGroupBase
    {
        private enum TestPropertyAction { Add, Modify, Delete };

        protected override string GroupName => nameof(ModificationTests);

        protected override string CurrentTestDatails { get; set; }


        public ModificationTests() { }


        protected override async Task RunTestGroup(TestParamsSet set)
        {
            var template = set.BuildOrder().ForPending();

            await TestOpenOrder(template);

            if (!template.IsPosition)
            {
                await VolumeModifyTests(template, 2);
                await RunExpirationModifyTests(template);
            }

            if (template.IsSupportedMaxVisibleVolume)
            {
                await PerformMaxVisibleVolumeModifyTests(template);
                await PriceModifyTests(template, 2);
            }

            if (template.IsGrossAcc)
            {
                await RunTakeProfitModifyTests(template);
                await RunStopLossModifyTests(template);
            }

            await RunCommentModifyTests(template);
            await RunTagModifyTests(template);

            await RemovePendingOrder(template);
        }


        private async Task PerformMaxVisibleVolumeModifyTests(OrderTemplate template)
        {
            await MaxVisibleVolumeTest(template, TestPropertyAction.Add, TestParamsSet.BaseOrderVolume);
            await MaxVisibleVolumeTest(template, TestPropertyAction.Modify, TestParamsSet.Symbol.MinTradeVolume);
            await MaxVisibleVolumeTest(template, TestPropertyAction.Delete, -1);
        }

        private async Task RunExpirationModifyTests(OrderTemplate template)
        {
            await ExpirationTest(template, TestPropertyAction.Add, DateTime.Now.AddYears(1));
            await ExpirationTest(template, TestPropertyAction.Modify, DateTime.Now.AddYears(2));
            await ExpirationTest(template, TestPropertyAction.Delete, DateTime.MinValue);
        }

        private async Task RunTakeProfitModifyTests(OrderTemplate template)
        {
            await TakeProfitTest(template, TestPropertyAction.Add, 4);
            await TakeProfitTest(template, TestPropertyAction.Modify, 5);
            await TakeProfitTest(template, TestPropertyAction.Delete, 0);
        }

        private async Task RunStopLossModifyTests(OrderTemplate template)
        {
            await StopLossTest(template, TestPropertyAction.Add, -4);
            await StopLossTest(template, TestPropertyAction.Modify, -5);
            await StopLossTest(template, TestPropertyAction.Delete, 0);
        }

        private async Task RunCommentModifyTests(OrderTemplate template)
        {
            await CommentTest(template, TestPropertyAction.Add, "New_comment");
            await CommentTest(template, TestPropertyAction.Modify, "Replace_Comment");
            await CommentTest(template, TestPropertyAction.Delete, string.Empty);
        }

        private async Task RunTagModifyTests(OrderTemplate template)
        {
            await TagTest(template, TestPropertyAction.Add, "New_tag");
            await TagTest(template, TestPropertyAction.Modify, "Replace_tag");
            await TagTest(template, TestPropertyAction.Delete, string.Empty);
        }


        private async Task VolumeModifyTests(OrderTemplate template, int coef)
        {
            template.Volume *= coef;

            await RunModifyTest(template, TestPropertyAction.Modify);
        }

        private async Task PriceModifyTests(OrderTemplate template, int coef)
        {
            template.Price = template.ForPending(coef).Price;

            await RunModifyTest(template, TestPropertyAction.Modify);
        }

        private async Task ExpirationTest(OrderTemplate template, TestPropertyAction action, DateTime? value)
        {
            template.Expiration = value;

            await RunModifyTest(template, action);
        }

        private async Task MaxVisibleVolumeTest(OrderTemplate template, TestPropertyAction action, double value)
        {
            template.MaxVisibleVolume = value;

            await RunModifyTest(template, action);
        }

        private async Task TakeProfitTest(OrderTemplate template, TestPropertyAction action, int coef)
        {
            template.TP = template.CalculatePrice(coef);

            await RunModifyTest(template, action);
        }

        private async Task StopLossTest(OrderTemplate template, TestPropertyAction action, int coef)
        {
            template.SL = template.CalculatePrice(coef);

            await RunModifyTest(template, action);
        }

        private async Task CommentTest(OrderTemplate template, TestPropertyAction action, string comment)
        {
            template.Comment = comment;

            await RunModifyTest(template, action);
        }

        private async Task TagTest(OrderTemplate template, TestPropertyAction action, string tag)
        {
            template.Tag = tag;

            await RunModifyTest(template, action);
        }

        private async Task RunModifyTest(OrderTemplate template, TestPropertyAction action, [CallerMemberName] string testName = "")
        {
            CurrentTestDatails = $"{action} {testName}";

            await RunTest(() => TestModifyOrder(template));
        }
    }
}