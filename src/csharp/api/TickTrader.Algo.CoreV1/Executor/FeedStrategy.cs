using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public abstract class FeedStrategy : IFeedBuferStrategyContext, CustomFeedProvider
    {
        private MarketStateFixture _marketFixture;
        private IBarSub _defaultBarSub;
        private IDisposable _barSubHandler;
        private readonly List<SetupAction> _setupActions = new List<SetupAction>();
        private string _mainSymbol;
        private BufferUpdateResult _mainSymbolUpdateResult;

        public FeedStrategy()
        {
        }

        internal IFixtureContext ExecContext { get; private set; }
        internal IFeedProvider FeedProvider { get; private set; }
        internal IFeedHistoryProvider FeedHistory { get; private set; }
        internal SubscriptionFixtureManager RateDispenser => ExecContext.Dispenser;
        internal TimelineFixture ModelTimeline { get; private set; }

        public abstract int BufferSize { get; }
        public abstract IFeedBuffer MainBuffer { get; }
        public abstract IEnumerable<string> BufferedSymbols { get; }

        internal abstract void OnInit();
        public FeedBufferStrategy BufferingStrategy { get; private set; }
        protected abstract BufferUpdateResult UpdateBuffers(IRateInfo update);
        protected abstract BarSeries GetBarSeries(string symbol);
        protected abstract BarSeries GetBarSeries(string symbol, Feed.Types.MarketSide side);
        protected abstract FeedStrategy CreateClone();

        internal void Init(IFixtureContext executor, FeedBufferStrategy bStrategy, MarketStateFixture marketFixture)
        {
            ExecContext = executor;
            _marketFixture = marketFixture;
            _mainSymbol = executor.MainSymbolCode;
            FeedProvider = executor.FeedProvider;
            FeedHistory = executor.FeedHistory;
            BufferingStrategy = bStrategy;
            RateDispenser.ClearUserSubscriptions();
            OnInit();
            BufferingStrategy.Init(this);
            _setupActions.ForEach(a => a.Apply(this));

            ModelTimeline = new TimelineFixture(ExecContext.ModelTimeFrame);
            _mainSymbolUpdateResult = new BufferUpdateResult();
        }

        internal virtual void Start()
        {
            RateDispenser.Start();
            InitDefaultSubscription();
            FeedProvider.QuoteUpdated += Feed_QuoteUpdated;

            ExecContext.EnqueueCustomInvoke(b => LoadDataAndBuild());
            ExecContext.Builder.CustomFeedProvider = this;
        }

        internal virtual void Stop()
        {
            RateDispenser.Stop();
            FeedProvider.QuoteUpdated -= Feed_QuoteUpdated;

            CancelDefaultSubscription();
        }

        internal FeedStrategy Clone()
        {
            var copy = CreateClone();
            copy._setupActions.AddRange(_setupActions);
            return copy;
        }

        protected void AddSetupAction(SetupAction setupAction)
        {
            _setupActions.Add(setupAction);
        }

        private void LoadDataAndBuild()
        {
            BufferingStrategy.Start();

            var builder = ExecContext.Builder;
            var barCount = BufferSize;

            builder.StartBatch();

            for (int i = 0; i < barCount; i++)
            {
                builder.IncreaseVirtualPosition();
                builder.InvokeCalculate(false);
            }

            builder.StopBatch();
        }

        private void Feed_BarUpdated(BarUpdate bar) => ExecContext.EnqueueBar(bar);

        private void Feed_QuoteUpdated(QuoteInfo quote) => ExecContext.EnqueueQuote(quote);

        private void InitDefaultSubscription()
        {
            _defaultBarSub = FeedProvider.GetBarSub();
            _defaultBarSub.Modify(BufferedSymbols.Select(s => BarSubUpdate.Upsert(new BarSubEntry(s, ExecContext.TimeFrame))).ToList());
            _barSubHandler = _defaultBarSub.AddHandler(bar => ExecContext.EnqueueBar(bar));
        }

        protected void AddSubscription(string symbol)
        {
            _defaultBarSub.Modify(BarSubUpdate.Upsert(new BarSubEntry(symbol, ExecContext.TimeFrame)));
        }

        private void CancelDefaultSubscription()
        {
            _barSubHandler.Dispose();
            _defaultBarSub.Dispose();
            _defaultBarSub = null;
        }

        internal BufferUpdateResult ApplyUpdate(FeedUpdateSummary feedUpdate)
        {
            var market = _marketFixture.Market;
            foreach (var quote in feedUpdate.NewQuotes)
            {
                var node = market.GetSymbolNodeOrNull(quote.Symbol);
                node?.SymbolInfo.UpdateRate(quote);
            }

            var result = new BufferUpdateResult();
            var modelUpdate = new BufferUpdateResult();
            foreach (var update in feedUpdate.RateUpdates)
            {
                var tmpRes = UpdateBuffers(update);
                result += tmpRes;

                if (!ModelTimeline.IsRealTime && update.Symbol == _mainSymbol)
                {
                    _mainSymbolUpdateResult += tmpRes;
                    modelUpdate += ModelTimeline.Update(update.LastQuote.Time);
                }
            }

            if (ModelTimeline.IsRealTime)
            {
                CalculateIndicators(result);
            }
            else if (modelUpdate.ExtendedBy > 0)
            {
                CalculateIndicators(_mainSymbolUpdateResult);
                _mainSymbolUpdateResult = new BufferUpdateResult();
            }

            foreach (var quote in feedUpdate.NewQuotes)
            {
                var node = market.GetSymbolNodeOrNull(quote.Symbol);
                RateDispenser.OnUpdateEvent(node);
            }

            if (ModelTimeline.IsRealTime || modelUpdate.ExtendedBy > 0)
                ExecContext.Builder.InvokeOnModelTick();

            return result;
        }

        private void CalculateIndicators(BufferUpdateResult result)
        {
            if (result.IsLastUpdated)
                ExecContext.Builder.InvokeCalculate(true);

            for (int i = 0; i < result.ExtendedBy; i++)
            {
                ExecContext.Builder.IncreaseVirtualPosition();
                ExecContext.Builder.InvokeCalculate(false);
            }
        }

        #region IFeedBufferController

        IFeedBuffer IFeedBuferStrategyContext.MainBuffer => MainBuffer;

        void IFeedBuferStrategyContext.TruncateBuffers(int bySize)
        {
            ExecContext.Builder.TruncateBuffers(bySize);
        }

        #endregion

        #region CustomFeedProvider

        BarSeries CustomFeedProvider.GetBarSeries(string symbol)
        {
            return GetBarSeries(symbol);
        }

        BarSeries CustomFeedProvider.GetBarSeries(string symbol, BarPriceType side)
        {
            return GetBarSeries(symbol, side.ToDomainEnum());
        }

        IEnumerable<Bar> CustomFeedProvider.GetBars(string symbol, TimeFrames timeframe, DateTime from, DateTime to, BarPriceType side, bool backwardOrder)
        {
            const int pageSize = 512;
            List<BarData> page;

            int i = 0;
            var fromTime = new UtcTicks(from);
            var toTime = new UtcTicks(to);
            var timeRef = backwardOrder ? toTime : fromTime;

            if (backwardOrder)
            {
                while (true)
                {
                    page = FeedHistory.QueryBars(symbol, side.ToDomainEnum(), timeframe.ToDomainEnum(), timeRef, -pageSize);

                    for (i = page.Count - 1; i >= 0; --i)
                        if (page[i].CloseTime > toTime) //do not include the right border in the segment
                            continue;
                        else
                        if (page[i].OpenTime >= fromTime)
                            yield return new BarEntity(page[i]);
                        else
                            break;

                    if (page.Count != pageSize || i >= 0)
                        break;

                    timeRef = page.First().CloseTime;
                }
            }
            else
            {
                while (true)
                {
                    page = FeedHistory.QueryBars(symbol, side.ToDomainEnum(), timeframe.ToDomainEnum(), timeRef, pageSize);

                    for (i = 0; i < page.Count; ++i)
                        if (page[i].CloseTime <= toTime)
                            yield return new BarEntity(page[i]);
                        else
                            break;

                    if (page.Count != pageSize || i != page.Count)
                        break;

                    timeRef = page.Last().OpenTime;
                }
            }
        }

        IEnumerable<Bar> CustomFeedProvider.GetBars(string symbol, TimeFrames timeFrame, DateTime from, int count, BarPriceType side)
        {
            const int pageSize = 512;
            List<BarData> page;
            int pageIndex;

            var fromTime = new UtcTicks(from);
            var backwardOrder = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                var requestCount = Math.Min(count, pageSize);
                if (backwardOrder)
                {
                    page = FeedHistory.QueryBars(symbol, side.ToDomainEnum(), timeFrame.ToDomainEnum(), fromTime, -requestCount);
                    pageIndex = page.Count - 1;

                    while (pageIndex >= 0)
                    {
                        var item = page[pageIndex];
                        pageIndex--;
                        yield return new BarEntity(item);
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < requestCount)
                        break; //last page
                    fromTime = page.First().OpenTime.AddMs(-1);
                }
                else
                {
                    page = FeedHistory.QueryBars(symbol, side.ToDomainEnum(), timeFrame.ToDomainEnum(), fromTime, requestCount);
                    pageIndex = 0;

                    while (pageIndex < page.Count)
                    {
                        var item = page[pageIndex];
                        pageIndex++;
                        yield return new BarEntity(item);
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < requestCount)
                        break; //last page
                    fromTime = page.Last().CloseTime.AddMs(1);
                }
            }
        }

        IEnumerable<Quote> CustomFeedProvider.GetQuotes(string symbol, DateTime from, DateTime to, bool level2, bool backwardOrder)
        {
            const int pageSize = 512;
            List<QuoteInfo> page;
            int pageIndex;

            var fromTime = new UtcTicks(from);
            var toTime = new UtcTicks(to);

            if (backwardOrder)
            {
                page = FeedHistory.QueryQuotes(symbol, toTime, -pageSize, level2);
                pageIndex = page.Count - 1;

                while (true)
                {
                    if (pageIndex < 0)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.First().Time.AddMs(-1);
                        page = FeedHistory.QueryQuotes(symbol, timeRef, -pageSize, level2);
                        if (page.Count == 0)
                            break;
                        pageIndex = page.Count - 1;
                    }

                    var item = page[pageIndex];
                    if (item.Time < fromTime)
                        break;
                    pageIndex--;
                    yield return new QuoteEntity(item);
                }
            }
            else
            {
                page = FeedHistory.QueryQuotes(symbol, fromTime, pageSize, level2);
                pageIndex = 0;

                while (true)
                {
                    if (pageIndex >= page.Count)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.Last().Time.AddMs(1);
                        page = FeedHistory.QueryQuotes(symbol, timeRef, pageSize, level2);
                        if (page.Count == 0)
                            break;
                        pageIndex = 0;
                    }

                    var item = page[pageIndex];
                    if (item.Time > toTime)
                        break;
                    pageIndex++;
                    yield return new QuoteEntity(item);
                }
            }
        }

        IEnumerable<Quote> CustomFeedProvider.GetQuotes(string symbol, DateTime from, int count, bool level2)
        {
            const int pageSize = 512;
            List<QuoteInfo> page;
            int pageIndex;

            var fromTime = new UtcTicks(from);
            var backwardOrder = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                var requestCount = Math.Min(count, pageSize);
                if (backwardOrder)
                {
                    page = FeedHistory.QueryQuotes(symbol, fromTime, -requestCount, level2);
                    pageIndex = page.Count - 1;

                    while (pageIndex >= 0)
                    {
                        var item = page[pageIndex];
                        pageIndex--;
                        yield return new QuoteEntity(item);
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < requestCount)
                        break; //last page
                    fromTime = page.First().Time.AddMs(-1);
                }
                else
                {
                    page = FeedHistory.QueryQuotes(symbol, fromTime, requestCount, level2);
                    pageIndex = 0;

                    while (pageIndex < page.Count)
                    {
                        var item = page[pageIndex];
                        pageIndex++;
                        yield return new QuoteEntity(item);
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < requestCount)
                        break; //last page
                    fromTime = page.Last().Time.AddMs(1);
                }
            }
        }

        void CustomFeedProvider.Subscribe(string symbol, int depth)
        {
            if (depth == 0) // in Algo.Api depth=0 is all bands available
                depth = SubscriptionDepth.MaxDepth;
            else if (depth < 0)
                depth = 1;

            RateDispenser.SetUserSubscription(symbol, depth);
        }

        void CustomFeedProvider.Unsubscribe(string symbol)
        {
            RateDispenser.RemoveUserSubscription(symbol);
        }

        #endregion


        public abstract class SetupAction
        {
            public abstract void Apply(FeedStrategy fStartegy);
        }

        public abstract class InputSetupAction : SetupAction
        {
            public InputSetupAction(string inputName, string symbol)
            {
                InputName = inputName;
                SymbolName = symbol;
            }

            public string InputName { get; }
            public string SymbolName { get; }
        }
    }
}
