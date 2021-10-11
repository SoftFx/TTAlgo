using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal interface ITestGroup
    {
        Task Run(TestParamsSet set);

        event Action<GroupTestResult> TestsFinishedEvent;
    }

    internal abstract class TestGroupBase : ITestGroup
    {
        private const int MaxAttemptsCount = 5;

        private readonly TimeSpan DelayBetweenServerRequests = TimeSpan.FromSeconds(1);

        private readonly Stopwatch _groupWatcher = new Stopwatch();
        private readonly ErrorStorage _errorStorage = new ErrorStorage();

        private int _testsCount;
        private bool _asyncMode;


        protected abstract string GroupName { get; }

        protected abstract string CurrentTestDatails { get; set; }

        internal static CompositeTradeApiTest Bot { get; set; }


        public event Action<GroupTestResult> TestsFinishedEvent;


        public async Task Run(TestParamsSet set)
        {
            try
            {

                _groupWatcher.Start();

                await RunTestGroup(set);

                _asyncMode = true;
                await RunTestGroup(set);

                _groupWatcher.Stop();

                TestsFinishedEvent?.Invoke(
                    new GroupTestResult(_testsCount, _errorStorage.ErrorsCount, _groupWatcher.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                _groupWatcher.Stop();

                Bot.PrintError($"{GroupName} group error: {ex.Message}");
            }
        }

        protected abstract Task RunTestGroup(TestParamsSet set);

        protected async Task RunTest(Func<Task> test)
        {
            try
            {
                Bot.Print($"Test #{++_testsCount}: {CurrentTestDatails} {(_asyncMode ? "Async" : "Sync")}");

                await test();
                return;
            }
            catch (Exception ex)
            {
                _errorStorage.AddError(ex.Message);
            }
        }

        protected async Task RemovePendingOrder(OrderTemplate template)
        {
            if (!template.IsCloseOrder)
                await TestCancelOrder(template);
            else
                await TestCloseOrder(template);
        }

        protected async Task TestOpenOrder(OrderTemplate template, bool activate = true, bool fill = true)
        {
            //PrintLog(template.GetInfo(TestOrderAction.Open));

            var request = template.GetOpenRequest();

            async Task<OrderCmdResult> OpenCommand() =>
                _asyncMode ? await Bot.OpenOrderAsync(request) : Bot.OpenOrder(request);

            var result = await SuccessfulServerRequest(OpenCommand);

            template.UpdateTemplate(result);

            //response.ThrowIfFailed(TestOrderAction.Open);

            //await WaitOpenAndUpdateTemplate(template);

            //if (!activate && !template.IsImmediateFill)
            //    try
            //    {
            //        template.Verification();
            //    }
            //    catch (Exception ex)
            //    {
            //        PrintError(ex.Message);
            //    }

            //if ((activate && fill) || template.IsImmediateFill)
            //    await TryPerformTest(() => TestEventFillOrder(template), 1);
        }

        protected async Task TestModifyOrder(OrderTemplate template)
        {
            var request = template.GetModifyRequest();

            async Task<OrderCmdResult> ModifyCommand() =>
                _asyncMode ? await Bot.ModifyOrderAsync(request) : Bot.ModifyOrder(request);

            await SuccessfulServerRequest(ModifyCommand);
        }

        protected async Task TestCancelOrder(OrderTemplate template)
        {
            //PrintLog(template.GetInfo(TestOrderAction.Cancel));

            async Task<OrderCmdResult> CancelCommand() =>
                _asyncMode ? await Bot.CancelOrderAsync(template.Id) : Bot.CancelOrder(template.Id);

            await SuccessfulServerRequest(CancelCommand);

            //response.ThrowIfFailed(TestOrderAction.Cancel);

            //await WaitAndStoreEvent<OrderCanceledEventArgs>(template, CancelEventTimeout);
        }

        protected async Task TestCloseOrder(OrderTemplate template, double? volume = null)
        {
            //PrintLog(template.GetInfo(TestOrderAction.Close));

            var request = template.GetCloseRequest(volume);

            async Task<OrderCmdResult> CloseCommand() =>
                _asyncMode ? await Bot.CloseOrderAsync(request) : Bot.CloseOrder(request);

            await SuccessfulServerRequest(CloseCommand);

            //response.ThrowIfFailed(TestOrderAction.Close);

            //template.Verification(volume == null);

            //await WaitAndStoreEvent<OrderClosedEventArgs>(template, CloseEventTimeout);
        }

        private async Task<Order> SuccessfulServerRequest(Func<Task<OrderCmdResult>> request)
        {
            int attemptsCount = 0;

            do
            {
                var result = await request();

                if (result.IsCompleted)
                    return result.ResultingOrder;

                if (result.ResultCode.IsServerError())
                    await Task.Delay(DelayBetweenServerRequests);
                else
                    throw new ServerRequestException($"{result.ResultCode}");
            }
            while (++attemptsCount <= MaxAttemptsCount);

            throw ServerRequestException.ServerException;
        }
    }
}