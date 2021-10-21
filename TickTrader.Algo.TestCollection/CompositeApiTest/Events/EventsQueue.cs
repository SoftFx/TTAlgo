using System;
using System.Collections.Generic;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class EventsQueue
    {
        private readonly List<(Type Event, OrderTemplate Order)> _expected;
        private readonly List<EventsQueueNode> _origin;

        private OrderTemplate _expectedTemplate;


        protected int ExpectedCount => _expected?.Count ?? 0;

        protected int OriginCount => _origin?.Count ?? 0;

        protected bool FullOriginQueue => ExpectedCount == OriginCount;


        protected EventsQueue()
        {
            _expected = new List<(Type, OrderTemplate)>(1 << 4);
            _origin = new List<EventsQueueNode>(1 << 4);
        }


        internal virtual void AddExpectedEvent(Type type) => _expected.Add((type, null));

        internal virtual void AddOriginEvent(EventsQueueNode node) => _origin.Add(node);


        protected void VerifyOriginQueue(string mainOrderId)
        {
            //GenerateExpectedTemplates(mainOrderId);

            if (!FullOriginQueue)
                throw EventException.UnexpectedCount(ExpectedCount, OriginCount);

            for (int i = 0; i < ExpectedCount; ++i)
                CompareEvents(_expected[i], _origin[i]);
        }

        protected void SetNewTemplate(OrderTemplate template)
        {
            _expectedTemplate = template?.Copy();

            _expected.Clear();
            _origin.Clear();
        }

        private void GenerateExpectedTemplates(string orderId)
        {
            _expected[0] = (_expected[0].Event, _expectedTemplate.ToOpen(orderId).Copy());

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

        private static void CompareEvents((Type Event, OrderTemplate Order) expect, EventsQueueNode origin)
        {
            if (expect.Event != origin.Event)
                throw EventException.UnexpectedEvent(expect.Event, origin.Event);

            //OrderComparator.Compare(origin.CurrentArg, expect.Order);
            //OrderComparator.Compare(collectionOrder, expect.Order);
        }
    }
}
