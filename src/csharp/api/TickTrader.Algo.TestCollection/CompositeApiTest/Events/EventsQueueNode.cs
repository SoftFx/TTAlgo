using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal readonly struct EventsQueueNode
    {
        internal Type Event { get; }

        internal Order CurrentArg { get; }

        internal Order OldArg { get; }


        internal EventsQueueNode(Type @event, SingleOrderEventArgs args) : this(@event, args.Order) { }

        internal EventsQueueNode(Type @event, DoubleOrderEventArgs args) : this(@event, args.NewOrder, args.OldOrder) { }

        private EventsQueueNode(Type @event, Order current, Order old = null)
        {
            Event = @event;
            CurrentArg = current;
            OldArg = old;
        }
    }
}
