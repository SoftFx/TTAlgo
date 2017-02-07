using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal abstract class FeedStrategy : CrossDomainObject,  IFeedFixtureContext
    {
        private SubscriptionManager dispenser;
        private Dictionary<string, SubscriptionFixture> userSubscriptions = new Dictionary<string, SubscriptionFixture>();

        public FeedStrategy(IPluginFeedProvider feed)
        {
            if (feed == null)
                throw new ArgumentNullException("feed");

            this.dispenser = new SubscriptionManager(feed);
            this.Feed = feed;
        }

        internal IFixtureContext ExecContext { get; private set; }
        internal IPluginFeedProvider Feed { get; private set; }

        public abstract int BufferSize { get; }
        public abstract ITimeRef TimeRef { get; }

        internal abstract void OnInit();
        protected abstract BufferUpdateResult UpdateBuffers(RateUpdate update);
        internal abstract void MapInput<TSrc, TVal>(string inputName, string symbolCode, Func<TSrc, TVal> selector);

        public void OnUserSubscribe(string symbolCode, int depth)
        {
            SubscriptionFixture fixture;
            if (userSubscriptions.TryGetValue(symbolCode, out fixture))
            {
                if (fixture.Depth == depth)
                    return;
                dispenser.Remove(fixture);
            }
            fixture = new SubscriptionFixture(this, symbolCode, depth);
            userSubscriptions[symbolCode] = fixture;
            dispenser.Add(fixture);
        }

        public void OnUserUnsubscribe(string symbolCode)
        {
            SubscriptionFixture fixture;
            if (userSubscriptions.TryGetValue(symbolCode, out fixture))
            {
                userSubscriptions.Remove(symbolCode);
                dispenser.Remove(fixture);
            }
        }

        internal void Init(IFixtureContext executor)
        {
            ExecContext = executor;
            userSubscriptions.Clear();
            dispenser.Reset();
            OnInit();
        }

        internal virtual void Start()
        {
            Feed.Sync.Invoke(StartStrategy);
        }

        internal virtual void Stop()
        {
            Feed.Sync.Invoke(StopStrategy);
        }

        private void StartStrategy()
        {
            Feed.Subscribe(Feed_FeedUpdated);
            ExecContext.Enqueue(b => BatchBuild(BufferSize));

            // apply snapshot
            foreach(var quote in Feed.GetSnapshot())
                ExecContext.Builder.Symbols.SetRate(quote);
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
                ExecContext.Enqueue(update);
        }

        internal void ApplyUpdate(RateUpdate update)
        {
            var lastQuote = update.LastQuotes[0];

            ExecContext.Builder.Symbols.SetRate(lastQuote);

            var result = UpdateBuffers(update);

            if (result.IsLastUpdated)
                ExecContext.Builder.InvokeCalculate(true);

            for (int i = 0; i < result.ExtendedBy; i++)
            {
                ExecContext.Builder.IncreaseVirtualPosition();
                ExecContext.Builder.InvokeCalculate(false);
            }

            dispenser.OnUpdateEvent(lastQuote);
        }

        #region IFeedStrategyContext

        IFixtureContext IFeedFixtureContext.ExecContext { get { return ExecContext; } }
        IPluginFeedProvider IFeedFixtureContext.Feed { get { return Feed; } }

        void IFeedFixtureContext.Add(IFeedFixture subscriber)
        {
            dispenser.Add(subscriber);
        }

        void IFeedFixtureContext.Remove(IFeedFixture subscriber)
        {
            dispenser.Remove(subscriber);
        }

        #endregion IFeedStrategyContext
    }
}
