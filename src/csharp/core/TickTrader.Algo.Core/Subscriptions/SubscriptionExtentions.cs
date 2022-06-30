using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public static class SubscriptionExtentions
    {
        public static void Modify(this IQuoteSub subscription, IEnumerable<string> symbols, int depth)
        {
            var updates = symbols.Select(s => FeedSubscriptionUpdate.Upsert(s, depth));
            subscription.Modify(updates.ToList());
        }

        public static void Modify(this IQuoteSub subscription, string symbol, int depth)
        {
            var update = FeedSubscriptionUpdate.Upsert(symbol, depth);
            subscription.Modify(ToList(update));
        }

        public static void Modify(this IQuoteSub subscription, FeedSubscriptionUpdate update)
        {
            subscription.Modify(ToList(update));
        }

        public static void Remove(this IQuoteSub subscription, string symbol)
        {
            var update = FeedSubscriptionUpdate.Remove(symbol);
            subscription.Modify(ToList(update));
        }

        private static List<FeedSubscriptionUpdate> ToList(FeedSubscriptionUpdate update) => new List<FeedSubscriptionUpdate> { update };
    }
}
