using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public abstract class FeedStrategy : CrossDomainObject, IFeedBuferStrategyContext, CustomFeedProvider
    {
        private Action<RateUpdate> _rateUpdateCallback;

        private List<Action> setupActions = new List<Action>();

        public FeedStrategy()
        {
        }

        internal IFixtureContext ExecContext { get; private set; }
        internal IPluginFeedProvider Feed { get; private set; }
        internal SubscriptionManager RateDispenser => ExecContext.Dispenser;

        public abstract int BufferSize { get; }
        public abstract IFeedBuffer MainBuffer { get; }

        internal abstract void OnInit();
        public FeedBufferStrategy BufferingStrategy { get; private set; }
        protected abstract BufferUpdateResult UpdateBuffers(RateUpdate update);
        protected abstract RateUpdate Aggregate(RateUpdate last, QuoteEntity quote);
        protected abstract BarSeries GetBarSeries(string symbol);
        protected abstract BarSeries GetBarSeries(string symbol, BarPriceType side);
        //protected abstract IEnumerable<Bar> QueryBars(string symbol, TimeFrames timeFrame, DateTime from, DateTime to);
        //protected abstract IEnumerable<Quote> QueryQuotes(string symbol, DateTime from, DateTime to, bool level2);

        internal void Init(IFixtureContext executor, FeedBufferStrategy bStrategy, Action<RateUpdate> rateUpdateCallback)
        {
            ExecContext = executor;
            _rateUpdateCallback = rateUpdateCallback;
            Feed = executor.FeedProvider;
            BufferingStrategy = bStrategy;
            RateDispenser.ClearUserSubscriptions();
            OnInit();
            BufferingStrategy.Init(this);
            BufferingStrategy.Start();
            setupActions.ForEach(a => a());
        }

        internal virtual void Start()
        {
            RateDispenser.Start();
            Feed.Sync.Invoke(StartStrategy);
        }

        internal virtual void Stop()
        {
            RateDispenser.Stop();
            Feed.Sync.Invoke(StopStrategy);
        }

        internal void SetSubscribed(string symbol, int depth)
        {
            RateDispenser.SetUserSubscription(symbol, depth);
        }

        protected void AddSetupAction(Action setupAction)
        {
            setupActions.Add(setupAction);
        }

        private void StartStrategy()
        {
            Feed.Subscribe(Feed_FeedUpdated);
            ExecContext.EnqueueCustomInvoke(b => BatchBuild(BufferSize));

            // apply snapshot
            foreach (var quote in Feed.GetSnapshot())
            {
                ExecContext.Builder.Symbols.SetRate(quote);
                _rateUpdateCallback(quote);
            }

            ExecContext.Builder.CustomFeedProvider = this;
        }

        private void StopStrategy()
        {
            Feed.Unsubscribe();
        }

        private void BatchBuild(int x)
        {
            var builder = ExecContext.Builder;

            builder.StartBatch();

            for (int i = 0; i < x; i++)
            {
                builder.IncreaseVirtualPosition();
                builder.InvokeCalculate(false);
            }

            builder.StopBatch();
        }

        private void Feed_FeedUpdated(QuoteEntity[] updates)
        {
            foreach (var update in updates)
                ExecContext.EnqueueQuote(update);
        }

        internal BufferUpdateResult ApplyUpdate(RateUpdate update, bool hidden)
        {
            var lastQuote = update.LastQuote;

            ExecContext.Builder.Symbols.SetRate(lastQuote);

            var result = UpdateBuffers(update);

            if (result.IsLastUpdated)
                ExecContext.Builder.InvokeCalculate(true);

            for (int i = 0; i < result.ExtendedBy; i++)
            {
                ExecContext.Builder.IncreaseVirtualPosition();
                ExecContext.Builder.InvokeCalculate(false);
            }

            _rateUpdateCallback((QuoteEntity)lastQuote);

            if (!hidden)
                RateDispenser.OnUpdateEvent(lastQuote);

            return result;
        }

        internal RateUpdate InvokeAggregate(RateUpdate last, QuoteEntity quote)
        {
            return Aggregate(last, quote);
        }

        #region IFeedStrategyContext

        //IPluginFeedProvider IFeedFixtureContext.Feed { get { return Feed; } }

        //void IFeedFixtureContext.Add(IRateSubscription subscriber)
        //{
        //    dispenser.Add(subscriber);
        //}

        //void IFeedFixtureContext.Remove(IRateSubscription subscriber)
        //{
        //    dispenser.Remove(subscriber);
        //}

        #endregion IFeedStrategyContext

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
            int pageIndex;

            from = from.ToUniversalTime();
            to = to.ToUniversalTime().AddMilliseconds(-1);

            if (backwardOrder)
            {
                page =  Feed.QueryBars(symbol, side, to, -pageSize, timeFrame);
                pageIndex = page.Count - 1;

                while (true)
                {
                    if (pageIndex < 0)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.First().OpenTime.AddMilliseconds(-1);
                        page = Feed.QueryBars(symbol, side, timeRef, -pageSize, timeFrame);
                        if (page.Count == 0)
                            break;
                        pageIndex = page.Count - 1;
                    } 

                    var item = page[pageIndex];
                    if (item.OpenTime < from)
                        break;
                    pageIndex--;
                    yield return item;
                }
            }
            else
            {
                page = Feed.QueryBars(symbol, side, from, pageSize, timeFrame);
                pageIndex = 0;

                while (true)
                {
                    if (pageIndex >= page.Count)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.Last().CloseTime.AddMilliseconds(1);
                        page = Feed.QueryBars(symbol, side, timeRef, pageSize, timeFrame);
                        if (page.Count == 0)
                            break;
                        pageIndex = 0;
                    }

                    var item = page[pageIndex];
                    if (item.OpenTime > to)
                        break;
                    pageIndex++;
                    yield return item;
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
                    page = Feed.QueryBars(symbol, side, from, -pageSize, timeFrame);
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
                    page = Feed.QueryBars(symbol, side, from, pageSize, timeFrame);
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
                page = Feed.QueryTicks(symbol, to, -pageSize, level2);
                pageIndex = page.Count - 1;

                while (true)
                {
                    if (pageIndex < 0)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.First().Time.AddMilliseconds(-1);
                        page = Feed.QueryTicks(symbol, timeRef, -pageSize, level2);
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
                page = Feed.QueryTicks(symbol, from, pageSize, level2);
                pageIndex = 0;

                while (true)
                {
                    if (pageIndex >= page.Count)
                    {
                        if (page.Count < pageSize)
                            break; //last page
                        var timeRef = page.Last().Time.AddMilliseconds(1);
                        page = Feed.QueryTicks(symbol, timeRef, pageSize, level2);
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
                    page = Feed.QueryTicks(symbol, from, -pageSize, level2);
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
                    page = Feed.QueryTicks(symbol, from, pageSize, level2);
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
    }
}
