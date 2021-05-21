using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class SubscriptionGroup
    {
        public int Depth { get; internal set; } = -1;
        public Dictionary<Subscription, int> Subscriptions { get; } = new Dictionary<Subscription, int>();
        public string Symbol { get; private set; }
        public QuoteInfo LastQuote { get; private set; }
        
        public SubscriptionGroup(string symbol)
        {
            this.Symbol = symbol;
            Subscriptions = new Dictionary<Subscription, int>();
        }

        public void UpdateRate(QuoteInfo tick)
        {
            LastQuote = tick;

            foreach (Subscription subscription in Subscriptions.Keys)
                subscription.OnNewQuote(tick);
        }

        public int GetMaxDepth()
        {
            int max = 1;

            foreach (var value in Subscriptions.Values)
            {
                if (value == 0)
                    return 0;
                if (value > max)
                    max = value;
            }

            return max;
        }

        internal bool Upsert(Subscription subscription, int depth)
        {
            var added = !Subscriptions.ContainsKey(subscription);
            Subscriptions[subscription] = depth;
            return added;
        }

        internal void Remove(Subscription subscription)
        {
            Subscriptions.Remove(subscription);
        }
    }
}
