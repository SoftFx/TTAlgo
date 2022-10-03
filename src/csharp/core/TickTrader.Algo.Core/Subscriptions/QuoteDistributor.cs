using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public class QuoteDistributor : IQuoteSubInternal, IDisposable
    {
        private const string AllSymbolAlias = QuoteSubUpdate.AllSymbolsAlias;

        private readonly ConcurrentDictionary<string, ListenerGroup> _symbolGroups = new ConcurrentDictionary<string, ListenerGroup>();
        private readonly ListenerGroup _allGroup = new ListenerGroup();
        private readonly IQuoteSubManager _manager;
        private readonly ChannelConsumerWrapper<QuoteInfo> _quoteConsumer;


        public QuoteDistributor(IQuoteSubManager manager)
        {
            _manager = manager;
            _manager.Add(this);

            _quoteConsumer = new ChannelConsumerWrapper<QuoteInfo>(DefaultChannelFactory.CreateForOneToOne<QuoteInfo>(), $"{nameof(QuoteDistributor)} loop");
            _quoteConsumer.BatchSize = 10;
            _quoteConsumer.Start(DispatchQuote);
        }


        public void Dispose()
        {
            _quoteConsumer.Dispose();
            _manager.Remove(this);
            _manager.Modify(this, _symbolGroups.Select(p => QuoteSubUpdate.Remove(p.Key)).ToList());
        }


        public void UpdateRate(QuoteInfo quote)
        {
            _quoteConsumer.Add(quote);
        }

        void IQuoteSubInternal.Dispatch(QuoteInfo quote) => UpdateRate(quote);

        public IDisposable AddListener(Action<QuoteInfo> handler) => new Listener(handler, this, AllSymbolAlias, SubscriptionDepth.Ambient);

        public IDisposable AddListener(Action<QuoteInfo> handler, string symbol, int depth = SubscriptionDepth.Ambient) => new Listener(handler, this, symbol, depth);


        private void AddListener(string symbol, Listener listener)
        {
            if (symbol == AllSymbolAlias)
            {
                _allGroup.Add(listener);
                return;
            }

            var group = _symbolGroups.GetOrAdd(symbol, () => new ListenerGroup());
            var oldDepth = group.Depth;
            group.Add(listener);
            var newDepth = group.Depth;
            if (oldDepth != newDepth)
            {
                ModifySourceSub(symbol, newDepth);
            }
        }

        private void RemoveListener(string symbol, Listener listener)
        {
            if (symbol == AllSymbolAlias)
            {
                _allGroup.Remove(listener);
                return;
            }

            if (_symbolGroups.TryGetValue(symbol, out var group))
            {
                var oldDepth = group.Depth;
                group.Remove(listener);
                var newDepth = group.Depth;
                if (oldDepth != newDepth)
                {
                    ModifySourceSub(symbol, newDepth);
                }
            }
        }

        private void DispatchQuote(QuoteInfo quote)
        {
            if (_symbolGroups.TryGetValue(quote.Symbol, out var group))
            {
                group.DispathQuote(quote);
            }
        }

        private void ModifySourceSub(string symbol, int depth)
        {
            var update = depth != SubscriptionDepth.Ambient
                ? QuoteSubUpdate.Upsert(symbol, depth)
                : QuoteSubUpdate.Remove(symbol);
            _manager.Modify(this, update);
        }


        private class Listener : IDisposable
        {
            private readonly Action<QuoteInfo> _handler;
            private readonly QuoteDistributor _parent;
            private readonly string _symbol;


            public int Depth { get; }


            public Listener(Action<QuoteInfo> handler, QuoteDistributor parent, string symbol, int depth)
            {
                _handler = handler;
                _parent = parent;
                _symbol = symbol;
                Depth = depth;

                _parent.AddListener(_symbol, this);
            }

            public void Dispose()
            {
                _parent.RemoveListener(_symbol, this);
            }

            public void OnNewQuote(QuoteInfo quote)
            {
                _handler?.Invoke(quote);
            }


            //private QuoteInfo TruncateQuote(QuoteInfo quote)
            //{
            //    var depth = Depth;
            //    if (depth == SubscriptionDepth.MaxDepth)
            //    {
            //        return quote;
            //    }
            //    depth = depth < 1 ? 1 : depth;
            //    return quote.Truncate(depth);
            //}
        }

        private class ListenerGroup
        {
            private readonly SubList<Listener> _subList = new SubList<Listener>();


            public int Depth { get; set; } = SubscriptionDepth.Ambient;


            public void Add(Listener listener)
            {
                _subList.AddSub(listener);
                Depth = GetMaxDepth();
            }

            public void Remove(Listener listener)
            {
                _subList.RemoveSub(listener);
                Depth = GetMaxDepth();
            }

            public int GetMaxDepth()
            {
                var max = SubscriptionDepth.Ambient;

                var sublist = _subList.Items;
                for (var i = 0; i < sublist.Length; i++)
                {
                    var depth = sublist[i].Depth;
                    if (max < depth)
                        max = depth;
                }

                return max;
            }

            public void DispathQuote(QuoteInfo quote)
            {
                var sublist = _subList.Items;
                for (var i = 0; i < sublist.Length; i++)
                {
                    sublist[i].OnNewQuote(quote);
                }
            }
        }
    }
}
