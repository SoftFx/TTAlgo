using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class EventsQueueManager : EventsQueue
    {
        private readonly Dictionary<Type, TimeSpan> _timeouts = new Dictionary<Type, TimeSpan>
        {
            [OrderEvents.Open] = TimeSpan.FromSeconds(5),
            [OrderEvents.Fill] = TimeSpan.FromSeconds(10),
            [OrderEvents.Expire] = TimeSpan.FromSeconds(25),
            [OrderEvents.Activate] = TimeSpan.FromSeconds(10),
            [OrderEvents.Modify] = TimeSpan.FromSeconds(10),
            [OrderEvents.Cancel] = TimeSpan.FromSeconds(5),
            [OrderEvents.Close] = TimeSpan.FromSeconds(5),
        };

        private readonly CompositeTradeApiTest _bot;
        private readonly OrderList _orders;

        private TaskCompletionSource<bool> _allEventsReceivedTask = new TaskCompletionSource<bool>();
        private TimeSpan _totalWaitingTime;


        internal EventsQueueManager(CompositeTradeApiTest bot) : base()
        {
            _bot = bot;
            _orders = bot.Account.Orders;
        }


        internal void AddExpectedEvents(params Type[] types)
        {
            foreach (var type in types)
                AddExpectedEvent(type);
        }

        internal override void AddExpectedEvent(Type type)
        {
            if (_allEventsReceivedTask.Task.IsCompleted)
                _allEventsReceivedTask = new TaskCompletionSource<bool>();

            _totalWaitingTime += _timeouts[type];

            _bot.PrintDebug($"Add expected event {type.Name}");

            base.AddExpectedEvent(type);
        }

        internal void ResetAllQueues()
        {
            ClearQueues();

            _totalWaitingTime = TimeSpan.Zero;
            _allEventsReceivedTask = new TaskCompletionSource<bool>();
        }

        internal async Task WaitAllEvents()
        {
            var eventTask = _allEventsReceivedTask.Task;

            if (ExpectedCount > 0 && await Task.WhenAny(eventTask, Task.Delay(_totalWaitingTime)) != eventTask)
                throw EventException.TimeoutException;

            VerifyOriginQueue();
        }

        internal void SubscribeToOrderEvents()
        {
            ResetAllQueues();

            _orders.Opened += OnCollectionEventFired;
            _orders.Filled += OnCollectionEventFired;
            _orders.Closed += OnCollectionEventFired;
            _orders.Expired += OnCollectionEventFired;
            _orders.Canceled += OnCollectionEventFired;
            _orders.Activated += OnCollectionEventFired;
            _orders.Modified += OnCollectionEventFired;

            //_orders.Added += OnCollectionOrderReceived;
            //_orders.Removed += OnCollectionOrderReceived;
            //_orders.Replaced += OnCollectionOrderReceived;
        }

        internal void UnsubscribeToOrderEvents()
        {
            _orders.Opened -= OnCollectionEventFired;
            _orders.Filled -= OnCollectionEventFired;
            _orders.Closed -= OnCollectionEventFired;
            _orders.Expired -= OnCollectionEventFired;
            _orders.Canceled -= OnCollectionEventFired;
            _orders.Activated -= OnCollectionEventFired;
            _orders.Modified -= OnCollectionEventFired;

            //_orders.Added -= OnCollectionOrderReceived;
            //_orders.Removed -= OnCollectionOrderReceived;
            //_orders.Replaced -= OnCollectionOrderReceived;
        }


        //private void OnCollectionOrderReceived(Order order) => AddCollectionOrder(order);

        private void OnCollectionEventFired<T>(T args)
        {
            var eventType = typeof(T);

            _bot.PrintDebug($"Event {eventType.Name} received");

            OriginEventReceived(eventType, args);

            if (FullOriginQueue)
                _allEventsReceivedTask?.TrySetResult(true);
        }
    }
}
