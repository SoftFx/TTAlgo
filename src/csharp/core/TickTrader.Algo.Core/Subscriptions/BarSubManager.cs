using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public interface IBarSubManager
    {
        void Add(IBarSubInternal sub);

        void Remove(IBarSubInternal sub);

        void Modify(IBarSubInternal sub, BarSubUpdate update);

        void Modify(IBarSubInternal sub, List<BarSubUpdate> updates);
    }

    public interface IBarSubInternal
    {
        void Dispatch(BarUpdate bar);
    }

    public interface IBarSubProvider
    {
        void Modify(List<BarSubUpdate> updates);
    }


    public class BarSubManager : IBarSubManager
    {
        private readonly object _syncObj = new object();
        private readonly Dictionary<BarSubEntry, SubGroup> _groups = new Dictionary<BarSubEntry, SubGroup>();
        private readonly SubList<IBarSubInternal> _subList = new SubList<IBarSubInternal>();
        private readonly IBarSubProvider _provider;


        public BarSubManager(IBarSubProvider provider)
        {
            _provider = provider;
        }


        public void Dispatch(BarUpdate bar)
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

        public void Modify(IBarSubInternal sub, BarSubUpdate update)
        {
            var propagate = false;

            lock (_syncObj)
            {
                propagate = ModifyGroup(sub, update);
                if (propagate && update.IsRemoveAction)
                    _groups.Remove(update.Entry); // cleanup empty group
            }

            if (propagate)
                _provider.Modify(new List<BarSubUpdate> { update });
        }

        public void Modify(IBarSubInternal sub, List<BarSubUpdate> updates)
        {
            var groupUpdates = new List<BarSubUpdate>();

            lock (_syncObj)
            {
                foreach (var update in updates)
                {
                    if (ModifyGroup(sub, update))
                        groupUpdates.Add(update);
                }

                // clean up empty groups
                foreach (var update in groupUpdates)
                {
                    if (update.IsRemoveAction)
                        _groups.Remove(update.Entry);
                }
            }

            if (groupUpdates.Count > 0)
                _provider.Modify(groupUpdates);
        }

        public List<BarSubUpdate> GetCurrentSubs()
        {
            lock (_syncObj)
            {
                return _groups.Values.Where(g => !g.IsEmpty).Select(g => BarSubUpdate.Upsert(g.Entry)).ToList();
            }
        }


        private bool ModifyGroup(IBarSubInternal sub, BarSubUpdate update)
        {
            var entry = update.Entry;
            var propagate = false;

            if (update.IsUpsertAction)
            {
                var group = GetOrAddGroup(entry);
                propagate = group.Upsert(sub);
            }
            else if (update.IsRemoveAction)
            {
                var group = GetGroupOrDefault(entry);
                propagate = group?.Remove(sub) ?? false;
            }

            return propagate;
        }

        private SubGroup GetOrAddGroup(BarSubEntry entry)
        {
            return _groups.GetOrAdd(entry, e => new SubGroup(e));
        }

        private SubGroup GetGroupOrDefault(BarSubEntry entry)
        {
            _groups.TryGetValue(entry, out var group);
            return group;
        }


        private class SubGroup
        {
            private readonly Dictionary<IBarSubInternal, bool> _subs = new Dictionary<IBarSubInternal, bool>();


            public BarSubEntry Entry { get; }

            public bool IsEmpty => _subs.Count == 0;


            public SubGroup(BarSubEntry entry)
            {
                Entry = entry;
            }


            public bool Upsert(IBarSubInternal sub)
            {
                var propagate = IsEmpty;
                _subs[sub] = false;
                return propagate;
            }

            public bool Remove(IBarSubInternal sub)
            {
                _subs.Remove(sub);
                return IsEmpty;
            }
        }
    }
}
