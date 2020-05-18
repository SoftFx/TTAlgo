using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    [TradeBot(DisplayName = "Composite Trade API Test", Version = "1.4", Category = "Auto Tests", SetupMainSymbol = true)]
    public class CompositeTradeApiTest : TradeBot
    {
        private readonly TimeSpan OpenEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan ActivateEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan FillEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan ModifyEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan TPSLEventTimeout = TimeSpan.FromSeconds(20);
        private readonly TimeSpan CancelEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan CloseEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan ExpirationEventTimeout = TimeSpan.FromSeconds(25);
        private readonly TimeSpan PauseBetweenOrders = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan PauseBeforeAndAfterTests = TimeSpan.FromSeconds(2);
        private readonly TimeSpan TimeToExpire = TimeSpan.FromSeconds(3);

        private readonly List<HistoryOrderTemplate> _historyStorage = new List<HistoryOrderTemplate>();
        private TaskCompletionSource<object> _eventWaiter;

        private int _testCount = 0;
        private int _errorCount = 0;

        private Stopwatch _st = new Stopwatch(); // for test


        [Parameter]
        public bool CleanUpOrdersAfterTests { get; set; }

        [Parameter(DefaultValue = false)]
        public bool UseDebug { get; set; }

        [Parameter]
        public bool IncludeADCases { get; set; }

        [Parameter(DefaultValue = 3)]
        public int TestCaseAttempts { get; set; }

        [Parameter(DefaultValue = 0.1, DisplayName = "Volume")]
        public double DefaultOrderVolume { get; set; }

        [Parameter(DefaultValue = 100)]
        public int PriceDelta { get; set; }

        protected override void Init()
        {
            TestParamsSet.AccountType = Account.Type;
            TestParamsSet.Orders = Account.Orders;
        }

        protected async override void OnStart()
        {
            CleanUp();

            await Delay(PauseBeforeAndAfterTests);

            _st.Start(); // test

            SubscribeEventListening(); //Should be location after CleanUp (Reject CancelEvents)

            foreach (OrderSide orderSide in Enum.GetValues(typeof(OrderSide)))
                foreach (OrderType orderType in Enum.GetValues(typeof(OrderType)))
                {
                    if (orderType == OrderType.Position)
                        continue;

                    var testSet = new TestParamsSet(orderType, orderSide);

                    do
                    {
                        await FullTestRun(testSet, OrderExecOptions.None);

                        if (orderType == OrderType.StopLimit || orderType == OrderType.Limit)
                            await FullTestRun(testSet, OrderExecOptions.ImmediateOrCancel);

                    }
                    while (testSet.SwitchAsyncMode());
                }

            Print("Waiting for trade reports to load...");
            await Delay(PauseBeforeAndAfterTests);

            //History test
            await TryPerformTest(() => FullHistoryTestRun(), 1);

            UnsubscribeEventListening();

            if (CleanUpOrdersAfterTests)
                CleanUp();

            _st.Stop();
            //Status.Write($"Total time: {_st.ElapsedMilliseconds}");
            PrintStatus();
            Exit();
        }

        private async Task FullTestRun(TestParamsSet test, OrderExecOptions options)
        {
            PrintStatus();

            test.Options = options;

            await PrepareAndRun(TestAcion.FullModify, PerfomOpenModifyTests, test);
            await PerformExecutionTests(test);
            await PerformCloseByTests(test);

            if (IncludeADCases)
                await PerformADCommentsTest(test);
        }

        private async Task PerfomOpenModifyTests(OrderTemplate template)
        {
            await TryPerformTest(() => TestOpenOrder(template, false));

            if (!template.IsImmediateFill)
            {
                await PerformVolumeModifyTests(template, DefaultOrderVolume * 2);
                await PerformCommentModifyTests(template);
                await PerformExpirationModifyTests(template);

                if (Account.Type == AccountTypes.Gross)
                {
                    await PerformTakeProfitModifyTests(template);
                    await PerformStopLossModifyTests(template);
                }

                if (template.Type != OrderType.Stop)
                {
                    await PerformMaxVisibleVolumeModifyTests(template);
                    await PerformPriceModifyTests(template, CalculatePrice(template));
                }

                if (template.Type != OrderType.Limit)
                    await PerformStopPriceModifyTests(template, CalculatePrice(template, 4));

                if (template.Type == OrderType.StopLimit)
                    await PerformOptionsModifyTests(template);

                if (!template.IsCloseOrder)
                    await TryPerformTest(() => TestCancelOrder(template));
                else
                    await TryPerformTest(() => TestCloseOrder<OrderCanceledEventArgs>(template));
            }
            else
            if (Account.Type == AccountTypes.Gross)
                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template));
        }

        private async Task PerformExecutionTests(TestParamsSet test)
        {
            await PrepareAndRun(TestAcion.Fill, PerformOrderFillExecutionTest, test, OrderExecutionMode.Execution);

            if (test.Type == OrderType.Limit && test.Options == OrderExecOptions.ImmediateOrCancel) //remove type == Limit
                await PrepareAndRun(TestAcion.RejectIoC, PerformRejectIocExecutionTest, test, OrderExecutionMode.Execution);

            if (Account.Type == AccountTypes.Gross)
            {
                await PrepareAndRun(TestAcion.TP, PerformTakeProfitExecutionTest, test, OrderExecutionMode.Execution);
                await PrepareAndRun(TestAcion.SL, PerformStopLossExecutionTest, test, OrderExecutionMode.Execution);
            }

            if (!test.IsInstantOrder)
            {
                if (!test.Options.HasFlag(OrderExecOptions.ImmediateOrCancel))
                    await PrepareAndRun(TestAcion.FillByModify, PerformFillByModifyExecutionTest, test);

                await PrepareAndRun(TestAcion.Cancel, PerformCancelExecutionTest, test);
                await PrepareAndRun(TestAcion.Expiration, PerformExpirationExecutionTest, test);
            }
        }

        private async Task PerformCloseByTests(TestParamsSet test)
        {
            if (Account.Type != AccountTypes.Gross || test.Type != OrderType.Market)
                return;

            await PrepareCloseByTest(TestAcion.CloseBySmallBig, DefaultOrderVolume * 2, test);
            await PrepareCloseByTest(TestAcion.CloseByBigSmall, DefaultOrderVolume / 2, test);
            await PrepareCloseByTest(TestAcion.CloseByEven, null, test);
        }

        private async Task PerformADCommentsTest(TestParamsSet test)
        {
            await PrepareAndRun(TestAcion.ADReject, TestCommentReject, test, OrderExecutionMode.Execution);

            if (test.Type != OrderType.StopLimit && test.Options != OrderExecOptions.ImmediateOrCancel)
                await PrepareAndRun(TestAcion.ADPartialActivate, TestCommentPartialActivate, test);
        }

        #region Tests

        private async Task PerformOrderFillExecutionTest(OrderTemplate template)
        {
            template.Volume = 4 * DefaultOrderVolume;

            await TryPerformTest(() => TestOpenOrder(template));

            if (Account.Type == AccountTypes.Gross)
            {
                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template, template.Volume / 4));
                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template));
            }
        }

        private async Task PerformRejectIocExecutionTest(OrderTemplate template)
        {
            template.Price = CalculatePrice(template.Side, -5);

            await TryCatchOrderReject(template);
        }

        private async Task PerformFillByModifyExecutionTest(OrderTemplate template)
        {
            template.Volume = 4 * DefaultOrderVolume;
            template.Price = CalculatePrice(template.Side, -5);

            if (template.Type == OrderType.StopLimit)
                template.StopPrice = CalculatePrice(template.Side, -1);

            await TryPerformTest(() => TestOpenOrder(template, fill: false));

            template.Price = CalculatePrice(template.Side, 2);
            template.StopPrice = CalculatePrice(template.Side, -1);

            await TryPerformTest(() => TestModifyOrder(template, true));

            await TryPerformTest(() => TestEventFillOrder(template), 1);

            if (Account.Type == AccountTypes.Gross)
            {
                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template, template.Volume / 4));
                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template));
            }
        }

        private async Task PerformCancelExecutionTest(OrderTemplate template)
        {
            await TryPerformTest(() => TestOpenOrder(template, false));
            await TryPerformTest(() => TestCancelOrder(template));
        }

        private async Task PerformExpirationExecutionTest(OrderTemplate template)
        {
            template.Expiration = DateTime.Now + TimeToExpire;

            await TryPerformTest(() => TestOpenOrder(template, false));

            await WaitAndStoreEvent<OrderExpiredEventArgs>(template, ExpirationEventTimeout + TimeToExpire);
        }

        private async Task PerformTakeProfitExecutionTest(OrderTemplate template)
        {
            template.TP = CalculatePrice(template, -2);

            await RunOpenWithModifyCloseEvent(template);
        }

        private async Task PerformStopLossExecutionTest(OrderTemplate template)
        {
            template.SL = CalculatePrice(template, 2);

            await RunOpenWithModifyCloseEvent(template);
        }

        private async Task RunOpenWithModifyCloseEvent(OrderTemplate template)
        {
            await TryPerformTest(() => TestOpenOrder(template));

            await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
            await WaitAndStoreEvent<OrderClosedEventArgs>(template, TPSLEventTimeout);
        }

        private async Task PrepareCloseByTest(TestAcion action, double? closeVolume, TestParamsSet test)
        {
            async Task func(OrderTemplate template)
            {
                var inversed = template.InversedCopy(closeVolume);

                template.TP = CalculatePrice(template, 4);
                template.SL = CalculatePrice(template, -4);
                template.Comment = "First";

                inversed.TP = CalculatePrice(inversed, 3);
                inversed.SL = CalculatePrice(inversed, -3);
                inversed.Comment = "Second";

                await TryPerformTest(() => TestOpenOrder(template));
                await TryPerformTest(() => TestOpenOrder(inversed));

                await TryPerformTest(() => TestCloseBy(template, inversed));
            }

            await PrepareAndRun(action, func, test, OrderExecutionMode.Execution);
        }

        private async Task TryCatchOrderReject(OrderTemplate template)
        {
            try
            {
                await TestOpenOrder(template, reopen: false);

                //if (template.Type == OrderType.StopLimit)
                //    return;
            }
            catch
            {
                return;
            }

            throw new Exception("Order not rejected!");
        }

        private async Task PrepareAndRun(TestAcion action, Func<OrderTemplate, Task> func, TestParamsSet test, OrderExecutionMode mode = OrderExecutionMode.Waiting)
        {
            await Delay(PauseBetweenOrders);

            PrintTest(test.Info(action));

            try
            {
                var template = new OrderTemplate(test, mode)
                {
                    Volume = DefaultOrderVolume,
                };

                if (mode == OrderExecutionMode.Execution)
                {
                    template.Price = CalculatePrice(template, 2);
                    template.StopPrice = CalculatePrice(template, -1);
                }
                else
                {
                    template.Price = CalculatePrice(template, 3);
                    template.StopPrice = CalculatePrice(template, 3);
                }

                await func(template);
            }
            catch (Exception ex)
            {
                ++_errorCount;
                PrintError(ex.Message);
            }
        }

        private double? CalculatePrice(OrderTemplate template, int coef = 1)
        {
            if (template.Mode == OrderExecutionMode.Waiting)
            {
                if (template.IsInstantOrder)
                    return CalculatePrice(template.Side, coef > 0 ? 1 : -1);

                if (template.Type == OrderType.Limit)
                    return CalculatePrice(template.Side, -coef);

                return CalculatePrice(template.Side, coef);
            }

            return CalculatePrice(template.Side, coef);
        }

        private double? CalculatePrice(OrderSide side, int coef)
        {
            var delta = coef * PriceDelta * Symbol.Point * Math.Max(1, 10 - Symbol.Digits);

            return side == OrderSide.Buy ? Symbol.Ask.Round(Symbol.Digits) + delta : Symbol.Bid.Round(Symbol.Digits) - delta;
        }

        #endregion

        #region Test Order Actions

        private async Task TestOpenOrder(OrderTemplate template, bool activate = true, bool fill = true, bool reopen = true)
        {
            PrintLog(template.GetInfo(TestOrderAction.Open));

            var response = template.Async ? await OpenOrderAsync(Symbol.Name, template.Type, template.Side, template.Volume.Value, template.MaxVisibleVolume, template.Price, template.StopPrice, template.SL, template.TP, template.Comment, template.Options, TestParamsSet.Tag, template.Expiration)
                                          : OpenOrder(Symbol.Name, template.Type, template.Side, template.Volume.Value, template.MaxVisibleVolume, template.Price, template.StopPrice, template.SL, template.TP, template.Comment, template.Options, TestParamsSet.Tag, template.Expiration);

            response.ThrowIfFailed(TestOrderAction.Open);

            await WaitOpenAndUpdateTemplate(template);

            if (!activate && !template.IsImmediateFill)
                template.Verification();

            if (activate && template.Type == OrderType.StopLimit)
                await TryPerformTest(() => TestEventActivateOrder(template, reopen), 1);

            if ((activate && fill && reopen) || template.IsImmediateFill)
                await TryPerformTest(() => TestEventFillOrder(template), 1);
        }

        private async Task TestCancelOrder(OrderTemplate template)
        {
            PrintLog(template.GetInfo(TestOrderAction.Cancel));

            var response = template.Async ? await CancelOrderAsync(template.Id) : CancelOrder(template.Id);

            response.ThrowIfFailed(TestOrderAction.Cancel);

            template.Verification(true);

            await WaitAndStoreEvent<OrderCanceledEventArgs>(template, CancelEventTimeout);
        }

        private async Task TestCloseOrder<T>(OrderTemplate template, double? volume = null) where T : class
        {
            PrintLog(template.GetInfo(TestOrderAction.Close));

            var response = template.Async ? await CloseOrderAsync(template.Id, volume) : CloseOrder(template.Id, volume);

            response.ThrowIfFailed(TestOrderAction.Close);

            template.Verification(volume == null);

            await WaitAndStoreEvent<T>(template, CloseEventTimeout);
        }

        private async Task TestCloseBy(OrderTemplate template, OrderTemplate inversed)
        {
            PrintLog(template.GetInfo(TestOrderAction.CloseBy));

            var response = template.Async ? await CloseOrderByAsync(template.Id, inversed.Id) : CloseOrderBy(template.Id, inversed.Id);

            response.ThrowIfFailed(TestOrderAction.CloseBy);

            var resultCopy = template.Volume < inversed.Volume ? (OrderTemplate)inversed.Clone() : (OrderTemplate)template.Clone();

            if (template.Volume != inversed.Volume)
                await WaitOpenAndUpdateTemplate(resultCopy);

            await WaitAndStoreEvent<OrderClosedEventArgs>(template.Volume < inversed.Volume ? inversed : template, CloseEventTimeout);
            await WaitAndStoreEvent<OrderClosedEventArgs>(template.Volume < inversed.Volume ? template : inversed, CloseEventTimeout);

            if (template.Volume != inversed.Volume)
                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(resultCopy));
        }

        private async Task TestModifyOrder(OrderTemplate template, bool filled = false)
        {
            PrintLog(template.GetInfo(TestOrderAction.Modify));

            var response = template.Async ? await ModifyOrderAsync(template.Id, template.Price, template.StopPrice, template.MaxVisibleVolume, template.SL, template.TP, template.Comment, template.Expiration, template.Volume, template.Options)
                                          : ModifyOrder(template.Id, template.Price, template.StopPrice, template.MaxVisibleVolume, template.SL, template.TP, template.Comment, template.Expiration, template.Volume, template.Options);

            response.ThrowIfFailed(TestOrderAction.Modify);

            await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);

            if (!filled)
                template.Verification();
        }

        #endregion

        #region History test

        private async Task FullHistoryTestRun()
        {
            ReportsIteratorTest();
            await DoQueryTests(false);
            await DoQueryTests(true);
        }

        private async Task DoQueryTests(bool async)
        {
            var from = _historyStorage.First().TradeReportTimestamp;
            var to = _historyStorage.Last().TradeReportTimestamp;

            await QuerySegmentTest(from, to, async, ThQueryOptions.None);
            await QuerySegmentTest(from, to, async, ThQueryOptions.Backwards);
            await QuerySegmentTest(from, to, async, ThQueryOptions.SkipCanceled);
            await QuerySegmentTest(from, to, async, ThQueryOptions.SkipCanceled | ThQueryOptions.Backwards);
        }

        private void ReportsIteratorTest()
        {
            Print("Query trade history: simple iterator");

            var expected = Enumerable.Reverse(_historyStorage).ToList();
            var actual = Account.TradeHistory.Take(expected.Count).ToList();

            CheckReports(expected, actual);
        }

        private async Task QuerySegmentTest(DateTime from, DateTime to, bool async, ThQueryOptions options)
        {
            PrintLog($"Query trade history: async={async} options={options}");

            try
            {
                var reversed = options.HasFlag(ThQueryOptions.Backwards);
                var noCancels = options.HasFlag(ThQueryOptions.SkipCanceled);

                var expected = TakeVerifiers(from, to, reversed, noCancels);
                var actual = await QuerySegmentToList(from, to, async, options);

                CheckReports(expected, actual);
            }
            catch (Exception ex)
            {
                PrintError($"Test failed: {ex.Message}");
            }
        }

        private bool ShouldSkip(bool skipCancels, HistoryOrderTemplate v)
        {
            return skipCancels && (v.TradeReportAction == TradeExecActions.OrderCanceled
                || v.TradeReportAction == TradeExecActions.OrderExpired
                || v.TradeReportAction == TradeExecActions.OrderActivated);
        }

        private List<HistoryOrderTemplate> TakeVerifiers(DateTime from, DateTime to, bool reversed, bool noCancels)
        {
            var result = new List<HistoryOrderTemplate>();

            foreach (var v in _historyStorage)
            {
                if (ShouldSkip(noCancels, v))
                    continue;

                if (v.TradeReportTimestamp >= from && v.TradeReportTimestamp <= to)
                    result.Add(v);
            }

            if (reversed)
                result.Reverse();

            return result;
        }

        private void CheckReports(List<HistoryOrderTemplate> verifiers, List<TradeReport> reports)
        {
            if (reports.Count != verifiers.Count)
                throw new Exception($"Report count does not match expected number! {reports.Count} vs {verifiers.Count}");

            var ide = verifiers.Select(u => u.Id).ToList();
            var idr = reports.Select(u => u.OrderId).ToList();

            for (int i = 0; i < reports.Count; i++)
                verifiers[i].VerifyTradeReport(reports[i]);
        }

        private async Task<List<TradeReport>> QuerySegmentToList(DateTime from, DateTime to, bool async, ThQueryOptions options)
        {
            from = from.Floor(TimeSpan.FromSeconds(1));
            to = to.Ceil(TimeSpan.FromSeconds(1));

            if (async)
            {
                var result = new List<TradeReport>();

                using (var e = Account.TradeHistory.GetRangeAsync(from, to, options))
                {
                    while (await e.Next())
                        result.Add(e.Current);
                }

                return result;
            }
            else
                return Account.TradeHistory.GetRange(from, to, options).ToList();
        }

        #endregion

        #region Event Verification

        private async Task TestEventFillOrder(OrderTemplate template)
        {
            await WaitAndStoreEvent<OrderFilledEventArgs>(template, FillEventTimeout, Account.Type != AccountTypes.Gross);

            if (Account.Type == AccountTypes.Gross)
                await WaitOpenAndUpdateTemplate(template, true);
        }

        private async Task TestEventActivateOrder(OrderTemplate template, bool open = true)
        {
            await WaitAndStoreEvent<OrderActivatedEventArgs>(template, ActivateEventTimeout);

            if (open)
                await WaitOpenAndUpdateTemplate(template, true);
        }

        private async Task WaitOpenAndUpdateTemplate(OrderTemplate template, bool activate = false)
        {
            var args = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

            template.UpdateTemplate(args.Order, activate);
        }

        private async Task WaitAndStoreEvent<T>(OrderTemplate template, TimeSpan delay, bool store = true)
        {
            var args = await WaitEvent<T>(delay);

            if (store)
                _historyStorage.Add(HistoryOrderTemplate.Create(template, args));
        }

        private async Task<TArgs> WaitEvent<TArgs>(TimeSpan waitTimeout)
        {
            _eventWaiter = new TaskCompletionSource<object>();

            var eventTask = _eventWaiter.Task;

            var delayTask = Delay(waitTimeout);
            var completedFirst = await Task.WhenAny(delayTask, eventTask);

            if (completedFirst == delayTask)
                throw new Exception($"Timeout reached while wating for event {typeof(TArgs).Name}");

            var argsObj = await eventTask;

            if (argsObj is TArgs)
            {
                PrintLog($"Event received: {argsObj.GetType().Name}");
                return (TArgs)argsObj;
            }
            throw new Exception($"Unexpected event: Received {argsObj.GetType().Name} while expecting {typeof(TArgs).Name}");
        }

        private void OnEventFired<TArgs>(TArgs args)
        {
            if (_eventWaiter != null)
            {
                var waiterCopy = _eventWaiter;
                _eventWaiter = null;
                waiterCopy.SetResult(args); // note: function may start wating for new event inside SetResult(), so _eventWaiter = null should be before SetResult()
            }
            else
                throw new Exception($"Unexpected event: {args.GetType().Name}");
        }

        #endregion

        #region Misc methods

        private void CleanUp()
        {
            Print("Cleaning up...");

            foreach (var order in Account.Orders.ToList())
            {
                if (order.Type == OrderType.Position)
                    CloseOrder(order.Id);
                else
                    CancelOrder(order.Id);
            }
        }

        private void PrintStatus()
        {
            Status.WriteLine($"Tests: {_testCount}, Errors: {_errorCount}");
            Status.WriteLine($"See logs for more details");
            Status.Flush();
        }

        private void PrintLog(string message)
        {
            if (UseDebug)
                Print(message);
        }

        private void PrintTest(string message)
        {
            Print($"Test №{++_testCount} {message}");
        }

        private void SubscribeEventListening()
        {
            Account.Orders.Opened += OnEventFired;
            Account.Orders.Filled += OnEventFired;
            Account.Orders.Closed += OnEventFired;
            Account.Orders.Expired += OnEventFired;
            Account.Orders.Canceled += OnEventFired;
            Account.Orders.Activated += OnEventFired;
            Account.Orders.Modified += OnEventFired;
        }

        private void UnsubscribeEventListening()
        {
            Account.Orders.Opened -= OnEventFired;
            Account.Orders.Filled -= OnEventFired;
            Account.Orders.Closed -= OnEventFired;
            Account.Orders.Expired -= OnEventFired;
            Account.Orders.Canceled -= OnEventFired;
            Account.Orders.Activated -= OnEventFired;
            Account.Orders.Modified -= OnEventFired;
        }

        private async Task TryPerformTest(Func<Task> func, int? count = null)
        {
            int attemptsFailed = 0;
            int attemptsBorder = count ?? TestCaseAttempts;

            while (attemptsFailed < attemptsBorder)
            {
                try
                {
                    await func();
                    return;
                }
                catch (VerificationException ex)
                {
                    ++_errorCount;
                    PrintError(ex.Message);
                }
                catch (Exception ex)
                {
                    if (++attemptsFailed < attemptsBorder)
                        Print("Attempt failed, retrying.");
                    else
                    {
                        ++_errorCount;
                        PrintError(ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Add/Modify test

        private async Task PerformCommentModifyTests(OrderTemplate test)
        {
            await RunCommentTest(test, TestPropertyAction.Add, "New_comment");
            await RunCommentTest(test, TestPropertyAction.Modify, "Replace_Comment");
            await RunCommentTest(test, TestPropertyAction.Delete, string.Empty);
        }

        private async Task PerformTakeProfitModifyTests(OrderTemplate template)
        {
            await RunTakeProfitTest(template, TestPropertyAction.Add, CalculatePrice(template, 4));
            await RunTakeProfitTest(template, TestPropertyAction.Modify, CalculatePrice(template, 5));
            await RunTakeProfitTest(template, TestPropertyAction.Delete, 0);
        }

        private async Task PerformStopLossModifyTests(OrderTemplate template)
        {
            await RunStopLossTest(template, TestPropertyAction.Add, CalculatePrice(template, -4));
            await RunStopLossTest(template, TestPropertyAction.Modify, CalculatePrice(template, -5));
            await RunStopLossTest(template, TestPropertyAction.Delete, 0);
        }

        private async Task PerformExpirationModifyTests(OrderTemplate template)
        {
            await RunExpirationTest(template, TestPropertyAction.Add, DateTime.Now.AddYears(1));
            await RunExpirationTest(template, TestPropertyAction.Modify, DateTime.Now.AddYears(2));
            await RunExpirationTest(template, TestPropertyAction.Delete, DateTime.MinValue);
        }

        private async Task PerformMaxVisibleVolumeModifyTests(OrderTemplate template)
        {
            await RunMaxVisibleVolumeTest(template, TestPropertyAction.Add, DefaultOrderVolume);
            await RunMaxVisibleVolumeTest(template, TestPropertyAction.Modify, Symbol.MinTradeVolume);
            await RunMaxVisibleVolumeTest(template, TestPropertyAction.Delete, -1);
        }

        private async Task PerformOptionsModifyTests(OrderTemplate template)
        {
            await RunOptionsTest(template, TestPropertyAction.Add, OrderExecOptions.ImmediateOrCancel);
            await RunOptionsTest(template, TestPropertyAction.Delete, OrderExecOptions.None);
        }

        private async Task PerformVolumeModifyTests(OrderTemplate template, double value)
        {
            template.Volume = value;

            await RunModifyTest(template, TestPropertyAction.Modify, nameof(template.Volume));
        }

        private async Task PerformPriceModifyTests(OrderTemplate template, double? value)
        {
            template.Price = value;

            await RunModifyTest(template, TestPropertyAction.Modify, nameof(template.Price));
        }

        private async Task PerformStopPriceModifyTests(OrderTemplate template, double? value)
        {
            template.StopPrice = value;

            await RunModifyTest(template, TestPropertyAction.Modify, nameof(template.StopPrice));
        }

        private async Task RunCommentTest(OrderTemplate template, TestPropertyAction action, string comment)
        {
            template.Comment = comment;

            await RunModifyTest(template, action, nameof(template.Comment));
        }

        private async Task RunTakeProfitTest(OrderTemplate template, TestPropertyAction action, double? value)
        {
            template.TP = value;

            await RunModifyTest(template, action, nameof(template.TP));
        }

        private async Task RunStopLossTest(OrderTemplate template, TestPropertyAction action, double? value)
        {
            template.SL = value;

            await RunModifyTest(template, action, nameof(template.SL));
        }

        private async Task RunExpirationTest(OrderTemplate template, TestPropertyAction action, DateTime? value)
        {
            template.Expiration = value;

            await RunModifyTest(template, action, nameof(template.Expiration));
        }

        private async Task RunMaxVisibleVolumeTest(OrderTemplate template, TestPropertyAction action, double value)
        {
            template.MaxVisibleVolume = value;

            await RunModifyTest(template, action, nameof(template.MaxVisibleVolume));
        }

        private async Task RunOptionsTest(OrderTemplate template, TestPropertyAction action, OrderExecOptions value)
        {
            template.Options = value;

            await RunModifyTest(template, action, nameof(template.Options));
        }

        private async Task RunModifyTest(OrderTemplate template, TestPropertyAction action, string property)
        {
            PrintTest(template.GetAction(action, property));

            await TryPerformTest(() => TestModifyOrder(template));
        }
        #endregion

        #region AD Comments test

        private async Task TestCommentReject(OrderTemplate template)
        {
            var commentModel = new CommentModelManager
            {
                new CommentActionModel(ADCases.Reject)
            };

            template.Comment = commentModel.GetComment();

            await TryCatchOrderReject(template);
        }

        private async Task TestCommentPartialActivate(OrderTemplate template)
        {
            template.Price = CalculatePrice(template.Side, -5);

            if (template.Type == OrderType.StopLimit)
                template.StopPrice = CalculatePrice(template.Side, -1);

            var customVolume = 0.2 * DefaultOrderVolume * Symbol.ContractSize;

            var commentModel = new CommentModelManager
            {
                new OrderCommentActionModel(template.IsInstantOrder ? ADCases.Confirm : ADCases.Activate, customVolume)
            };

            if (!template.IsImmediateFill)
                commentModel.Add(new OrderCommentActionModel(ADCases.Activate, null));

            template.Comment = commentModel.GetComment();

            await TryPerformTest(() => TestOpenOrder(template));

            if (template.Type == OrderType.Limit && template.Options == OrderExecOptions.ImmediateOrCancel)
                await WaitEvent<OrderCanceledEventArgs>(CancelEventTimeout);

            if (Account.Type != AccountTypes.Gross && !template.IsImmediateFill)
                await TryPerformTest(() => TestEventFillOrder(template), 1);

            if (Account.Type == AccountTypes.Gross)
            {
                template.Id = template.RelatedId;

                await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template));

                if (!template.IsImmediateFill)
                {
                    template.Id = template.RelatedId; ;
                    await TryPerformTest(() => TestCloseOrder<OrderClosedEventArgs>(template));
                }
            }
        }
        #endregion
    }
}
