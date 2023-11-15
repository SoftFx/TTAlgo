using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using static TickTrader.Algo.Api.BaseCloseRequest;

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

            if (@event == Events.Open)
            {
                var template = _expectedToOpenTemplates.First.Value;
                _expectedToOpenTemplates.RemoveFirst();

                if (template.IsGrossAcc && template.Opened.Task.IsCompleted)
                    template.ToGrossPosition();
                else
                    template.ToOpen(orderId);

                RegisterExistingTemplate(template); //for modify tests
                //_currentTemplates.Add(orderId, template.ToOpen(orderId));
            }

            if (@event == Events.Activate)
            {
                var template = _currentTemplates[orderId];
                _currentTemplates.Remove(orderId);

                if (template.IsOnTimeTrigger)
                    _expectedToOpenTemplates.AddFirst(template.ToOnTimeTriggerReceived());
                else
                    _expectedToOpenTemplates.AddFirst(template.ToActivate());
            }

            if (@event == Events.Cancel)
            {
                var template = _currentTemplates[orderId];
                _currentTemplates.Remove(orderId);

                template.ToCancel();
                template.OnFinalEvent(orderId, @event);
            }

            if (@event == Events.Close)
            {
                var template = _currentTemplates[orderId];
                _currentTemplates.Remove(orderId);

                template.ToClose();
                template.OnFinalEvent(orderId, @event);
            }

            if (@event == Events.Expire)
            {
                var template = _currentTemplates[orderId];
                _currentTemplates.Remove(orderId);

                template.ToExpire();
                template.OnFinalEvent(orderId, @event);
            }
        }

        private void UpdateTemplates(Type @event, DoubleOrderEventArgs args)
        {
            var oldOrderId = args.OldOrder.Id;

            if (@event == Events.Fill)
            {
                var filledVolume = args.NewOrder.LastFillVolume;

                var baseTemplate = _currentTemplates[oldOrderId];
                var filledPart = baseTemplate.ToFilled(filledVolume);

                if (baseTemplate == filledPart)
                {
                    _currentTemplates.Remove(oldOrderId);
                    if (!baseTemplate.IsGrossAcc)
                        baseTemplate.OnFinalEvent(oldOrderId, @event);
                }

                if (baseTemplate.IsGrossAcc)
                    _expectedToOpenTemplates.AddFirst(filledPart);
            }

            if (@event == Events.Modify)
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
