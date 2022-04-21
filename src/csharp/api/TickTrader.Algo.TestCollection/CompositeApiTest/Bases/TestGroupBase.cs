using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class TestGroupBase
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


        protected abstract Task RunTestGroup(OrderBaseSet set);


        protected async Task RunTest(Func<OrderStateTemplate, Task> test, OrderBaseSet set, OrderStateTemplate template = null,
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
                    await Task.Delay(WaitAllFailedTestEvents);
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
            foreach (var linkOrder in template.LinkedOrders)
                _eventManager.RegistryNewTemplate(linkOrder);

            _eventManager.RegistryNewTemplate(template);

            return template;
        }


        protected async Task OpenExecutionOrder(OrderStateTemplate template)
        {
            await OpenOrderAndWaitExecution(template.ForExecuting());
            await ClearTestEnviroment(template);
        }

        protected async Task OpenOrderAndWaitExecution(OrderStateTemplate template, Type[] events = null)
        {
            await TestOpenOrder(template, events ?? GetExecutionEvents(template));
            await template.FinalExecution.Task;
        }

        protected async Task ModifyForExecutionOrder(OrderStateTemplate template)
        {
            await TestModifyOrder(template.ForExecuting());

            _eventManager.AddExpectedEvents(GetExecutionEvents(template));

            await ClearTestEnviroment(template);
        }

        protected async Task ClearTestEnviroment(OrderStateTemplate template)
        {
            if (template.IsGrossAcc)
            {
                await Task.Delay(WaitToUpdateGrossPositions);
                await RemoveOrder(template);
            }
        }

        private static Type[] GetExecutionEvents(OrderStateTemplate template)
        {
            var isGross = template.IsGrossAcc;
            var events = isGross ? OrderEvents.FillOnGrossOrderEvents : OrderEvents.FillOrderEvents;

            if (template.IsStopLimit)
                events = isGross ? OrderEvents.FillOnGrossStopLimitEvents : OrderEvents.FillStopLimitEvents;

            return events;
        }

        protected async Task RemoveOrder(OrderStateTemplate template)
        {
            await template.Opened.Task;

            if (template?.RealOrder?.IsNull ?? true)
                return;

            if (template.CanCloseOrder)
            {
                foreach (var filledPart in template.FilledParts)
                    await TestCloseOrder(filledPart);

                await TestCloseOrder(template);
            }
            else
            {
                if (template?.RelatedOcoTemplate?.RealOrder?.IsNull ?? true)
                    await TestCancelOrder(template);
                else
                {
                    await TestCancelOrder(template, OrderEvents.Cancel);
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

            await WaitSuccServerRequest(OpenCommand, OrderEvents.Open, eventsAfterOpen);
            await template.Opened.Task;
        }

        protected async Task TestOpenReject(OrderStateTemplate template, OrderCmdResultCodes errorCode = OrderCmdResultCodes.DealerReject)
        {
            var request = template.GetOpenRequest();

            async Task<OrderCmdResult> OpenCommand() =>
                _asyncMode ? await Bot.OpenOrderAsync(request) : Bot.OpenOrder(request);

            await WaitRejectSeverRequest(OpenCommand, errorCode);
        }


        protected async Task TestModifyOrder(OrderStateTemplate template, params Type[] eventsAfterModify)
        {
            var request = template.GetModifyRequest();

            async Task<OrderCmdResult> ModifyCommand() =>
                _asyncMode ? await Bot.ModifyOrderAsync(request) : Bot.ModifyOrder(request);

            await WaitSuccServerRequest(ModifyCommand, OrderEvents.Modify, eventsAfterModify);
            await template.Modified.Task;
        }

        protected async Task TestModifyReject(OrderStateTemplate template, OrderCmdResultCodes errorCode)
        {
            var request = template.GetModifyRequest();

            async Task<OrderCmdResult> ModifyCommand() =>
                _asyncMode ? await Bot.ModifyOrderAsync(request) : Bot.ModifyOrder(request);

            await WaitRejectSeverRequest(ModifyCommand, errorCode);
        }


        protected async Task TestCancelOrders(params OrderStateTemplate[] orders)
        {
            foreach (var order in orders)
                await TestCancelOrder(order);
        }

        protected async Task TestCancelOrder(OrderStateTemplate template, params Type[] eventsAfterCancel)
        {
            async Task<OrderCmdResult> CancelCommand() =>
                _asyncMode ? await Bot.CancelOrderAsync(template.Id) : Bot.CancelOrder(template.Id);

            await WaitSuccServerRequest(CancelCommand, OrderEvents.Cancel, eventsAfterCancel);
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

            await WaitSuccServerRequest(CloseCommand, OrderEvents.Close);
        }

        protected async Task<OrderStateTemplate> TestCloseByOrders(OrderStateTemplate first, OrderStateTemplate second)
        {
            async Task<OrderCmdResult> CloseByCommand() =>
                _asyncMode ? await Bot.CloseOrderByAsync(first.Id, second.Id) : Bot.CloseOrderBy(first.Id, second.Id);

            var result = first + second;

            if (!result.IsNull)
            {
                _eventManager.AddExpectedEvent(OrderEvents.Open);
                RegisterAdditionalTemplate(result);
            }

            await WaitSuccServerRequest(CloseByCommand, OrderEvents.Close, OrderEvents.Close);

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
                    await Task.Delay(DelayBetweenServerRequests);
                else
                    throw new ServerRequestException(resultCode);
            }
            while (++attemptsCount <= MaxAttemptsCount);

            throw new ServerRequestException(resultCode);
        }

        private async Task WaitRejectSeverRequest(Func<Task<OrderCmdResult>> request, OrderCmdResultCodes expectedError)
        {
            int attemptsCount = 0;
            OrderCmdResultCodes resultCode;

            do
            {
                Bot.PrintDebug($"Start reject request Async = {_asyncMode}");

                var result = await request();

                resultCode = result.ResultCode;

                if (resultCode == expectedError)
                    return;
                else
                if (resultCode.IsServerError())
                    await Task.Delay(DelayBetweenServerRequests);
                else
                    throw new ServerRequestException(resultCode);
            }
            while (++attemptsCount <= MaxAttemptsCount);
        }
    }
}