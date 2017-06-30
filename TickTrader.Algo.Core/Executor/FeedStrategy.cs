using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public abstract class FeedStrategy : CrossDomainObject, IFeedBuferStrategyContext
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

        public void OnUserSubscribe(string symbolCode, int depth)
        {
            RateDispenser.SetUserSubscription(symbolCode, depth);
        }

        public void OnUserUnsubscribe(string symbolCode)
        {
            RateDispenser.RemoveUserSubscription(symbolCode);
        }

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

        protected void AddSetupAction(Action setupAction)
        {
            setupActions.Add(setupAction);
        }

        private void StartStrategy()
        {
            Feed.Subscribe(Feed_FeedUpdated);
            ExecContext.EnqueueTradeUpdate(b => BatchBuild(BufferSize));

            // apply snapshot
            foreach (var quote in Feed.GetSnapshot())
            {
                ExecContext.Builder.Symbols.SetRate(quote);
                _rateUpdateCallback(quote);
            }
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
    }
}
