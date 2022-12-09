using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Subscriptions
{
    public interface IBarSub : IDisposable
    {
        void Modify(BarSubUpdate update);

        void Modify(List<BarSubUpdate> updates);

        IDisposable AddHandler(Action<BarUpdate> handler);
    }


    public class BarSubscription : IBarSub, IBarSubInternal
    {
        private readonly object _syncObj = new object();
        private readonly ConcurrentDictionary<BarSubEntry, byte> _subEntries = new ConcurrentDictionary<BarSubEntry, byte>();
        private readonly BarSubEntry _dispatchEntry = new BarSubEntry();
        private readonly IBarSubManager _manager;
        private readonly bool _useFiltering;

        private ChannelConsumerWrapper<BarUpdate> _barConsumer;
        private SubList<HandlerWrapper> _handlers;


        public BarSubscription(IBarSubManager manager, bool useFiltering)
        {
            _manager = manager;
            _useFiltering = useFiltering;

            manager.Add(this);
        }


        public void Dispose()
        {
            _barConsumer?.Dispose();
            _manager.Remove(this);
            _manager.Modify(this, _subEntries.Select(e => BarSubUpdate.Remove(e.Key)).ToList());
        }


        public void Modify(BarSubUpdate update)
        {
            var propagate = false;

            lock (_syncObj)
            {
                propagate = ApplyUpdate(update);
            }

            if (propagate)
                _manager.Modify(this, update);
        }

        public void Modify(List<BarSubUpdate> updates)
        {
            var validUpdates = new List<BarSubUpdate>(updates.Count);

            lock (_syncObj)
            {
                foreach (var update in updates)
                {
                    if (ApplyUpdate(update))
                        validUpdates.Add(update);
                }
            }

            if (validUpdates.Count > 0)
                _manager.Modify(this, updates);
        }

        public IDisposable AddHandler(Action<BarUpdate> handler)
        {
            if (_handlers == null)
            {
                _handlers = new SubList<HandlerWrapper>();
                _barConsumer = new ChannelConsumerWrapper<BarUpdate>(DefaultChannelFactory.CreateForOneToOne<BarUpdate>(), $"{nameof(BarSubscription)} loop");
                _barConsumer.BatchSize = 10;
                _barConsumer.Start(DispatchBar);
            }

            return new HandlerWrapper(handler, this);
        }

        void IBarSubInternal.Dispatch(BarUpdate bar) => _barConsumer?.Add(bar);


        private bool ApplyUpdate(BarSubUpdate update)
        {
            if (update.IsUpsertAction)
                return _subEntries.TryAdd(update.Entry, 0);
            else if (update.IsRemoveAction)
                return _subEntries.TryRemove(update.Entry, out _);

            return false;
        }

        private void DispatchBar(BarUpdate bar)
        {
            if (_useFiltering)
            {
                // Single consuming thread. Using cached object to reduce memory traffic
                var entry = _dispatchEntry;
                entry.Symbol = bar.Symbol;
                entry.Timeframe = bar.Timeframe;

                if (!_subEntries.ContainsKey(_dispatchEntry))
                    return;
            }

            var sublist = _handlers.Items;
            for (var i = 0; i < sublist.Length; i++)
            {
                sublist[i].OnNewBar(bar);
            }
        }


        private class HandlerWrapper : IDisposable
        {
            private readonly Action<BarUpdate> _handler;
            private readonly BarSubscription _parent;


            public HandlerWrapper(Action<BarUpdate> handler, BarSubscription parent)
            {
                _handler = handler;
                _parent = parent;

                _parent._handlers.AddSub(this);
            }


            public void Dispose()
            {
                _parent._handlers.RemoveSub(this);
            }


            public void OnNewBar(BarUpdate bar)
            {
                _handler?.Invoke(bar);
            }
        }
    }
}
