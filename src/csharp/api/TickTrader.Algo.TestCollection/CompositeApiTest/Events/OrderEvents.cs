using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal static class OrderEvents
    {
        internal static Type Open { get; } = typeof(OrderOpenedEventArgs);

        internal static Type Fill { get; } = typeof(OrderFilledEventArgs);

        internal static Type Expire { get; } = typeof(OrderExpiredEventArgs);

        internal static Type Activate { get; } = typeof(OrderActivatedEventArgs);

        internal static Type Modify { get; } = typeof(OrderModifiedEventArgs);

        internal static Type Cancel { get; } = typeof(OrderCanceledEventArgs);

        internal static Type Close { get; } = typeof(OrderClosedEventArgs);


        //shoud be after initialization single events
        internal static Type[] FillOrderEvents { get; } = new Type[] { Fill };

        internal static Type[] PartialFillOrderEvents { get; } = new Type[] { Fill, Fill };

        internal static Type[] FillStopLimitEvents { get; } = new[] { Activate, Open, Fill };

        internal static Type[] FillOnGrossOrderEvents { get; } = new Type[] { Fill, Open };

        internal static Type[] PartialFillGrossOrderEvents { get; } = new Type[] { Fill, Open, Fill, Open };

        internal static Type[] FillOnGrossStopLimitEvents { get; } = new[] { Activate, Open, Fill, Open };
    }
}
