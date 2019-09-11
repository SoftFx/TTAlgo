using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class Subscription : IFeedSubscription
    {
        private readonly QuoteDistributor _parent;
        private readonly Action<QuoteEntity> _handler;
        private Dictionary<string, int> bySymbol = new Dictionary<string, int>();

        public Subscription(Action<QuoteEntity> handler, QuoteDistributor parent)
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

        public List<QuoteEntity> Modify(List<FeedSubscriptionUpdate> updates)
        {
            var instantSnaphsot = new List<QuoteEntity>();
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

        private bool Modify(FeedSubscriptionUpdate update, List<QuoteEntity> snapshot)
        {
            if (update.IsUpsertAction)
            {
                var group = _parent.GetOrAddGroup(update.Symbol);
                var isNewSub = group.Upsert(this, update.Depth);
                if (isNewSub && group.LastQuote != null)
                    snapshot.Add(group.LastQuote);
                //_parent.AdjustGroupSubscription(symbol);
                bySymbol[update.Symbol] = update.Depth;
                return true;
            }
            else if (update.IsRemoveAction)
            {
                if (bySymbol.Remove(update.Symbol))
                {
                    var group = _parent.GetGroupOrDefault(update.Symbol);
                    if (group != null)
                    {
                        group.Subscriptions.Remove(this);
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

            var symbols = bySymbol.Keys.ToList();
            bySymbol.Clear();

            foreach (var symbol in symbols)
            {
                var group = _parent.GetGroupOrDefault(symbol);
                if (group != null)
                    group.Subscriptions.Remove(this);
            }

            if (symbols.Count > 0)
                _parent.AdjustGroupSubscription(symbols);
        }
    }
}
