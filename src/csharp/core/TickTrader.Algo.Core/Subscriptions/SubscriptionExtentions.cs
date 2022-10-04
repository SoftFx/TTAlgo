using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public static class SubscriptionExtentions
    {
        public static void Modify(this IQuoteSub subscription, IEnumerable<string> symbols, int depth)
        {
            var updates = symbols.Select(s => QuoteSubUpdate.Upsert(s, depth));
            subscription.Modify(updates.ToList());
        }

        public static void Modify(this IQuoteSub subscription, string symbol, int depth)
        {
            var update = QuoteSubUpdate.Upsert(symbol, depth);
            subscription.Modify(update);
        }
    }
}
