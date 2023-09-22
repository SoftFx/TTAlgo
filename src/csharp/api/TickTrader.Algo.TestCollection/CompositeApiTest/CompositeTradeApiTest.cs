using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    [TradeBot(DisplayName = "Composite Trade API Test", Version = "2.1", Category = "Auto Tests", SetupMainSymbol = true)]
    public class CompositeTradeApiTest : TradeBot
    {
        private readonly List<HistoryOrderTemplate> _historyStorage = new List<HistoryOrderTemplate>();
        private List<(Predicate<OrderBaseSet> Condition, TestGroupBase Tests)> _testGroups;

        [Parameter(DefaultValue = false)]
        public bool UseDebug { get; set; }

        [Parameter(DefaultValue = 0.1, DisplayName = "BaseVolume")]
        public double DefaultOrderVolume { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int PriceDelta { get; set; }

        //[Parameter]
        //public bool LoadAllHistory { get; set; }


        [Parameter(DefaultValue = true)]
        public bool UseModificationTests { get; set; }

        [Parameter(DefaultValue = true)]
        public bool UseExecutionTests { get; set; }

        [Parameter(DefaultValue = true)]
        public bool UseSlippageTests { get; set; }

        [Parameter(DefaultValue = true)]
        public bool UseOCOTests { get; set; }

        [Parameter(DefaultValue = true)]
        public bool UseOTOTests { get; set; }


        [Parameter]
        public bool UseCloseByTests { get; set; }

        //[Parameter]
        public bool UseADCases { get; set; }


        internal StatManagerFactory StatManager { get; private set; }


        protected override void Init()
        {
            OrderBaseSet.Bot = this;
            TestGroupBase.Bot = this;
            StatManagerFactory.Bot = this;

            Events.Init(Account.Type);
        }

        protected async override void OnStart()
        {
            await PrepareWorkspace();

            await RunAllTestGroups();

            //Print("Waiting for trade reports to load...");
            //await Delay(PauseBeforeAndAfterTests);

            ////History test
            //await TryPerformTest(() => FullHistoryTestRun(), 1);

            OnStop();
        }

        protected override void OnStop()
        {
            GroupTestReport.ResetStaticFields();

            Print($"Finish");

            Exit();
        }

        private async Task PrepareWorkspace()
        {
            CleanUpWorkspace();

            StatManager = new StatManagerFactory();

            _testGroups = new List<(Predicate<OrderBaseSet>, TestGroupBase)>
            {
                (s => UseModificationTests && !s.IsInstantOrder, new ModificationTests()),
                (_ => UseExecutionTests, new ExecutionTests()),
                (s => UseSlippageTests && s.IsSupportedSlippage, new SlippageTests()),
                (s => UseCloseByTests && s.IsGrossAcc, new CloseByTests()),
                (s => UseOCOTests && s.IsSupportedOCO, new OCOTests(UseADCases)),
                (s => UseOTOTests && s.IsSupportedOTO, new OTOTests(UseOCOTests, UseADCases)),
                (_ => UseADCases, new ADTests()),
            };

            await Delay(2000); // wait while all orders have been canceled
        }

        private async Task RunAllTestGroups()
        {
            foreach (var (Condition, Tests) in _testGroups)
                foreach (OrderType orderType in Enum.GetValues(typeof(OrderType)))
                    foreach (OrderSide orderSide in Enum.GetValues(typeof(OrderSide)))
                        if (orderType != OrderType.Position)
                        {
                            var set = new OrderBaseSet(orderType, orderSide);
                            if (Condition(set))
                                await Tests.Run(set);
                        }
        }

        private void CleanUpWorkspace()
        {
            Status.WriteLine("Cleaning up...");

            foreach (var netPosition in Account.NetPositions.ToList())
                CloseNetPosition(CloseNetPositionRequest.Template.Create().WithParams(netPosition.Symbol).MakeRequest());

            foreach (var order in Account.Orders.ToList())
            {
                if (order.Type == OrderType.Position)
                    CloseOrder(CloseOrderRequest.Template.Create().WithParams(order.Id).MakeRequest());
                else
                    CancelOrder(order.Id);
            }
        }

        internal void PrintDebug(string message)
        {
            if (UseDebug)
                Print(message);
        }

        #region Test Order Actions
        //private async Task TestOpenOrder(OrderTemplate template, bool activate = true, bool fill = true)
        //{
        //    var request = OpenOrderRequest.Template.Create().WithParams(Symbol.Name, template.Side, template.Type, template.Volume,
        //        template.Price, template.StopPrice, template.MaxVisibleVolume, template.TP, template.SL, template.Comment, template.Options,
        //        "tag", template.Expiration, template.GetSlippageInPercent()).MakeRequest();

        //    var response = template.Async ? await OpenOrderAsync(request) : OpenOrder(request);

        //    response.ThrowIfFailed(TestOrderAction.Open);

        //    await WaitOpenAndUpdateTemplate(template);

        //    if (!activate && !template.IsImmediateFill)
        //        try
        //        {
        //            template.Verification();
        //        }
        //        catch (Exception ex)
        //        {
        //            PrintError(ex.Message);
        //        }

        //    if ((activate && fill) || template.IsImmediateFill)
        //        await TryPerformTest(() => TestEventFillOrder(template), 1);
        //}

        //private async Task TestCancelOrder(OrderTemplate template)
        //{
        //    var response = template.Async ? await CancelOrderAsync(template.Id) : CancelOrder(template.Id);

        //    response.ThrowIfFailed(TestOrderAction.Cancel);

        //    await WaitAndStoreEvent<OrderCanceledEventArgs>(template, CancelEventTimeout);
        //}

        //private async Task TestCloseOrder(OrderTemplate template, double? volume = null)
        //{
        //    var request = CloseOrderRequest.Template.Create().WithParams(template.Id, volume, template.GetSlippageInPercent()).MakeRequest();

        //    var response = template.Async ? await CloseOrderAsync(request) : CloseOrder(request);

        //    response.ThrowIfFailed(TestOrderAction.Close);

        //    template.Verification(volume == null);

        //    await WaitAndStoreEvent<OrderClosedEventArgs>(template, CloseEventTimeout);
        //}

        //private async Task TestCloseBy(OrderTemplate template, OrderTemplate inversed)
        //{
        //    var resultCopy = template.Volume < inversed.Volume ? inversed.Copy() : template.Copy();

        //    var response = template.Async ? await CloseOrderByAsync(template.Id, inversed.Id) : CloseOrderBy(template.Id, inversed.Id);

        //    response.ThrowIfFailed(TestOrderAction.CloseBy);

        //    if (template.Volume != inversed.Volume)
        //        await WaitOpenAndUpdateTemplate(resultCopy);

        //    await WaitAndStoreEvent<OrderClosedEventArgs>(template.Volume < inversed.Volume ? inversed : template, CloseEventTimeout);
        //    await WaitAndStoreEvent<OrderClosedEventArgs>(template.Volume < inversed.Volume ? template : inversed, CloseEventTimeout);

        //    if (template.Volume != inversed.Volume)
        //        await TryPerformTest(() => TestCloseOrder(resultCopy));
        //}

        //private async Task TestModifyOrder(OrderTemplate template)
        //{
        //    var request = GetModifyRequest(template);

        //    var response = template.Async ? await ModifyOrderAsync(request) : ModifyOrder(request);

        //    response.ThrowIfFailed(TestOrderAction.Modify);

        //    await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);

        //    template.Verification();
        //}

        //private static ModifyOrderRequest GetModifyRequest(OrderTemplate template) => ModifyOrderRequest.Template.Create().WithParams(template.Id, template.Price, template.StopPrice, template.Volume, template.MaxVisibleVolume,
        //        template.TP, template.SL, template.Comment, template.Expiration, template.Options, template.GetSlippageInPercent()).MakeRequest();


        #endregion

        #region History test

        private async Task FullHistoryTestRun()
        {
            //if (LoadAllHistory)
            //    ReportsIteratorTest();

            await DoQueryTests(false);
            await DoQueryTests(true);
        }

        private async Task DoQueryTests(bool async)
        {
            var from = _historyStorage.First().TradeReportTimestamp.AddSeconds(-1);
            var to = _historyStorage.Last().TradeReportTimestamp.AddSeconds(1);

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
            Print($"Query trade history: async={async} options={options}");

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

            for (int i = 0; i < reports.Count; i++)
            {
                try
                {
                    verifiers[i].VerifyTradeReport(reports[i]);
                }
                catch (Exception ex)
                {
                    PrintError(ex.Message);
                }
            }
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

        //private async Task TestEventFillOrder(OrderTemplate template)
        //{
        //    if (template.Type == OrderType.StopLimit)
        //    {
        //        await WaitAndStoreEvent<OrderActivatedEventArgs>(template, ActivateEventTimeout);
        //        await WaitOpenAndUpdateTemplate(template, true);
        //    }

        //    await WaitAndStoreEvent<OrderFilledEventArgs>(template, FillEventTimeout, Account.Type != AccountTypes.Gross);

        //    if (Account.Type == AccountTypes.Gross)
        //    {
        //        PrintDebug("To Position");
        //        await WaitOpenAndUpdateTemplate(template, true, true);
        //    }
        //}


        //private async Task WaitAndStoreEvent<T>(OrderTemplate template, TimeSpan delay, bool store = true)
        //{
        //    var args = await WaitEvent<T>(delay);

        //    if (store)
        //        _historyStorage.Add(HistoryOrderTemplate.Create(template, args));
        //}
        #endregion
    }
}