using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class Subscription : IFeedSubscription
    {
        private readonly ConcurrentDictionary<string, int> _bySymbol = new ConcurrentDictionary<string, int>();
        private readonly QuoteDistributor _parent;
        private readonly Action<QuoteInfo> _handler;

        public Subscription(Action<QuoteInfo> handler, QuoteDistributor parent)
        {
            _parent = parent;
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        public void OnNewQuote(QuoteInfo newQuote)
        {
            _handler.Invoke(TruncateQuote(newQuote));
        }

        protected QuoteInfo TruncateQuote(QuoteInfo quote)
        {
            if (_bySymbol.TryGetValue(quote.Symbol, out var depth) && depth == 0)
            {
                return quote;
            }
            depth = depth < 1 ? 1 : depth;
            return quote.Truncate(depth);
        }

        public List<QuoteInfo> Modify(List<FeedSubscriptionUpdate> updates)
        {
            var instantSnaphsot = new List<QuoteInfo>();
            var changedSymbols = new List<string>();

            foreach (var update in updates)
            {
                if (Modify(update, instantSnaphsot))
                    changedSymbols.Add(update.Symbol);
            }

            if (changedSymbols.Count > 0)
            {
                var parentSnapshot = _parent.AdjustGroupSubscription(changedSymbols);
                if (parentSnapshot != null)
                    instantSnaphsot.AddRange(parentSnapshot);
            }

            return instantSnaphsot;
        }

        public Task<List<QuoteInfo>> ModifyAsync(List<FeedSubscriptionUpdate> updates)
        {
            return Task.FromResult(Modify(updates));
        }

        private bool Modify(FeedSubscriptionUpdate update, List<QuoteInfo> snapshot)
        {
            if (update.IsUpsertAction)
            {
                var group = _parent.GetOrAddGroup(update.Symbol);
                var isNewSub = group.Upsert(this, update.Depth);
                if (isNewSub && group.LastQuote != null)
                    snapshot.Add(group.LastQuote);
                //_parent.AdjustGroupSubscription(symbol);
                _bySymbol.AddOrUpdate(update.Symbol, update.Depth, (key, value) => update.Depth);
                return true;
            }
            else if (update.IsRemoveAction)
            {
                if (_bySymbol.TryRemove(update.Symbol, out _))
                {
                    var group = _parent.GetGroupOrDefault(update.Symbol);
                    if (group != null)
                    {
                        group.Subscriptions.TryRemove(this, out _);
                        //_parent.AdjustGroupSubscription(symbol);
                        return true;
                    }
                }
            }

            return false;
        }

        public void CancelAll()
        {
            //if (IsSubscribedForAll)
            //{
            //    _parent._allSymbolSubscriptions.Remove(this);
            //    _parent.AdjustAllSymbolsSubscription();
            //}

            var symbols = _bySymbol.Keys.ToList();
            _bySymbol.Clear();

            foreach (var symbol in symbols)
            {
                var group = _parent.GetGroupOrDefault(symbol);
                if (group != null)
                    group.Subscriptions.TryRemove(this, out _);
            }

            if (symbols.Count > 0)
                _parent.AdjustGroupSubscription(symbols);
        }

        public Task CancelAllAsync()
        {
            CancelAll();

            return Task.CompletedTask;
        }
    }
}
