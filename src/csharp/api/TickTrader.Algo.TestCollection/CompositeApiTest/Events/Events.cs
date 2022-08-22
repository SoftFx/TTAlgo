using System;
using System.Linq;
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

        internal virtual Type[] PendingOnTimeActivationExpire { get; } = new[] { Events.Activate, Events.Open, Events.Expire };

        internal virtual Type[] ActivatePosition { get; } = Array.Empty<Type>();


        internal virtual Type[] FullExecutionPath(OrderStateTemplate template)
        {
            return template.IsStopLimit ? Events.Order.FillStopLimit : Events.Order.Fill;
        }

        internal virtual Type[] FullActivationPath(OrderStateTemplate template) => FullExecutionPath(template);
    }


    internal sealed class GrossOrderEvents : OrderEvents
    {
        internal override Type[] Fill { get; } = new[] { Events.Fill, Events.Open };

        internal override Type[] PartialFill { get; } = new[] { Events.Fill, Events.Fill, Events.Open };

        internal override Type[] FillStopLimit { get; } = new[] { Events.Activate, Events.Open, Events.Fill, Events.Open };

        internal override Type[] InstantOnTimeActivation { get; } = new[] { Events.Activate, Events.Open, Events.Fill, Events.Open };

        internal override Type[] ActivatePosition { get; } = new[] { Events.Close };


        internal override Type[] FullActivationPath(OrderStateTemplate template)
        {
            return base.FullActivationPath(template).Concat(ActivatePosition).ToArray();
        }
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
