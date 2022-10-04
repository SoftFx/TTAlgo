using System;
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

        IDisposable AddHandler(Action<BarInfo> handler);
    }


    public class BarSubscription : IBarSub, IBarSubInternal
    {
        private readonly object _syncObj = new object();
        private readonly HashSet<BarSubEntry> _subEntries = new HashSet<BarSubEntry>();
        private readonly IBarSubManager _manager;

        private ChannelConsumerWrapper<BarInfo> _barConsumer;
        private SubList<HandlerWrapper> _handlers;


        public BarSubscription(IBarSubManager manager)
        {
            _manager = manager;

            manager.Add(this);
        }


        public void Dispose()
        {
            _barConsumer?.Dispose();
            _manager.Remove(this);
            _manager.Modify(this, _subEntries.Select(e => BarSubUpdate.Remove(e)).ToList());
        }


        public void Modify(BarSubUpdate update)
        {
            var propagate = false;

            lock (_syncObj)
            {
                if (update.IsUpsertAction)
                    propagate = _subEntries.Add(update.Entry);
                else if (update.IsRemoveAction)
                    propagate = _subEntries.Remove(update.Entry);
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
                    if (update.IsUpsertAction)
                    {
                        if (_subEntries.Add(update.Entry))
                            validUpdates.Add(update);
                    }
                    else if (update.IsRemoveAction)
                    {
                        if (_subEntries.Remove(update.Entry))
                            validUpdates.Add(update);
                    }
                }
            }

            if (validUpdates.Count > 0)
                _manager.Modify(this, updates);
        }

        public IDisposable AddHandler(Action<BarInfo> handler)
        {
            if (_handlers == null)
            {
                _handlers = new SubList<HandlerWrapper>();
                _barConsumer = new ChannelConsumerWrapper<BarInfo>(DefaultChannelFactory.CreateForOneToOne<BarInfo>(), $"{nameof(BarSubscription)} loop");
                _barConsumer.BatchSize = 10;
                _barConsumer.Start(DispatchBar);
            }

            return new HandlerWrapper(handler, this);
        }

        void IBarSubInternal.Dispatch(BarInfo bar) => _barConsumer?.Add(bar);


        private void DispatchBar(BarInfo bar)
        {
            var sublist = _handlers.Items;
            for (var i = 0; i < sublist.Length; i++)
            {
                sublist[i].OnNewBar(bar);
            }
        }


        private class HandlerWrapper : IDisposable
        {
            private readonly Action<BarInfo> _handler;
            private readonly BarSubscription _parent;


            public HandlerWrapper(Action<BarInfo> handler, BarSubscription parent)
            {
                _handler = handler;
                _parent = parent;

                _parent._handlers.AddSub(this);
            }


            public void Dispose()
            {
                _parent._handlers.RemoveSub(this);
            }


            public void OnNewBar(BarInfo bar)
            {
                _handler?.Invoke(bar);
            }
        }
    }
}
