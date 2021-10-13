using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class TestGroupBase
    {
        private const int MaxAttemptsCount = 5;

        private readonly TimeSpan DelayBetweenServerRequests = TimeSpan.FromSeconds(1);

        private readonly GroupStatManager _statsManager;
        private readonly EventManager _eventManager;

        //private int _testsCount;
        private bool _asyncMode;


        protected abstract string GroupName { get; }

        protected abstract string CurrentTestDatails { get; set; }

        internal static CompositeTradeApiTest Bot { get; set; }


        public event Action<GroupTestReport> TestsFinishedEvent;


        public TestGroupBase()
        {
            _statsManager = StatManagerFactory.GetGroupStatManager();
            _eventManager = new EventManager(Bot);
        }


        protected abstract Task RunTestGroup(TestParamsSet set);

        protected async Task RunTest(Func<Task> test)
        {
            try
            {
                _eventManager.ClearEventsQueue();
                _statsManager.StartNewTest(CurrentTestDatails, _asyncMode);

                await test();
                await _eventManager.WaitAllEvents();

                return;
            }
            catch (Exception ex)
            {
                _statsManager.TestError(ex.Message);
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
                _eventManager.UnsubscribeToOrderEvents();
            }
            catch (Exception ex)
            {
                _statsManager.FatalGroupError(ex.Message);
            }
        }

        protected async Task RemovePendingOrder(OrderTemplate template)
        {
            if (template.CanCloseOrder)
                await TestCloseOrder(template);
            else
                await TestCancelOrder(template);
        }

        protected async Task TestOpenOrder(OrderTemplate template, bool activate = true, bool fill = true)
        {
            //PrintLog(template.GetInfo(TestOrderAction.Open));

            var request = template.GetOpenRequest();

            async Task<OrderCmdResult> OpenCommand() =>
                _asyncMode ? await Bot.OpenOrderAsync(request) : Bot.OpenOrder(request);

            //_eventManager.AddEvent<OrderOpenedEventArgs>();

            var result = await WaitSuccessfulServerRequest<OrderOpenedEventArgs>(OpenCommand);

            if (template.Mode == Behavior.Execution)
                _eventManager.AddEvent<OrderFilledEventArgs>();
            else
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

            //_eventManager.AddEvent<OrderModifiedEventArgs>();

            await WaitSuccessfulServerRequest<OrderModifiedEventArgs>(ModifyCommand);
        }

        protected async Task TestCancelOrder(OrderTemplate template)
        {
            //PrintLog(template.GetInfo(TestOrderAction.Cancel));

            async Task<OrderCmdResult> CancelCommand() =>
                _asyncMode ? await Bot.CancelOrderAsync(template.Id) : Bot.CancelOrder(template.Id);

            await WaitSuccessfulServerRequest<OrderCanceledEventArgs>(CancelCommand);

            //response.ThrowIfFailed(TestOrderAction.Cancel);

            //await WaitAndStoreEvent<OrderCanceledEventArgs>(template, CancelEventTimeout);
        }

        protected async Task TestCloseOrder(OrderTemplate template, double? volume = null)
        {
            //PrintLog(template.GetInfo(TestOrderAction.Close));

            var request = template.GetCloseRequest(volume);

            async Task<OrderCmdResult> CloseCommand() =>
                _asyncMode ? await Bot.CloseOrderAsync(request) : Bot.CloseOrder(request);

            await WaitSuccessfulServerRequest<OrderClosedEventArgs>(CloseCommand);

            //response.ThrowIfFailed(TestOrderAction.Close);

            //template.Verification(volume == null);

            //await WaitAndStoreEvent<OrderClosedEventArgs>(template, CloseEventTimeout);
        }

        private async Task<Order> WaitSuccessfulServerRequest<TEvent>(Func<Task<OrderCmdResult>> request)
        {
            int attemptsCount = 0;

            _eventManager.AddEvent<TEvent>();

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