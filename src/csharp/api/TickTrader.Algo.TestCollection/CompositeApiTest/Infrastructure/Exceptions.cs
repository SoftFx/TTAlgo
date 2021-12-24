using System;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal sealed class EventException : Exception
    {
        internal static EventException TimeoutException { get; } = new EventException("Not all events were received");


        private EventException(string message) : base($"{nameof(EventException)}: {message}") { }


        internal static EventException UnexpectedCount(int expected, int current) =>
            new EventException($"Unexpected count of events expect={expected} current={current}");

        internal static EventException UnexpectedEvent(Type expected, Type current) =>
            new EventException($"Unexpected event {current.Name} while expecting {expected.Name}");
    }
}
