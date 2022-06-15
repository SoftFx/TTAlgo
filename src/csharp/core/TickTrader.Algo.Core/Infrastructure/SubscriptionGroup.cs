using System;
using System.Collections.Concurrent;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class SubscriptionGroup
    {
        private readonly ChannelEventSource<QuoteInfo> _quoteSrc = new ChannelEventSource<QuoteInfo>();

        public int Depth { get; internal set; } = SubscriptionDepth.Ambient;
        public ConcurrentDictionary<Subscription, int> Subscriptions { get; } = new ConcurrentDictionary<Subscription, int>();
        public string Symbol { get; private set; }
        public QuoteInfo LastQuote { get; private set; }

        public SubscriptionGroup(string symbol)
        {
            this.Symbol = symbol;
            Subscriptions = new ConcurrentDictionary<Subscription, int>();
        }

        public void UpdateRate(QuoteInfo tick)
        {
            LastQuote = tick;

            _quoteSrc.Send(tick);

            foreach (Subscription subscription in Subscriptions.Keys)
                subscription.OnNewQuote(tick);
        }

        public int GetMaxDepth()
        {
            int max = SubscriptionDepth.Ambient;

            foreach (var value in Subscriptions.Values)
            {
                if (value > max)
                    max = value;
            }

            return max;
        }

        public IDisposable AddListener(Action<QuoteInfo> handler) => _quoteSrc.Subscribe(handler);

        internal bool Upsert(Subscription subscription, int depth)
        {
            var added = !Subscriptions.ContainsKey(subscription);
            Subscriptions[subscription] = depth;
            return added;
        }

        internal void Remove(Subscription subscription)
        {
            Subscriptions.TryRemove(subscription, out _);
        }
    }
}
