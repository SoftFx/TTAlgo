using System.Collections.Concurrent;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public interface IBarSubManager
    {
        void Add(IBarSubInternal sub);

        void Remove(IBarSubInternal sub);

        void Modify(IBarSubInternal sub, List<BarSubUpdate> updates);
    }

    public interface IBarSubInternal
    {
        void Dispatch(BarInfo bar);
    }

    public interface IBarSubProvider
    {
        void Modify(List<BarSubUpdate> updates);
    }


    public class BarSubManager : IBarSubManager
    {
        private readonly ConcurrentDictionary<string, SymbolGroup> _groups = new ConcurrentDictionary<string, SymbolGroup>();
        private readonly SubList<IBarSubInternal> _subList = new SubList<IBarSubInternal>();
        private readonly IBarSubProvider _provider;


        public BarSubManager(IBarSubProvider provider)
        {
            _provider = provider;
        }


        public void Dispatch(BarInfo bar)
        {
            var subList = _subList.Items;
            var n = subList.Length;
            for (var i = 0; i < n; i++)
            {
                subList[i].Dispatch(bar);
            }
        }


        public void Add(IBarSubInternal sub) => _subList.AddSub(sub);

        public void Remove(IBarSubInternal sub) => _subList.RemoveSub(sub);

        public void Modify(IBarSubInternal sub, List<BarSubUpdate> updates)
        {
        }


        private BarSubUpdate ModifyGroup(IBarSubInternal sub, BarSubUpdate update)
        {
            return update;

        }

        private SymbolGroup GetOrAddGroup(string symbol)
        {
            return _groups.GetOrAdd(symbol, s => new SymbolGroup(s));
        }

        private SymbolGroup GetGroupOrDefault(string symbol)
        {
            _groups.TryGetValue(symbol, out var group);
            return group;
        }


        private class SymbolGroup
        {
            private readonly object _syncObj = new object();
            private readonly Dictionary<BarSubEntry, SubGroup> _groups = new Dictionary<BarSubEntry, SubGroup>();


            public string Symbol { get; set; }


            public SymbolGroup(string symbol)
            {
                Symbol = symbol;
            }


            public bool Upsert(IBarSubInternal sub, BarSubEntry entry)
            {
                lock (_syncObj)
                {
                    var added = false;
                    if (!_groups.TryGetValue(entry, out var group))
                    {
                        group = new SubGroup(entry.Timeframe, entry.MarketSide);
                        _groups.Add(entry, group);
                        added = true;
                    }
                    group.Upsert(sub);
                    return added;
                }
            }

            public void Remove(IBarSubInternal sub, BarSubEntry entry)
            {
                lock (_syncObj)
                {
                    if (_groups.TryGetValue(entry, out var group))
                    {
                        group.Remove(sub);
                        if (group.IsEmpty)
                            _groups.Remove(entry);
                    }
                }
            }
        }


        private class SubGroup
        {
            private readonly Dictionary<IBarSubInternal, bool> _subs = new Dictionary<IBarSubInternal, bool>();


            public Feed.Types.Timeframe Timeframe { get; set; }

            public Feed.Types.MarketSide Side { get; set; }

            public bool IsEmpty => _subs.Count == 0;


            public SubGroup(Feed.Types.Timeframe timeframe, Feed.Types.MarketSide side)
            {
                Timeframe = timeframe;
                Side = side;
            }


            public bool Upsert(IBarSubInternal sub)
            {
                var added = !_subs.ContainsKey(sub);
                _subs[sub] = false;
                return added;
            }

            public void Remove(IBarSubInternal sub)
            {
                _subs.Remove(sub);
            }
        }
    }
}
