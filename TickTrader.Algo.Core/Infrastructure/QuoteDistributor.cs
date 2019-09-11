﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Infrastructure
{
    public partial class QuoteDistributor
    {
        private readonly Dictionary<string, QuoteEntity> _lastQuotes = new Dictionary<string, QuoteEntity>();
        private readonly Dictionary<string, SubscriptionGroup> groups = new Dictionary<string, SubscriptionGroup>();
        private IFeedSubscription _src;
        //private bool _isSubscribedForAll;
        private HashSet<string> _availableSymbols;

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
            GetGroupOrDefault(tick.Symbol)?.UpdateRate(tick);
        }

        public bool IsStarted => _src != null;
        //protected IEnumerable<Subscription> GlobalSubscriptions => _allSymbolSubscriptions;

        public virtual void Start(IFeedSubscription src, IEnumerable<string> avaialbleSymbols = null, bool subscribeOnStart = true)
        {
            _availableSymbols = avaialbleSymbols?.ToSet();

            _src = src;

            //if (_isSubscribedForAll)
            //    _src.SubscribeForAll();

            //ResetAllSubscriptions();

            if (subscribeOnStart)
            {
                var updates = new List<FeedSubscriptionUpdate>();

                foreach (var group in groups.Values)
                {
                    var depth = group.GetMaxDepth();
                    updates.Add(new FeedSubscriptionUpdate(group.Symbol, depth));
                    group.Depth = depth;
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
                    yield return new Tuple<int, string>(1, smb);
                else
                    yield return new Tuple<int, string>(group.Depth, smb);
            }
        }

        //private void AdjustAllSymbolsSubscription()
        //{
        //    if (!IsStarted)
        //        return;

        //    if (_allSymbolSubscriptions.Count > 0)
        //    {
        //        if (!_isSubscribedForAll)
        //        {
        //            _src.SubscribeForAll();
        //            _isSubscribedForAll = true;
        //        }
        //    }
        //}

        //private void ResetAllSubscriptions()
        //{
        //    foreach (var group in groups.Values)
        //        group.Depth = -1;
        //}

        internal List<QuoteEntity> AdjustGroupSubscription(IEnumerable<string> symbols)
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

        protected virtual List<QuoteEntity> ModifySourceSubscription(List<FeedSubscriptionUpdate> updates)
        {
            return _src.Modify(updates);
        }

        private bool GetUpdate(string symbol, out FeedSubscriptionUpdate update)
        {
            if (_availableSymbols == null || _availableSymbols.Contains(symbol))
            {
                var group = GetOrAddGroup(symbol);
                if (group != null)
                {
                    if (group.Subscriptions.Count == 0)
                    {
                        if (group.Depth >= 0)
                        {
                            //_src.Remove(group.Symbol);
                            update = FeedSubscriptionUpdate.Remove(symbol);
                            group.Depth = -1;
                            return true;
                        }
                    }
                    else
                    {
                        var oldDepth = group.Depth;
                        var newDepth = group.GetMaxDepth();
                        if (newDepth != oldDepth)
                        {
                            group.Depth = newDepth;
                            //_src.AddOrModify(symbol, newDepth);
                            update = FeedSubscriptionUpdate.Upsert(symbol, newDepth);
                            return true;
                        }
                    }
                }
            }

            update = default(FeedSubscriptionUpdate);
            return false;
        }

        internal virtual SubscriptionGroup GetOrAddGroup(string symbol)
        {
            SubscriptionGroup group;
            if (!groups.TryGetValue(symbol, out group))
            {
                group = new SubscriptionGroup(symbol);
                groups.Add(symbol, group);
            }
            return group;
        }

        internal virtual SubscriptionGroup GetGroupOrDefault(string symbol)
        {
            groups.TryGetValue(symbol, out var group);
            return group;
        }
    }
}
