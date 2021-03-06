using Google.Protobuf.WellKnownTypes;
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
        private IQuoteSub _defaultSubscription;
        private readonly List<SetupAction> _setupActions = new List<SetupAction>();
        private CrossDomainProxy _proxy;
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
        protected abstract IRateInfo Aggregate(IRateInfo last, QuoteInfo quote);
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
            _proxy = new CrossDomainProxy(this);
            FeedProvider.Sync.Invoke(_proxy.StartStrategy);
            ExecContext.EnqueueCustomInvoke(b => LoadDataAndBuild());
            ExecContext.Builder.CustomFeedProvider = this;
        }

        internal virtual void Stop()
        {
            FeedProvider.Sync.Invoke(_proxy.StopStrategy);
            CancelDefaultSubscription();
            _proxy = null;
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

        private void Feed_RatesUpdated(List<QuoteInfo> updates)
        {
            foreach (var update in updates)
                ExecContext.EnqueueQuote(update);
        }

        private void Feed_RateUpdated(QuoteInfo upd)
        {
            ExecContext.EnqueueQuote(upd);
        }

        private void InitDefaultSubscription()
        {
            _defaultSubscription = FeedProvider.GetSubscription();
            _defaultSubscription.Modify(BufferedSymbols, 1);
        }

        protected void AddSubscription(string symbol)
        {
            _defaultSubscription.Modify(symbol, 1);
        }

        private void CancelDefaultSubscription()
        {
            _defaultSubscription.Dispose();
            _defaultSubscription = null;
        }

        internal BufferUpdateResult ApplyUpdate(IRateInfo update)
        {
            var node = _marketFixture.Market.GetUpdateNode(update);

            var result = UpdateBuffers(update);

            var modelUpdate = new BufferUpdateResult();
            if (ModelTimeline.IsRealTime)
            {
                CalculateIndicators(result);
            }
            else
            {
                if (update.Symbol == _mainSymbol)
                {
                    _mainSymbolUpdateResult += result;
                    modelUpdate += ModelTimeline.Update(update.LastQuote.Time);
                }
                if (modelUpdate.ExtendedBy > 0)
                {
                    CalculateIndicators(_mainSymbolUpdateResult);
                    _mainSymbolUpdateResult = new BufferUpdateResult();
                }
            }

            RateDispenser.OnUpdateEvent(node);

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

        internal IRateInfo InvokeAggregate(IRateInfo last, QuoteInfo quote)
        {
            return Aggregate(last, quote);
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
            const int pageSize = 500;
            List<BarData> page;

            int i = 0;
            var fromTime = new UtcTicks(from);
            var toTime = new UtcTicks(to);
            var timeRef = (backwardOrder ? to : from).ToTimestamp();

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

                    timeRef = page.First().CloseTime.ToTimestamp();
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

                    timeRef = page.Last().OpenTime.ToTimestamp();
                }
            }
        }

        IEnumerable<Bar> CustomFeedProvider.GetBars(string symbol, TimeFrames timeFrame, DateTime from, int count, BarPriceType side)
        {
            const int pageSize = 500;
            List<BarData> page;
            int pageIndex;

            var fromTime = new UtcTicks(from);
            var backwardOrder = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                if (backwardOrder)
                {
                    page = FeedHistory.QueryBars(symbol, side.ToDomainEnum(), timeFrame.ToDomainEnum(), fromTime.ToTimestamp(), pageSize);
                    pageIndex = page.Count - 1;

                    while (pageIndex > 0)
                    {
                        var item = page[pageIndex];
                        pageIndex--;
                        yield return new BarEntity(item);
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < pageSize)
                        break; //last page
                    fromTime = page.First().OpenTime.AddMs(-1);
                }
                else
                {
                    page = FeedHistory.QueryBars(symbol, side.ToDomainEnum(), timeFrame.ToDomainEnum(), fromTime.ToTimestamp(), -pageSize);
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

                    if (page.Count < pageSize)
                        break; //last page
                    fromTime = page.Last().CloseTime.AddMs(1);
                }
            }
        }

        IEnumerable<Quote> CustomFeedProvider.GetQuotes(string symbol, DateTime from, DateTime to, bool level2, bool backwardOrder)
        {
            const int pageSize = 500;
            List<QuoteInfo> page;
            int pageIndex;

            var fromTime = new UtcTicks(from);
            var toTime = new UtcTicks(to);

            if (backwardOrder)
            {
                page = FeedHistory.QueryQuotes(symbol, toTime.ToTimestamp(), -pageSize, level2);
                pageIndex = page.Count - 1;

                while (true)
                {
                    if (pageIndex < 0)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.First().Time.AddMs(-1);
                        page = FeedHistory.QueryQuotes(symbol, timeRef.ToTimestamp(), -pageSize, level2);
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
                page = FeedHistory.QueryQuotes(symbol, fromTime.ToTimestamp(), pageSize, level2);
                pageIndex = 0;

                while (true)
                {
                    if (pageIndex >= page.Count)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.Last().Time.AddMs(1);
                        page = FeedHistory.QueryQuotes(symbol, timeRef.ToTimestamp(), pageSize, level2);
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
            const int pageSize = 500;
            List<QuoteInfo> page;
            int pageIndex;

            var fromTime = new UtcTicks(from);
            var backwardOrder = count < 0;
            count = Math.Abs(count);

            while (count > 0)
            {
                if (backwardOrder)
                {
                    page = FeedHistory.QueryQuotes(symbol, fromTime.ToTimestamp(), -pageSize, level2);
                    pageIndex = page.Count - 1;

                    while (pageIndex > 0)
                    {
                        var item = page[pageIndex];
                        pageIndex--;
                        yield return new QuoteEntity(item);
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < pageSize)
                        break; //last page
                    fromTime = page.First().Time.AddMs(-1);
                }
                else
                {
                    page = FeedHistory.QueryQuotes(symbol, fromTime.ToTimestamp(), pageSize, level2);
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

                    if (page.Count < pageSize)
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

        private class CrossDomainProxy
        {
            private readonly FeedStrategy _strategy;

            public CrossDomainProxy(FeedStrategy strategy)
            {
                _strategy = strategy;
            }

            public void StartStrategy()
            {
                _strategy.RateDispenser.IsSynchronized = true;
                try
                {
                    _strategy.RateDispenser.Start();
                    _strategy.InitDefaultSubscription();
                    _strategy.FeedProvider.RateUpdated += Feed_RateUpdated;
                    _strategy.FeedProvider.RatesUpdated += Feed_RatesUpdated;
                }
                finally
                {
                    _strategy.RateDispenser.IsSynchronized = false;
                }
            }

            private void Feed_RatesUpdated(List<QuoteInfo> updates) => _strategy.Feed_RatesUpdated(updates);
            private void Feed_RateUpdated(QuoteInfo upd) => _strategy.Feed_RateUpdated(upd);

            public void StopStrategy()
            {
                _strategy.RateDispenser.IsSynchronized = true;
                try
                {
                    _strategy.RateDispenser.Stop();
                    _strategy.FeedProvider.RateUpdated -= Feed_RateUpdated;
                    _strategy.FeedProvider.RatesUpdated -= Feed_RatesUpdated;
                }
                finally
                {
                    _strategy.RateDispenser.IsSynchronized = false;
                }
            }
        }

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
