using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class FeedStrategy : IFeedBuferStrategyContext, CustomFeedProvider
    {
        [NonSerialized]
        private MarketStateFixture _marketFixture;
        [NonSerialized]
        private IFeedSubscription _defaultSubscription;
        private readonly List<SetupAction> _setupActions = new List<SetupAction>();
        private CrossDomainProxy _proxy;

        public FeedStrategy()
        {
        }

        internal IFixtureContext ExecContext { get; private set; }
        internal IFeedProvider Feed { get; private set; }
        internal IFeedHistoryProvider FeedHistory { get; private set; }
        internal SubscriptionFixtureManager RateDispenser => ExecContext.Dispenser;

        public abstract int BufferSize { get; }
        public abstract IFeedBuffer MainBuffer { get; }
        public abstract IEnumerable<string> BufferedSymbols { get; }

        internal abstract void OnInit();
        public FeedBufferStrategy BufferingStrategy { get; private set; }
        protected abstract BufferUpdateResult UpdateBuffers(RateUpdate update);
        protected abstract RateUpdate Aggregate(RateUpdate last, QuoteEntity quote);
        protected abstract BarSeries GetBarSeries(string symbol);
        protected abstract BarSeries GetBarSeries(string symbol, BarPriceType side);
        protected abstract FeedStrategy CreateClone();

        internal void Init(IFixtureContext executor, FeedBufferStrategy bStrategy, MarketStateFixture marketFixture)
        {
            ExecContext = executor;
            _marketFixture = marketFixture;
            Feed = executor.FeedProvider;
            FeedHistory = executor.FeedHistory;
            BufferingStrategy = bStrategy;
            RateDispenser.ClearUserSubscriptions();
            OnInit();
            BufferingStrategy.Init(this);
            _setupActions.ForEach(a => a.Apply(this));
        }

        internal virtual void Start()
        {
            _proxy = new CrossDomainProxy(this);
            Feed.Sync.Invoke(_proxy.StartStrategy);
            ExecContext.EnqueueCustomInvoke(b => LoadDataAndBuild());
            ExecContext.Builder.CustomFeedProvider = this;
        }

        internal virtual void Stop()
        {
            Feed.Sync.Invoke(_proxy.StopStrategy);
            CancelDefaultSubscription();
            _proxy.Dispose();
            _proxy = null;
        }

        internal FeedStrategy Clone()
        {
            var copy = CreateClone();
            copy._setupActions.AddRange(_setupActions);
            return copy;
        }

        internal void SetUserSubscription(string symbol, int depth)
        {
            RateDispenser.SetUserSubscription(symbol, depth);
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

        private void Feed_RatesUpdated(List<QuoteEntity> updates)
        {
            foreach (var update in updates)
                ExecContext.EnqueueQuote(update);
        }

        private void Feed_RateUpdated(QuoteEntity upd)
        {
            ExecContext.EnqueueQuote(upd);
        }

        private void InitDefaultSubscription()
        {
            _defaultSubscription = RateDispenser.AddSubscription(q => { });
            var snaphsot = _defaultSubscription.AddOrModify(BufferedSymbols, 1);
            ApplySnaphost(snaphsot);
        }

        internal void SubscribeAll()
        {
            var symbols = ExecContext.Builder.Symbols.Select(s => s.Name);
            var snaphsot = _defaultSubscription.AddOrModify(symbols, 1);
            ApplySnaphost(snaphsot);
        }

        private void CancelDefaultSubscription()
        {
            _defaultSubscription.CancelAll();
            _defaultSubscription = null;
        }

        private void ApplySnaphost(List<QuoteEntity> snaphsot)
        {
            if (snaphsot != null)
            {
                foreach (var rate in snaphsot)
                    _marketFixture.UpdateRate(rate);
            }
        }

        internal BufferUpdateResult ApplyUpdate(RateUpdate update, out AlgoMarketNode node)
        {
            node = _marketFixture.UpdateRate(update);

            var result = UpdateBuffers(update);

            if (result.IsLastUpdated)
                ExecContext.Builder.InvokeCalculate(true);

            for (int i = 0; i < result.ExtendedBy; i++)
            {
                ExecContext.Builder.IncreaseVirtualPosition();
                ExecContext.Builder.InvokeCalculate(false);
            }

            RateDispenser.OnUpdateEvent(node);

            return result;
        }

        internal RateUpdate InvokeAggregate(RateUpdate last, QuoteEntity quote)
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
            return GetBarSeries(symbol, side);
        }

        IEnumerable<Bar> CustomFeedProvider.GetBars(string symbol, TimeFrames timeFrame, DateTime from, DateTime to, BarPriceType side, bool backwardOrder)
        {
            const int pageSize = 500;
            List<BarEntity> page;

            int i = 0;
            var timeRef = backwardOrder ? to : from;

            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            if (backwardOrder)
            {
                while (true)
                {
                    page = FeedHistory.QueryBars(symbol, side, timeRef, -pageSize, timeFrame);

                    for (i = page.Count - 1; i >= 0; --i)
                        if (page[i].CloseTime > to) //do not include the right border in the segment
                            continue;
                        else
                        if (page[i].OpenTime >= from)
                            yield return page[i];
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
                    page = FeedHistory.QueryBars(symbol, side, timeRef, pageSize, timeFrame);

                    for (i = 0; i < page.Count; ++i)
                        if (page[i].CloseTime <= to)
                            yield return page[i];
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
            const int pageSize = 500;
            List<BarEntity> page;
            int pageIndex;

            from = from.ToUniversalTime();
            var backwardOrder = count < 0;
            count = System.Math.Abs(count);

            while (count > 0)
            {
                if (backwardOrder)
                {
                    page = FeedHistory.QueryBars(symbol, side, from, pageSize, timeFrame);
                    pageIndex = page.Count - 1;

                    while (pageIndex > 0)
                    {
                        var item = page[pageIndex];
                        pageIndex--;
                        yield return item;
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < pageSize)
                        break; //last page
                    from = page.First().OpenTime.AddMilliseconds(-1);
                }
                else
                {
                    page = FeedHistory.QueryBars(symbol, side, from, -pageSize, timeFrame);
                    pageIndex = 0;

                    while (pageIndex < page.Count)
                    {
                        var item = page[pageIndex];
                        pageIndex++;
                        yield return item;
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < pageSize)
                        break; //last page
                    from = page.Last().CloseTime.AddMilliseconds(1);
                }
            }
        }

        IEnumerable<Quote> CustomFeedProvider.GetQuotes(string symbol, DateTime from, DateTime to, bool level2, bool backwardOrder)
        {
            const int pageSize = 500;
            List<QuoteEntity> page;
            int pageIndex;

            from = from.ToUniversalTime();
            to = to.ToUniversalTime();

            if (backwardOrder)
            {
                page = FeedHistory.QueryTicks(symbol, to, -pageSize, level2);
                pageIndex = page.Count - 1;

                while (true)
                {
                    if (pageIndex < 0)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.First().Time.AddMilliseconds(-1);
                        page = FeedHistory.QueryTicks(symbol, timeRef, -pageSize, level2);
                        if (page.Count == 0)
                            break;
                        pageIndex = page.Count - 1;
                    }

                    var item = page[pageIndex];
                    if (item.Time < from)
                        break;
                    pageIndex--;
                    yield return item;
                }
            }
            else
            {
                page = FeedHistory.QueryTicks(symbol, from, pageSize, level2);
                pageIndex = 0;

                while (true)
                {
                    if (pageIndex >= page.Count)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.Last().Time.AddMilliseconds(1);
                        page = FeedHistory.QueryTicks(symbol, timeRef, pageSize, level2);
                        if (page.Count == 0)
                            break;
                        pageIndex = 0;
                    }

                    var item = page[pageIndex];
                    if (item.Time > to)
                        break;
                    pageIndex++;
                    yield return item;
                }
            }
        }

        IEnumerable<Quote> CustomFeedProvider.GetQuotes(string symbol, DateTime from, int count, bool level2)
        {
            const int pageSize = 500;
            List<QuoteEntity> page;
            int pageIndex;

            from = from.ToUniversalTime();
            var backwardOrder = count < 0;
            count = System.Math.Abs(count);

            while (count > 0)
            {
                if (backwardOrder)
                {
                    page = FeedHistory.QueryTicks(symbol, from, -pageSize, level2);
                    pageIndex = page.Count - 1;

                    while (pageIndex > 0)
                    {
                        var item = page[pageIndex];
                        pageIndex--;
                        yield return item;
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < pageSize)
                        break; //last page
                    from = page.First().Time.AddMilliseconds(-1);
                }
                else
                {
                    page = FeedHistory.QueryTicks(symbol, from, pageSize, level2);
                    pageIndex = 0;

                    while (pageIndex < page.Count)
                    {
                        var item = page[pageIndex];
                        pageIndex++;
                        yield return item;
                        count--;
                        if (count <= 0)
                            break;
                    }

                    if (page.Count < pageSize)
                        break; //last page
                    from = page.Last().Time.AddMilliseconds(1);
                }
            }
        }

        void CustomFeedProvider.Subscribe(string symbol, int depth)
        {
            RateDispenser.SetUserSubscription(symbol, depth);
        }

        void CustomFeedProvider.Unsubscribe(string symbol)
        {
            RateDispenser.RemoveUserSubscription(symbol);
        }

        #endregion

        private class CrossDomainProxy : CrossDomainObject
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
                    _strategy.Feed.RateUpdated += Feed_RateUpdated;
                    _strategy.Feed.RatesUpdated += Feed_RatesUpdated;
                }
                finally
                {
                    _strategy.RateDispenser.IsSynchronized = false;
                }
            }

            private void Feed_RatesUpdated(List<QuoteEntity> updates) => _strategy.Feed_RatesUpdated(updates);
            private void Feed_RateUpdated(QuoteEntity upd) => _strategy.Feed_RateUpdated(upd);

            public void StopStrategy()
            {
                _strategy.RateDispenser.IsSynchronized = true;
                try
                {
                    _strategy.RateDispenser.Stop();
                    _strategy.Feed.RateUpdated -= Feed_RateUpdated;
                    _strategy.Feed.RatesUpdated -= Feed_RatesUpdated;
                }
                finally
                {
                    _strategy.RateDispenser.IsSynchronized = false;
                }
            }
        }

        [Serializable]
        public abstract class SetupAction
        {
            public abstract void Apply(FeedStrategy fStartegy);
        }

        [Serializable]
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
