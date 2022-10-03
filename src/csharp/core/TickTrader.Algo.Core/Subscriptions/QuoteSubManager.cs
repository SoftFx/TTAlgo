using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public interface IQuoteSubManager
    {
        void Add(IQuoteSubInternal sub);

        void Remove(IQuoteSubInternal sub);

        void Modify(IQuoteSubInternal sub, QuoteSubUpdate update);

        void Modify(IQuoteSubInternal sub, List<QuoteSubUpdate> updates);
    }

    public interface IQuoteSubInternal
    {
        void Dispatch(QuoteInfo quote);
    }

    public interface IQuoteSubProvider
    {
        void Modify(List<QuoteSubUpdate> updates);
    }


    public class QuoteSubManager : IQuoteSubManager
    {
        private readonly ConcurrentDictionary<string, SubGroup> _groups = new ConcurrentDictionary<string, SubGroup>();
        private readonly SubGroup _allSymbolsGroup = new SubGroup(QuoteSubUpdate.AllSymbolsAlias);
        private readonly SubList<IQuoteSubInternal> _subList = new SubList<IQuoteSubInternal>();
        private readonly IQuoteSubProvider _provider;

        private Dictionary<string, int> _unwrappedSymbolsDepth; // subscription depth by each symbol on provider if enabled


        public QuoteSubManager(IQuoteSubProvider provider)
        {
            _provider = provider;
        }


        public void Dispatch(QuoteInfo quote)
        {
            var subList = _subList.Items;
            var n = subList.Length;
            for (var i = 0; i < n; i++)
            {
                subList[i].Dispatch(quote);
            }
        }

        public void Add(IQuoteSubInternal sub) => _subList.AddSub(sub);

        public void Remove(IQuoteSubInternal sub) => _subList.RemoveSub(sub);

        public List<QuoteSubUpdate> InitUnwrap(IEnumerable<string> allSymbols)
        {
            _unwrappedSymbolsDepth = allSymbols.ToDictionary(k => k, v => SubscriptionDepth.Ambient);

            lock (_unwrappedSymbolsDepth)
            {
                var updates = new List<QuoteSubUpdate>(_unwrappedSymbolsDepth.Count);
                var minDepth = _allSymbolsGroup.Depth;
                foreach (var symbol in allSymbols)
                {
                    var depth = GetUnwrappedSymbolDepth(symbol, minDepth);
                    _unwrappedSymbolsDepth[symbol] = depth;
                    updates.Add(QuoteSubUpdate.Upsert(symbol, depth));
                }
                return updates;
            }
        }

        public void Modify(IQuoteSubInternal sub, QuoteSubUpdate update)
        {
            if (_unwrappedSymbolsDepth != null)
            {
                Modify(sub, new List<QuoteSubUpdate> { update });
            }
            else
            {
                var groupUpdate = ModifyGroup(sub, update);
                if (groupUpdate != null)
                    _provider.Modify(new List<QuoteSubUpdate> { groupUpdate });
            }
        }

        public void Modify(IQuoteSubInternal sub, List<QuoteSubUpdate> updates)
        {
            var allChanged = false;
            var groupUpdates = new List<QuoteSubUpdate>();
            foreach (var update in updates)
            {
                var groupUpdate = ModifyGroup(sub, update);
                if (groupUpdate != null)
                {
                    groupUpdates.Add(groupUpdate);
                    if (update.IsAllSymbols)
                        allChanged = true;
                }
            }

            if (groupUpdates.Count == 0)
                return;

            if (_unwrappedSymbolsDepth != null)
            {
                lock (_unwrappedSymbolsDepth)
                {
                    var minDepth = _allSymbolsGroup.Depth;
                    if (allChanged)
                    {
                        groupUpdates.Clear();
                        foreach (var smbDepth in _unwrappedSymbolsDepth)
                        {
                            var symbol = smbDepth.Key;
                            var depth = GetUnwrappedSymbolDepth(symbol, minDepth);
                            if (smbDepth.Value < depth)
                            {
                                _unwrappedSymbolsDepth[symbol] = depth;
                                groupUpdates.Add(QuoteSubUpdate.Upsert(symbol, depth));
                            }
                        }
                    }
                    else
                    {
                        // update depth if all symbol depth is higher
                        foreach (var update in groupUpdates)
                        {
                            update.Depth = Math.Max(update.Depth, minDepth);
                            _unwrappedSymbolsDepth[update.Symbol] = update.Depth;
                        }
                    }
                }
            }

            if (groupUpdates.Count > 0)
                _provider.Modify(groupUpdates);
        }


        private QuoteSubUpdate ModifyGroup(IQuoteSubInternal sub, QuoteSubUpdate update)
        {
            var symbol = update.Symbol;
            SubGroup group = null;

            if (update.IsUpsertAction)
            {
                group = GetOrAddGroup(symbol);
                group.Upsert(sub, update.Depth);
            }
            else if (update.IsRemoveAction)
            {
                group = GetGroupOrDefault(symbol);
                group?.Remove(sub);
            }

            if (group != null)
            {
                var oldDepth = group.Depth;
                var newDepth = group.GetMaxDepth();
                if (oldDepth != newDepth)
                {
                    group.Depth = newDepth;
                    return newDepth > SubscriptionDepth.Ambient
                        ? QuoteSubUpdate.Upsert(symbol, newDepth)
                        : QuoteSubUpdate.Remove(symbol);
                }
            }

            return null;
        }

        private SubGroup GetOrAddGroup(string symbol)
        {
            if (symbol == QuoteSubUpdate.AllSymbolsAlias)
                return _allSymbolsGroup;

            return _groups.GetOrAdd(symbol, s => new SubGroup(s));
        }

        private SubGroup GetGroupOrDefault(string symbol)
        {
            if (symbol == QuoteSubUpdate.AllSymbolsAlias)
                return _allSymbolsGroup;

            _groups.TryGetValue(symbol, out var group);
            return group;
        }

        private int GetUnwrappedSymbolDepth(string symbol, int minDepth)
        {
            var group = GetGroupOrDefault(symbol);
            if (group == null)
                return minDepth;

            return Math.Max(minDepth, group.Depth);
        }


        private class SubGroup
        {
            private readonly ConcurrentDictionary<IQuoteSubInternal, int> _subs = new ConcurrentDictionary<IQuoteSubInternal, int>();


            public string Symbol { get; }

            public int Depth { get; set; } = SubscriptionDepth.Ambient;


            public SubGroup(string symbol)
            {
                Symbol = symbol;
            }


            public bool Upsert(IQuoteSubInternal sub, int depth)
            {
                var added = !_subs.ContainsKey(sub);
                _subs[sub] = depth;
                return added;
            }

            public void Remove(IQuoteSubInternal sub)
            {
                _subs.TryRemove(sub, out _);
            }

            public int GetMaxDepth()
            {
                int max = SubscriptionDepth.Ambient;

                foreach (var value in _subs.Values)
                {
                    if (value > max)
                        max = value;
                }

                return max;
            }
        }
    }
}
