using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public interface IQuoteSub : IDisposable
    {
        void Modify(List<FeedSubscriptionUpdate> updates);

        IDisposable AddHandler(Action<QuoteInfo> handler);
    }


    public class QuoteSubscription : IQuoteSub, IQuoteSubInternal
    {
        private readonly ConcurrentDictionary<string, int> _bySymbol = new ConcurrentDictionary<string, int>();
        private readonly IQuoteSubManager _manager;

        private ChannelEventSource<QuoteInfo> _quoteSrc;


        public QuoteSubscription(IQuoteSubManager manager)
        {
            _manager = manager;

            manager.Add(this);
        }


        public void Dispose()
        {
            _quoteSrc?.Dispose();
            _manager.Remove(this);
            _manager.Modify(this, _bySymbol.Select(p => FeedSubscriptionUpdate.Remove(p.Key)).ToList());
        }


        public void Modify(List<FeedSubscriptionUpdate> updates)
        {
            var validUpdates = new List<FeedSubscriptionUpdate>(updates.Count);

            foreach (var update in updates)
            {
                if (update.IsUpsertAction)
                {
                    validUpdates.Add(update);
                    _bySymbol.AddOrUpdate(update.Symbol, update.Depth, (key, value) => update.Depth);
                }
                else if (update.IsRemoveAction)
                {
                    if (_bySymbol.TryRemove(update.Symbol, out var _))
                        validUpdates.Add(update);
                }
            }

            _manager.Modify(this, validUpdates);
        }

        public IDisposable AddHandler(Action<QuoteInfo> handler)
        {
            if (_quoteSrc == null)
                _quoteSrc = new ChannelEventSource<QuoteInfo>();

            return _quoteSrc.Subscribe(handler);
        }


        void IQuoteSubInternal.Dispatch(QuoteInfo quote) => _quoteSrc?.Send(TruncateQuote(quote));


        private QuoteInfo TruncateQuote(QuoteInfo quote)
        {
            if (_bySymbol.TryGetValue(quote.Symbol, out var depth) && depth == SubscriptionDepth.MaxDepth)
            {
                return quote;
            }
            depth = depth < 1 ? 1 : depth;
            return quote.Truncate(depth);
        }
    }
}
