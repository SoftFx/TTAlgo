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
        internal abstract void Stop();

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
            OnInit();
        }

        internal virtual void Start()
        {
            Feed.Sync.Invoke(StartStrategy);
        }

        private void StartStrategy()
        {
            ExecContext.Enqueue(b => BatchBuild(BufferSize));
            Feed.FeedUpdated += Feed_FeedUpdated;
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
            var result = UpdateBuffers(update);

            if (result.IsLastUpdated)
                ExecContext.Builder.InvokeCalculate(true);

            for (int i = 0; i < result.ExtendedBy; i++)
            {
                ExecContext.Builder.IncreaseVirtualPosition();
                ExecContext.Builder.InvokeCalculate(false);
            }

            dispenser.OnUpdateEvent(update.LastQuotes[0]);
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
