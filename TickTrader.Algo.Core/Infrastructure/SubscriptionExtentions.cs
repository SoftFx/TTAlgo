using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Infrastructure
{
    public static class SubscriptionExtentions
    {
        public static List<QuoteEntity> AddOrModify(this IFeedSubscription subscription, IEnumerable<string> symbols, int depth = 1)
        {
            var updates = symbols.Select(s => FeedSubscriptionUpdate.Upsert(s, depth));
            return subscription.Modify(updates.ToList());
        }

        public static QuoteEntity AddOrModify(this IFeedSubscription subscription, string symbol, int depth)
        {
            var update = FeedSubscriptionUpdate.Upsert(symbol, depth);
            var snapshot = subscription.Modify(ToList(update));
            return snapshot?.FirstOrDefault();
        }

        public static void Remove(this IFeedSubscription subscription, string symbol)
        {
            var update = FeedSubscriptionUpdate.Remove(symbol);
            subscription.Modify(ToList(update));
        }

        private static List<FeedSubscriptionUpdate> ToList(FeedSubscriptionUpdate update)
        {
            var list = new List<FeedSubscriptionUpdate>();
            list.Add(update);
            return list;
        }
    }
}
