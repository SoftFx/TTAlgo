using System;
using System.Collections.Concurrent;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public class BarDistributor : IBarSubInternal, IDisposable
    {
        private readonly ConcurrentDictionary<BarSubEntry, ListenerGroup> _groups = new ConcurrentDictionary<BarSubEntry, ListenerGroup>();
        private readonly IBarSubManager _manager;
        private readonly ChannelConsumerWrapper<BarInfo> _barConsumer;
        private readonly BarSubEntry _dispatchEntry = new BarSubEntry();


        public BarDistributor(IBarSubManager manager)
        {
            _manager = manager;
            _manager.Add(this);

            _barConsumer = new ChannelConsumerWrapper<BarInfo>(DefaultChannelFactory.CreateForOneToOne<BarInfo>(), $"{nameof(BarDistributor)} loop");
            _barConsumer.BatchSize = 10;
            _barConsumer.Start(DispatchBar);
        }


        public void Dispose()
        {
            _barConsumer.Dispose();
            _manager.Remove(this);
            _manager.Modify(this, _groups.Select(p => BarSubUpdate.Remove(p.Key)).ToList());
        }


        public void UpdateBar(BarInfo bar) => _barConsumer.Add(bar);

        void IBarSubInternal.Dispatch(BarInfo bar) => UpdateBar(bar);


        private void DispatchBar(BarInfo bar)
        {
            // Single consuming thread. Using cached object to reduce memory traffic
            var entry = _dispatchEntry;
            entry.Symbol = bar.Symbol;
            entry.MarketSide = bar.MarketSide;
            entry.Timeframe = bar.Timeframe;
            if (_groups.TryGetValue(entry, out var group))
            {
                group.DispathBar(bar);
            }
        }

        private void AddListener(BarSubEntry entry, Listener listener)
        {
            var sendUpdate = false;

            lock (_groups)
            {
                var group = _groups.GetOrAdd(entry, () => new ListenerGroup());
                group.Add(listener);
                sendUpdate = group.SubCount == 1;
            }

            if (sendUpdate)
                _manager.Modify(this, BarSubUpdate.Upsert(entry));
        }

        private void RemoveListener(BarSubEntry entry, Listener listener)
        {
            var sendUpdate = false;
            
            lock(_groups)
            {
                if (_groups.TryGetValue(entry, out var group))
                {
                    group.Remove(listener);
                    sendUpdate = group.SubCount == 0;
                    if (sendUpdate)
                        _groups.TryRemove(entry, out _);
                }
            }

            if (sendUpdate)
                _manager.Modify(this, BarSubUpdate.Remove(entry));
        }


        private class Listener
        {
            private readonly Action<BarInfo> _handler;
            private readonly BarDistributor _parent;
            private readonly BarSubEntry _entry;


            public Listener(Action<BarInfo> handler, BarDistributor parent, BarSubEntry entry)
            {
                _handler = handler;
                _parent = parent;
                _entry = entry;

                _parent.AddListener(_entry, this);
            }

            public void Dispose()
            {
                _parent.RemoveListener(_entry, this);
            }

            public void OnNewBar(BarInfo bar) => _handler?.Invoke(bar);
        }


        private class ListenerGroup
        {
            private readonly SubList<Listener> _subList = new SubList<Listener>();


            public int SubCount => _subList.Items.Length;


            public void Add(Listener listener)
            {
                _subList.AddSub(listener);
            }

            public void Remove(Listener listener)
            {
                _subList.RemoveSub(listener);
            }

            public void DispathBar(BarInfo bar)
            {
                var sublist = _subList.Items;
                for (var i = 0; i < sublist.Length; i++)
                {
                    sublist[i].OnNewBar(bar);
                }
            }
        }
    }
}
