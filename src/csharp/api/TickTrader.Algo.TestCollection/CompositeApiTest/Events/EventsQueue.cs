using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class EventsQueue
    {
        private readonly SortedDictionary<string, OrderStateTemplate> _currentTemplates;
        private readonly LinkedList<OrderStateTemplate> _expectedToOpenTemplates;

        private readonly List<(Type Event, OrderStateTemplate Order)> _expected;
        private readonly List<EventsQueueNode> _origin;


        protected int ExpectedCount => _expected?.Count ?? 0;

        protected int OriginCount => _origin?.Count ?? 0;

        protected bool FullOriginQueue => ExpectedCount == OriginCount;


        protected EventsQueue()
        {
            _currentTemplates = new SortedDictionary<string, OrderStateTemplate>();
            _expectedToOpenTemplates = new LinkedList<OrderStateTemplate>();

            _expected = new List<(Type, OrderStateTemplate)>(1 << 4);
            _origin = new List<EventsQueueNode>(1 << 4);
        }


        internal virtual void RegistryNewTemplate(OrderStateTemplate template) => _expectedToOpenTemplates.AddLast(template);

        internal virtual void RegisterExistingTemplate(OrderStateTemplate template)
        {
            if (!_currentTemplates.ContainsKey(template.Id))
                _currentTemplates.Add(template.Id, template);
        }

        internal virtual void AddExpectedEvent(Type type) => _expected.Add((type, null));


        protected void VerifyOriginQueue()
        {
            if (!FullOriginQueue)
                throw EventException.UnexpectedCount(ExpectedCount, OriginCount);

            for (int i = 0; i < ExpectedCount; ++i)
                CompareEvents(_expected[i], _origin[i]);
        }

        protected void OriginEventReceived<T>(Type eventType, T args)
        {
            switch (args)
            {
                case SingleOrderEventArgs single:
                    _origin.Add(new EventsQueueNode(eventType, single));
                    UpdateTemplates(eventType, single);
                    break;

                case DoubleOrderEventArgs @double:
                    _origin.Add(new EventsQueueNode(eventType, @double));
                    UpdateTemplates(eventType, @double);
                    break;
            }
        }

        protected void ClearQueues()
        {
            _expectedToOpenTemplates.Clear();
            _currentTemplates.Clear();
            _expected.Clear();
            _origin.Clear();
        }


        private void UpdateTemplates(Type @event, SingleOrderEventArgs args)
        {
            var orderId = args.Order.Id;

            if (@event == OrderEvents.Open)
            {
                var template = _expectedToOpenTemplates.First.Value;
                _expectedToOpenTemplates.RemoveFirst();

                RegisterExistingTemplate(template.ToOpen(orderId)); //for modify tests
                //_currentTemplates.Add(orderId, template.ToOpen(orderId));
            }

            if (@event == OrderEvents.Activate)
            {
                var template = _currentTemplates[orderId];
                _currentTemplates.Remove(orderId);

                if (template.IsOnTimeTrigger && template.TriggerTime <= DateTime.UtcNow)
                    _expectedToOpenTemplates.AddFirst(template.ToOnTimeTriggerReceived());
                else
                    _expectedToOpenTemplates.AddFirst(template.ToActivate());
            }

            if (@event == OrderEvents.Cancel)
            {
                //if (!_currentTemplates.TryGetValue(orderId, out var template))
                //    return;
                var template = _currentTemplates[orderId];
                _currentTemplates.Remove(orderId);

                template.ToCancel();
            }

            if (@event == OrderEvents.Close)
            {
                _currentTemplates.Remove(orderId);
            }
        }

        private void UpdateTemplates(Type @event, DoubleOrderEventArgs args)
        {
            var oldOrderId = args.OldOrder.Id;

            if (@event == OrderEvents.Fill)
            {
                var filledVolume = args.NewOrder.LastFillVolume;

                var baseTemplate = _currentTemplates[oldOrderId];
                var filledPart = baseTemplate.ToFilled(filledVolume);

                if (baseTemplate == filledPart)
                    _currentTemplates.Remove(oldOrderId);

                if (baseTemplate.IsGrossAcc)
                    _expectedToOpenTemplates.AddFirst(filledPart.ToGrossPosition());
            }

            if (@event == OrderEvents.Modify)
            {
                var baseTemplate = _currentTemplates[oldOrderId];
                baseTemplate.ToModified();
            }
        }

        private void GenerateExpectedTemplates(string orderId)
        {
            //_expected[0] = (_expected[0].Event, _expectedTemplate.ToOpen(orderId).Copy());

            for (int i = 1; i < ExpectedCount; ++i)
            {
                var (@event, _) = _expected[i];
                var current = _expected[i - 1].Order.Copy();

                switch (@event)
                {
                }

                _expected[i] = (@event, current);
            }
        }

        private static void CompareEvents((Type Event, OrderStateTemplate Order) expect, EventsQueueNode origin)
        {
            if (expect.Event != origin.Event)
                throw EventException.UnexpectedEvent(expect.Event, origin.Event);

            //OrderComparator.Compare(origin.CurrentArg, expect.Order);
            //OrderComparator.Compare(collectionOrder, expect.Order);
        }
    }
}
