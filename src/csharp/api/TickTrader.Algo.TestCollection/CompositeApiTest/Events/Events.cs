using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OrderEvents
    {
        internal virtual Type[] Fill { get; } = new[] { Events.Fill };

        internal virtual Type[] PartialFill { get; } = new[] { Events.Fill, Events.Fill };

        internal virtual Type[] FillStopLimit { get; } = new[] { Events.Activate, Events.Open, Events.Fill };

        internal virtual Type[] InstantOnTimeActivation { get; } = new[] { Events.Activate, Events.Open, Events.Fill };

        internal virtual Type[] PendingOnTimeActivation { get; } = new[] { Events.Activate, Events.Open };

        internal virtual Type[] PendingOnTimeActivationExpire { get; } = new[] { Events.Activate, Events.Activate, Events.Open, Events.Expire };
    }


    internal sealed class GrossOrderEvents : OrderEvents
    {
        internal override Type[] Fill { get; } = new[] { Events.Fill, Events.Open };

        internal override Type[] PartialFill { get; } = new[] { Events.Fill, Events.Fill, Events.Open };

        internal override Type[] FillStopLimit { get; } = new[] { Events.Activate, Events.Open, Events.Fill, Events.Open };

        internal override Type[] InstantOnTimeActivation { get; } = new[] { Events.Activate, Events.Open, Events.Fill, Events.Open };
    }


    internal static class Events
    {
        internal static Type Open { get; } = typeof(OrderOpenedEventArgs);

        internal static Type Fill { get; } = typeof(OrderFilledEventArgs);

        internal static Type Expire { get; } = typeof(OrderExpiredEventArgs);

        internal static Type Activate { get; } = typeof(OrderActivatedEventArgs);

        internal static Type Modify { get; } = typeof(OrderModifiedEventArgs);

        internal static Type Cancel { get; } = typeof(OrderCanceledEventArgs);

        internal static Type Close { get; } = typeof(OrderClosedEventArgs);


        internal static OrderEvents Order { get; private set; }


        internal static void Init(AccountTypes acc)
        {
            Order = acc == AccountTypes.Gross ? new GrossOrderEvents() : new OrderEvents();
        }
    }
}
