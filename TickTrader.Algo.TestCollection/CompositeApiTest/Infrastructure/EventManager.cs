using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class EventManager
    {
        private readonly Dictionary<Type, TimeSpan> _timeouts = new Dictionary<Type, TimeSpan>
        {
            [typeof(OrderOpenedEventArgs)] = TimeSpan.FromSeconds(5),
            [typeof(OrderFilledEventArgs)] = TimeSpan.FromSeconds(10),
            [typeof(OrderExpiredEventArgs)] = TimeSpan.FromSeconds(25),
            [typeof(OrderActivatedEventArgs)] = TimeSpan.FromSeconds(10),
            [typeof(OrderModifiedEventArgs)] = TimeSpan.FromSeconds(10),
            [typeof(OrderCanceledEventArgs)] = TimeSpan.FromSeconds(5),
            [typeof(OrderClosedEventArgs)] = TimeSpan.FromSeconds(5),
        };

        private readonly List<Type> _expectedEventsQueue;
        private readonly List<Type> _originEventsQueue;
        private readonly CompositeTradeApiTest _bot;
        private readonly OrderList _orders;

        private TimeSpan _totalWaitingTime;
        private TaskCompletionSource<bool> _allEventsReceivedTask;

        internal int OriginCount => _originEventsQueue?.Count ?? 0;

        internal int ExpectedCount => _originEventsQueue?.Count ?? 0;


        internal EventManager(CompositeTradeApiTest bot)
        {
            _expectedEventsQueue = new List<Type>(1 << 4);
            _originEventsQueue = new List<Type>(1 << 4);

            _bot = bot;
            _orders = bot.Account.Orders;
        }


        internal void AddEvent<T>()
        {
            var type = typeof(T);

            _totalWaitingTime += _timeouts[type];
            _expectedEventsQueue.Add(type);
        }

        internal void ClearEventsQueue()
        {
            _expectedEventsQueue.Clear();
            _originEventsQueue.Clear();

            _totalWaitingTime = TimeSpan.Zero;
            _allEventsReceivedTask = new TaskCompletionSource<bool>();
        }

        internal async Task WaitAllEvents()
        {
            var eventTask = _allEventsReceivedTask.Task;

            if (await Task.WhenAny(eventTask, Task.Delay(_totalWaitingTime)) != eventTask)
                throw new Exception("Events Timeout");

            var allEventsReceived = await eventTask;

            if (allEventsReceived)
            {
                for (int i = 0; i < ExpectedCount; ++i)
                    if (_originEventsQueue[i] != _expectedEventsQueue[i])
                        throw new Exception($"Unexpected event: Received {_originEventsQueue[i].Name} while expecting {_expectedEventsQueue[i].Name}");
            }
            else
                throw new Exception($"Unexpected number of events: expect = {ExpectedCount}, origin = {OriginCount}");
        }

        internal void SubscribeToOrderEvents()
        {
            _orders.Opened += OnCollectionEventFired;
            _orders.Filled += OnCollectionEventFired;
            _orders.Closed += OnCollectionEventFired;
            _orders.Expired += OnCollectionEventFired;
            _orders.Canceled += OnCollectionEventFired;
            _orders.Activated += OnCollectionEventFired;
            _orders.Modified += OnCollectionEventFired;
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
        }

        private void OnCollectionEventFired<T>(T args)
        {
            var eventType = typeof(T);

            _bot.PrintDebug($"Event {eventType.Name} received");
            _originEventsQueue.Add(eventType);

            if (OriginCount == ExpectedCount)
                _allEventsReceivedTask?.TrySetResult(true);
        }

        private void OnSingeOrderEventFired(SingleOrderEventArgs args)
        {
            //if (args is DoubleOrderEventArgs doubleArgs)
            //    OnDoubleOrderEventFired(doubleArgs);
        }

        private void OnDoubleOrderEventFired(DoubleOrderEventArgs args)
        {

        }
    }
}
