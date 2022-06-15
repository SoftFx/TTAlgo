using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
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

        private ChannelConsumerWrapper<QuoteInfo> _quoteConsumer;
        private SubList<HandlerWrapper> _handlers;


        public QuoteSubscription(IQuoteSubManager manager)
        {
            _manager = manager;

            manager.Add(this);
        }


        public void Dispose()
        {
            _quoteConsumer?.Dispose();
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
            if (_handlers == null)
            {
                _handlers = new SubList<HandlerWrapper>();
                _quoteConsumer = new ChannelConsumerWrapper<QuoteInfo>(DefaultChannelFactory.CreateForOneToOne<QuoteInfo>(), $"{nameof(QuoteSubscription)} loop");
                _quoteConsumer.BatchSize = 10;
                _quoteConsumer.Start(DispatchQuote);
            }

            return new HandlerWrapper(handler, this);
        }


        void IQuoteSubInternal.Dispatch(QuoteInfo quote) => _quoteConsumer?.Add(quote);


        private void DispatchQuote(QuoteInfo quote)
        {
            quote = TruncateQuote(quote);
            var sublist = _handlers.Items;
            for (var i = 0; i < sublist.Length; i++)
            {
                sublist[i].OnNewQuote(quote);
            }
        }

        private QuoteInfo TruncateQuote(QuoteInfo quote)
        {
            if (_bySymbol.TryGetValue(quote.Symbol, out var depth) && depth == SubscriptionDepth.MaxDepth)
            {
                return quote;
            }
            depth = depth < 1 ? 1 : depth;
            return quote.Truncate(depth);
        }


        private class HandlerWrapper : IDisposable
        {
            private readonly Action<QuoteInfo> _handler;
            private readonly QuoteSubscription _parent;


            public HandlerWrapper(Action<QuoteInfo> handler, QuoteSubscription parent)
            {
                _handler = handler;
                _parent = parent;

                _parent._handlers.AddSub(this);
            }


            public void Dispose()
            {
                _parent._handlers.RemoveSub(this);
            }


            public void OnNewQuote(QuoteInfo quote)
            {
                _handler?.Invoke(quote);
            }
        }
    }
}
