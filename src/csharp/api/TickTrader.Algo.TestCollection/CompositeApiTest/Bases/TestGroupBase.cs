using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class TestGroupBase
    {
        private const int MaxAttemptsCount = 5;

        private readonly TimeSpan DelayBetweenServerRequests = TimeSpan.FromSeconds(5);
        private readonly TimeSpan WaitAllFailedTestEvents = TimeSpan.FromSeconds(5);
        private readonly TimeSpan SingleTestTimeout = TimeSpan.FromSeconds(15);

        private readonly GroupStatManager _statsManager;
        private readonly EventsQueueManager _eventManager;

        private bool _asyncMode;

        internal static CompositeTradeApiTest Bot { get; set; }

        internal static Symbol Symbol => Bot.Symbol;


        protected abstract string GroupName { get; }


        public TestGroupBase()
        {
            _statsManager = Bot.StatManager.GetGroupStatManager();
            _eventManager = new EventsQueueManager(Bot);
        }


        protected abstract Task RunTestGroup(OrderBaseSet set);


        protected async Task RunTest(Func<OrderStateTemplate, Task> test, OrderBaseSet set, OrderStateTemplate template = null,
            string testInfo = null)
        {
            var runTestTask = RunTestInternal(test, set, template, testInfo);
            var firstCompleted = await Task.WhenAny(runTestTask, Bot.Delay(SingleTestTimeout));

            if (firstCompleted != runTestTask)
                _statsManager.TestError("FATAL: RunTest() timeout");
        }

        protected async Task RunTestInternal(Func<OrderStateTemplate, Task> test, OrderBaseSet set, OrderStateTemplate template = null,
            string testInfo = null)
        {
            try
            {
                Bot.PrintDebug("Start test");

                _eventManager.ResetAllQueues();

                if (template != null)
                    _eventManager.RegisterExistingTemplate(template);
                else
                    template = set.BuildOrder();

                _statsManager.StartNewTest(testInfo ?? test.Method.Name, _asyncMode);

                await test(template);
                await _eventManager.WaitAllEvents();

                Bot.PrintDebug("Stop test");
            }
            catch (Exception ex)
            {
                _statsManager.TestError(ex.Message);

                if (ex is EventException)
                    await Bot.Delay(WaitAllFailedTestEvents);
            }
        }

        public async Task Run(OrderBaseSet set)
        {
            try
            {
                _eventManager.SubscribeToOrderEvents();
                _statsManager.StartTestGroupWatch(GroupName, set);

                _asyncMode = false;
                await RunTestGroup(set);

                _eventManager.ResetAllQueues();

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


        private OrderStateTemplate RegisterAdditionalTemplate(OrderStateTemplate template)
        {
            _eventManager.RegistryNewTemplate(template);

            foreach (var linkOrder in template.LinkedOrders)
                _eventManager.RegistryNewTemplate(linkOrder);

            return template;
        }


        protected async Task OpenExecutionOrder(OrderStateTemplate template)
        {
            await OpenAndWaitExecution(template.ForExecuting());
            await RemoveOrder(template);
        }

        protected async Task ExecutionByModifyOrder(OrderStateTemplate template)
        {
            await ModifyAndWaitExecution(template);
            await RemoveOrder(template);
        }

        protected async Task OpenTpSlExecutionOrder(OrderStateTemplate template)
        {
            await OpenAndWaitExecution(template.ForExecuting(), Events.Order.FullActivationPath(template));
            await template.Closed.Task;
        }

        protected async Task OpenAndWaitExecution(OrderStateTemplate template, Type[] events = null)
        {
            await TestOpenOrder(template, events ?? Events.Order.FullExecutionPath(template));
            await template.IsExecuted.Task;
        }

        protected async Task ModifyAndWaitExecution(OrderStateTemplate template)
        {
            await TestModifyOrder(template.ForExecuting(), Events.Order.FullExecutionPath(template));
            await template.IsExecuted.Task;
        }

        protected async Task RemoveOrder(OrderStateTemplate template)
        {
            if (template.IsRemoved)
                return;

            if (template.IsSupportedClose)
            {
                await template.OpenedGrossPosition.Task;

                foreach (var filledPart in template.FilledParts)
                    if (!filledPart.IsRemoved)
                        await TestCloseOrder(filledPart);

                await TestCloseOrder(template);
            }
            else
            {
                await template.Opened.Task;

                if (template.RelatedOcoTemplate?.IsRemoved ?? true)
                    await TestCancelOrder(template);
                else
                {
                    await TestCancelOrder(template, Events.Cancel);
                    await template.RelatedOcoTemplate.Canceled.Task;
                }
            }
        }


        protected async Task TestOpenOrders(params OrderStateTemplate[] orders)
        {
            foreach (var order in orders)
                await TestOpenOrder(order);
        }

        protected async Task TestOpenOrder(OrderStateTemplate template, params Type[] eventsAfterOpen)
        {
            var request = template.GetOpenRequest();

            async Task<OrderCmdResult> OpenCommand() =>
                _asyncMode ? await Bot.OpenOrderAsync(request) : Bot.OpenOrder(request);

            RegisterAdditionalTemplate(template);

            await WaitSuccServerRequest(OpenCommand, Events.Open, eventsAfterOpen);
            await template.Opened.Task;
        }

        protected Task TestOpenReject(OrderStateTemplate template, OrderCmdResultCodes errorCode = OrderCmdResultCodes.DealerReject)
        {
            var request = template.GetOpenRequest();

            async Task<OrderCmdResult> OpenCommand() =>
                _asyncMode ? await Bot.OpenOrderAsync(request) : Bot.OpenOrder(request);

            return WaitRejectSeverRequest(OpenCommand, errorCode).ContinueWith(_ => template.ToRejectOpen());
        }


        protected async Task TestModifyOrder(OrderStateTemplate template, params Type[] eventsAfterModify)
        {
            var request = template.GetModifyRequest();

            async Task<OrderCmdResult> ModifyCommand() =>
                _asyncMode ? await Bot.ModifyOrderAsync(request) : Bot.ModifyOrder(request);

            await WaitSuccServerRequest(ModifyCommand, Events.Modify, eventsAfterModify);
            await template.Modified.Task;
        }

        protected Task TestModifyReject(OrderStateTemplate template, OrderCmdResultCodes errorCode)
        {
            var request = template.GetModifyRequest();

            async Task<OrderCmdResult> ModifyCommand() =>
                _asyncMode ? await Bot.ModifyOrderAsync(request) : Bot.ModifyOrder(request);

            return WaitRejectSeverRequest(ModifyCommand, errorCode);
        }


        //protected async Task TestCancelOrders(params OrderStateTemplate[] orders)
        //{
        //    foreach (var order in orders)
        //        await TestCancelOrder(order);
        //}

        protected async Task TestCancelOrder(OrderStateTemplate template, params Type[] eventsAfterCancel)
        {
            async Task<OrderCmdResult> CancelCommand() =>
                _asyncMode ? await Bot.CancelOrderAsync(template.Id) : Bot.CancelOrder(template.Id);

            await WaitSuccServerRequest(CancelCommand, Events.Cancel, eventsAfterCancel);
            await template.Canceled.Task;
        }

        protected async Task TestCancelReject(OrderStateTemplate template, OrderCmdResultCodes errorCode)
        {
            async Task<OrderCmdResult> CancelCommand() =>
                _asyncMode ? await Bot.CancelOrderAsync(template.Id) : Bot.CancelOrder(template.Id);

            await WaitRejectSeverRequest(CancelCommand, errorCode);
        }


        protected async Task TestCloseOrder(OrderStateTemplate template, double? volume = null)
        {
            var request = template.GetCloseRequest(volume);

            async Task<OrderCmdResult> CloseCommand() =>
                _asyncMode ? await Bot.CloseOrderAsync(request) : Bot.CloseOrder(request);

            await WaitSuccServerRequest(CloseCommand, Events.Close);
            await template.Closed.Task;
        }

        protected async Task<OrderStateTemplate> TestCloseByOrders(OrderStateTemplate first, OrderStateTemplate second)
        {
            async Task<OrderCmdResult> CloseByCommand() =>
                _asyncMode ? await Bot.CloseOrderByAsync(first.Id, second.Id) : Bot.CloseOrderBy(first.Id, second.Id);

            var result = first + second;

            if (!result.IsNull)
            {
                _eventManager.AddExpectedEvent(Events.Open);
                RegisterAdditionalTemplate(result.ToGrossPosition());
            }

            await WaitSuccServerRequest(CloseByCommand, Events.Close, Events.Close);
            await first.Closed.Task;
            await second.Closed.Task;

            return result;
        }


        private async Task<Order> WaitSuccServerRequest(Func<Task<OrderCmdResult>> request, Type initialEvent,
            params Type[] events)
        {
            int attemptsCount = 0;
            OrderCmdResultCodes resultCode;

            _eventManager.AddExpectedEvent(initialEvent);
            _eventManager.AddExpectedEvents(events);

            do
            {
                Bot.PrintDebug($"Start request Async = {_asyncMode}");

                var result = await request();

                if (result.IsCompleted)
                    return result.ResultingOrder;

                resultCode = result.ResultCode;

                if (resultCode.IsServerError())
                    await Bot.Delay(DelayBetweenServerRequests);
                else
                    throw new ServerRequestException(resultCode);
            }
            while (++attemptsCount <= MaxAttemptsCount);

            throw new ServerRequestException(resultCode);
        }

        private async Task WaitRejectSeverRequest(Func<Task<OrderCmdResult>> request, OrderCmdResultCodes expectedError)
        {
            int attemptsCount = 0;

            do
            {
                Bot.PrintDebug($"Start reject request Async = {_asyncMode}");

                var result = await request();
                var resultCode = result.ResultCode;

                if (resultCode == expectedError)
                    return;
                else if (resultCode.IsServerError())
                    await Bot.Delay(DelayBetweenServerRequests);
                else
                    throw new ServerRequestException(resultCode);
            }
            while (++attemptsCount <= MaxAttemptsCount);
        }
    }
}