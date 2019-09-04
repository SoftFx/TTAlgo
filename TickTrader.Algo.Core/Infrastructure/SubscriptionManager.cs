using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class SubscriptionManager : SubscriptionManagerBase
    {
        public IFeedSubscription AddSubscription()
        {
            return new Subscription(this);
        }

        public IFeedSubscription AddSubscription(IEnumerable<string> symbols, int depth = 1)
        {
            var sub = new Subscription(this);
            sub.AddOrModify(symbols, depth);
            return sub;
        }

        public IFeedSubscription AddSubscription(string symbol, int depth = 1)
        {
            var subscription = new Subscription(this);
            subscription.AddOrModify(symbol, depth);
            return subscription;
        }

    }
}
