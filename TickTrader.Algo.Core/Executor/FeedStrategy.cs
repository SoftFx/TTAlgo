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
        private Action<QuoteEntity> _rateUpdateCallback;

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

        internal void Init(IFixtureContext executor, FeedBufferStrategy bStrategy, Action<QuoteEntity> rateUpdateCallback)
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

        internal void ApplyUpdate(RateUpdate update)
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

            RateDispenser.OnUpdateEvent(lastQuote);
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

        IEnumerable<Quote> CustomFeedProvider.GetQuotes(string symbol, DateTime from, DateTime to, bool level2, bool backwardOrder)
        {
            var ticks = Feed.QueryTicks(symbol, from, to, level2 ? 0 : 1);

            if (backwardOrder)
            {
                for (int i = ticks.Count - 1; i >= 0; i--)
                    yield return ticks[i];
            }
            else
            {
                for (int i = 0; i < ticks.Count; i++)
                    yield return ticks[i];
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
