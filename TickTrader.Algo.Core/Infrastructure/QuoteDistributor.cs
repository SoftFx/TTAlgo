using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class QuoteDistributor : SubscriptionManagerBase
    {
        public IFeedSubscription AddSubscription(Action<QuoteEntity> handler)
        {
            return new Subscription(handler, this);
        }

        public IFeedSubscription AddSubscription(Action<QuoteEntity> handler, IEnumerable<string> symbols, int depth = 1)
        {
            var sub = new Subscription(handler, this);
            sub.AddOrModify(symbols, depth);
            return sub;
        }

        public IFeedSubscription AddSubscription(Action<QuoteEntity> handler, string symbol, int depth = 1)
        {
            var subscription = new Subscription(handler, this);
            subscription.AddOrModify(symbol, depth);
            return subscription;
        }

        public virtual void UpdateRate(QuoteEntity tick)
        {
            SubscriptionGroup group = GetGroupOrDefault(tick.Symbol);

            if (group != null)
            {
                foreach (Subscription subscription in group.Subscriptions.Keys)
                    subscription.OnNewQuote(tick);
            }
        }

        private new class Subscription : SubscriptionManagerBase.Subscription
        {
            private Action<QuoteEntity> _handler;

            public Subscription(Action<QuoteEntity> handler, SubscriptionManagerBase parent)
                : base(parent)
            {
                _parent = parent;
                _handler = handler ?? throw new ArgumentNullException("handler");
            }

            public void OnNewQuote(QuoteEntity newQuote)
            {
                _handler.Invoke(TruncateQuote(newQuote));
            }

            protected QuoteEntity TruncateQuote(QuoteEntity quote)
            {
                if (bySymbol.TryGetValue(quote.Symbol, out var depth) && depth == 0)
                {
                    return quote;
                }
                depth = depth < 1 ? 1 : depth;
                return new QuoteEntity(quote.Symbol, quote.CreatingTime, quote.BidList.Take(depth).ToArray(), quote.AskList.Take(depth).ToArray());
            }
        }
    }
}
