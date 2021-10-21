using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class TestGroupBase : IDisposable
    {
        private const int MaxAttemptsCount = 5;

        private readonly TimeSpan DelayBetweenServerRequests = TimeSpan.FromSeconds(5);
        private readonly TimeSpan WaitToUpdateGrossPositions = TimeSpan.FromSeconds(1);
        private readonly TimeSpan WaitAllFailedTestEvents = TimeSpan.FromSeconds(5);

        private readonly GroupStatManager _statsManager;
        private readonly EventsQueueManager _eventManager;

        private bool _asyncMode;

        internal static CompositeTradeApiTest Bot { get; set; }


        protected abstract string GroupName { get; }


        public TestGroupBase()
        {
            _statsManager = Bot.StatManager.GetGroupStatManager();
            _eventManager = new EventsQueueManager(Bot);
        }


        public void Dispose()
        {
            _eventManager.UnsubscribeToOrderEvents();
        }


        protected abstract Task RunTestGroup(TestParamsSet set);

        protected async Task RunTest(Func<OrderTemplate, Task> test, TestParamsSet set, OrderTemplate template = null,
            string testInfo = null)
        {
            try
            {
                Bot.PrintDebug("Start test");

                template = template ?? set.BuildOrder();

                _eventManager.SetNewTestTemplate(template);
                _statsManager.StartNewTest(testInfo ?? test.Method.Name, _asyncMode);

                await test(template);
                await _eventManager.WaitAllEvents();

                Bot.PrintDebug("Stop test");
            }
            catch (Exception ex)
            {
                _statsManager.TestError(ex.Message);

                if (ex is EventException)
                    await Task.Delay(WaitAllFailedTestEvents);
            }
        }

        public async Task Run(TestParamsSet set)
        {
            try
            {
                _eventManager.SubscribeToOrderEvents();
                _statsManager.StartTestGroupWatch(GroupName, set);

                _asyncMode = false;
                await RunTestGroup(set);

                _asyncMode = true;
                await RunTestGroup(set);

                _statsManager.StopTestGroupWatch();
            }
            catch (Exception ex)
            {
                _statsManager.FatalGroupError(ex.Message);
            }
            finally
            {
                _eventManager.UnsubscribeToOrderEvents();
            }
        }


        protected async Task OpenExecutionOrder(OrderTemplate template)
        {
            await TestOpenOrder(template.ForExecuting(), GetExecutionEvents(template));
            await ClearTestEnviroment(template);
        }

        protected async Task ModifyForExecutionOrder(OrderTemplate template)
        {
            await TestModifyOrder(template.ForExecuting());

            _eventManager.AddExpectedEvents(GetExecutionEvents(template));

            await ClearTestEnviroment(template);
        }

        private static Type[] GetExecutionEvents(OrderTemplate template)
        {
            var isGross = template.IsGrossAcc;
            var events = isGross ? OrderEvents.FillOnGrossOrderEvents : OrderEvents.FillOrderEvents;

            if (template.IsStopLimit)
                events = isGross ? OrderEvents.FillOnGrossStopLimitEvents : OrderEvents.FillStopLimitEvents;

            return events;
        }

        private async Task ClearTestEnviroment(OrderTemplate template)
        {
            if (template.IsGrossAcc)
            {
                await Task.Delay(WaitToUpdateGrossPositions);
                await RemoveOrder(template);
            }
        }

        protected async Task RemoveOrder(OrderTemplate template)
        {
            if (template.CanCloseOrder)
                await TestCloseOrder(template);
            else
                await TestCancelOrder(template);
        }


        protected async Task TestOpenOrder(OrderTemplate template, params Type[] eventsAfterOpen)
        {
            var request = template.GetOpenRequest();

            async Task<OrderCmdResult> OpenCommand() =>
                _asyncMode ? await Bot.OpenOrderAsync(request) : Bot.OpenOrder(request);

            template.ToOpen(await WaitSuccServerRequest(OpenCommand, OrderEvents.Open, eventsAfterOpen));
        }

        protected async Task TestModifyOrder(OrderTemplate template)
        {
            var request = template.GetModifyRequest();

            async Task<OrderCmdResult> ModifyCommand() =>
                _asyncMode ? await Bot.ModifyOrderAsync(request) : Bot.ModifyOrder(request);

            await WaitSuccServerRequest(ModifyCommand, OrderEvents.Modify);
        }

        protected async Task TestCancelOrder(OrderTemplate template)
        {
            async Task<OrderCmdResult> CancelCommand() =>
                _asyncMode ? await Bot.CancelOrderAsync(template.Id) : Bot.CancelOrder(template.Id);

            await WaitSuccServerRequest(CancelCommand, OrderEvents.Cancel);
        }

        protected async Task TestCloseOrder(OrderTemplate template, double? volume = null)
        {
            var request = template.GetCloseRequest(volume);

            async Task<OrderCmdResult> CloseCommand() =>
                _asyncMode ? await Bot.CloseOrderAsync(request) : Bot.CloseOrder(request);

            await WaitSuccServerRequest(CloseCommand, OrderEvents.Close);
        }

        private async Task<Order> WaitSuccServerRequest(Func<Task<OrderCmdResult>> request, Type initialEvent, params Type[] events)
        {
            int attemptsCount = 0;

            _eventManager.AddExpectedEvent(initialEvent);
            _eventManager.AddExpectedEvents(events);

            do
            {
                Bot.PrintDebug($"Start request Async = {_asyncMode}");

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