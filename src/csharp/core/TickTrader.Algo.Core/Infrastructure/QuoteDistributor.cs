using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class QuoteDistributor
    {
        private readonly ConcurrentDictionary<string, SubscriptionGroup> _groups = new ConcurrentDictionary<string, SubscriptionGroup>();
        private readonly ChannelEventSource<QuoteInfo> _globalQuoteSrc = new ChannelEventSource<QuoteInfo>();
        private readonly SubscriptionGroup _allSymbolsGroup = new SubscriptionGroup(FeedSubscriptionUpdate.AllSymbolsAlias);
        private IFeedSubscription _src;
        private ConcurrentDictionary<string, bool> _availableSymbols;


        public IFeedSubscription AddSubscription()
        {
            return new Subscription(this);
        }

        public IFeedSubscription AddSubscription(string symbol, int depth = 1)
        {
            var subscription = new Subscription(this);
            subscription.Modify(new List<FeedSubscriptionUpdate> { FeedSubscriptionUpdate.Upsert(symbol, depth) });
            return subscription;
        }

        public IDisposable AddListener(Action<QuoteInfo> handler) => _globalQuoteSrc.Subscribe(handler);

        public IDisposable AddListener(Action<QuoteInfo> handler, string symbol) => GetOrAddGroup(symbol).AddListener(handler);

        public virtual void UpdateRate(QuoteInfo tick)
        {
            _globalQuoteSrc.Send(tick);

            _allSymbolsGroup.UpdateRate(tick);

            GetGroupOrDefault(tick.Symbol)?.UpdateRate(tick);
        }

        public bool IsStarted => _src != null;

        public virtual void Start(IFeedSubscription src, IEnumerable<string> avaialbleSymbols = null, bool subscribeOnStart = true)
        {
            if (avaialbleSymbols != null)
                _availableSymbols = new ConcurrentDictionary<string, bool>(avaialbleSymbols?.ToDictionary(key => key, value => true));

            _src = src;

            if (subscribeOnStart)
            {
                var updates = new List<FeedSubscriptionUpdate>();

                lock (_groups)
                {
                    foreach (var group in _groups.Values)
                    {
                        var depth = group.GetMaxDepth();
                        updates.Add(FeedSubscriptionUpdate.Upsert(group.Symbol, depth));
                        group.Depth = depth;
                    }
                }

                _src.Modify(updates);
            }
        }

        public virtual void Stop(bool cancelSubscriptions = true)
        {
            if (IsStarted)
            {
                try
                {
                    if (cancelSubscriptions)
                        _src.CancelAll();
                }
                finally
                {
                    _src = null;
                }
            }
        }

        public IEnumerable<Tuple<int, string>> GetAllSubscriptions(IEnumerable<string> allSymbols)
        {
            foreach (var smb in allSymbols)
            {
                var group = GetGroupOrDefault(smb);
                if (group == null)
                    yield return new Tuple<int, string>(SubscriptionDepth.Ambient, smb);
                else
                    yield return new Tuple<int, string>(group.Depth, smb);
            }
        }

        internal List<QuoteInfo> AdjustGroupSubscription(IEnumerable<string> symbols)
        {
            if (!IsStarted)
                return null;

            var updates = new List<FeedSubscriptionUpdate>();

            foreach (var symbol in symbols)
            {
                if (GetUpdate(symbol, out var update))
                    updates.Add(update);
            }

            if (updates.Count > 0)
                return ModifySourceSubscription(updates);

            return null;
        }

        protected virtual List<QuoteInfo> ModifySourceSubscription(List<FeedSubscriptionUpdate> updates)
        {
            return _src.Modify(updates);
        }

        private bool GetUpdate(string symbol, out FeedSubscriptionUpdate update)
        {
            if (_availableSymbols == null || _availableSymbols.ContainsKey(symbol))
            {
                var group = GetOrAddGroup(symbol);
                if (group != null)
                {
                    var oldDepth = group.Depth;
                    var newDepth = group.GetMaxDepth();
                    if (newDepth != oldDepth)
                    {
                        group.Depth = newDepth;
                        update = newDepth != SubscriptionDepth.Ambient
                            ? FeedSubscriptionUpdate.Upsert(symbol, newDepth)
                            : FeedSubscriptionUpdate.Remove(symbol);
                        return true;
                    }
                }
            }

            update = default(FeedSubscriptionUpdate);
            return false;
        }

        public virtual SubscriptionGroup GetOrAddGroup(string symbol)
        {
            if (symbol == FeedSubscriptionUpdate.AllSymbolsAlias)
                return _allSymbolsGroup;

            return _groups.GetOrAdd(symbol, s => new SubscriptionGroup(s));
        }

        public virtual SubscriptionGroup GetGroupOrDefault(string symbol)
        {
            if (symbol == FeedSubscriptionUpdate.AllSymbolsAlias)
                return _allSymbolsGroup;

            _groups.TryGetValue(symbol, out var group);
            return group;
        }
    }
}
