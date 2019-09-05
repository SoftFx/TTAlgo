using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Infrastructure
{
    public class SubscriptionManagerBase
    {
        //private List<Subscription> _allSymbolSubscriptions = new List<Subscription>();
        private Dictionary<string, SubscriptionGroup> groups = new Dictionary<string, SubscriptionGroup>();
        private IFeedSubscription _src;
        //private bool _isSubscribedForAll;
        private HashSet<string> _availableSymbols;

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
                var group = groups.GetOrDefault(smb);
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

        private void AdjustGroupSubscription(IEnumerable<string> symbols)
        {
            if (!IsStarted)
                return;

            var updates = new List<FeedSubscriptionUpdate>();

            foreach (var symbol in symbols)
            {
                if (GetUpdate(symbol, out var update))
                    updates.Add(update);
            }

            if (updates.Count > 0)
                ModifySourceSubscription(updates);
        }

        protected virtual void ModifySourceSubscription(List<FeedSubscriptionUpdate> updates)
        {
            _src.Modify(updates);
        }

        //private void AdjustGroupSubscription(string symbol)
        //{
        //    AdjustGroupSubscription(symbol.Yield());
        //}

        private bool GetUpdate(string symbol, out FeedSubscriptionUpdate update)
        {
            if (_availableSymbols == null || _availableSymbols.Contains(symbol))
            {
                var group = groups.GetOrDefault(symbol);
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

        protected SubscriptionGroup GetOrAddGroup(string symbol)
        {
            SubscriptionGroup group;
            if (!groups.TryGetValue(symbol, out group))
            {
                group = new SubscriptionGroup(symbol);
                groups.Add(symbol, group);
            }
            return group;
        }

        protected SubscriptionGroup GetGroupOrDefault(string symbol)
        {
            groups.TryGetValue(symbol, out var group);
            return group;
        }

        protected class Subscription : IFeedSubscription
        {
            protected SubscriptionManagerBase _parent;
            protected Dictionary<string, int> bySymbol = new Dictionary<string, int>();

            //public bool IsSubscribedForAll { get; private set; }

            public Subscription(SubscriptionManagerBase parent)
            {
                _parent = parent;
            }

            //public void Modify(FeedSubscriptionUpdate update)
            //{
            //    if (update.IsUpsertAction)
            //        AddModifier(update.Symbol, update.Depth);
            //    else if (update.IsRemoveAction)
            //        RemoveModifier(update.Symbol);
            //}

            public void Modify(List<FeedSubscriptionUpdate> updates)
            {
                var changedSymbols = new List<string>();

                foreach (var update in updates)
                {
                    if (Modify(update))
                        changedSymbols.Add(update.Symbol);
                }

                if (changedSymbols.Count > 0)
                    _parent.AdjustGroupSubscription(changedSymbols);
            }

            //public void SubscribeForAll()
            //{
            //    if (!IsSubscribedForAll)
            //    {
            //        _parent._allSymbolSubscriptions.Add(this);
            //        _parent.AdjustAllSymbolsSubscription();
            //        IsSubscribedForAll = true;
            //    }
            //}

            private bool Modify(FeedSubscriptionUpdate update)
            {
                if (update.IsUpsertAction)
                {
                    var group = _parent.GetOrAddGroup(update.Symbol);
                    group.Upsert(this, update.Depth);
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

        protected class SubscriptionGroup
        {
            public int Depth { get; set; } = -1;
            public Dictionary<Subscription, int> Subscriptions { get; private set; }
            public string Symbol { get; private set; }

            public SubscriptionGroup(string symbol)
            {
                this.Symbol = symbol;
                Subscriptions = new Dictionary<Subscription, int>();
            }

            public int GetMaxDepth()
            {
                int max = 1;

                foreach (var value in Subscriptions.Values)
                {
                    if (value == 0)
                        return 0;
                    if (value > max)
                        max = value;
                }

                return max;
            }

            public void Upsert(Subscription subscription, int depth)
            {
                Subscriptions[subscription] = depth;
            }

            public void Remove(Subscription subscription)
            {
                Subscriptions.Remove(subscription);
            }
        }
    }
}
