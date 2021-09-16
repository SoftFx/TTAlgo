using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.CoreV1
{
    public static class TriggerConverters
    {
        public static Domain.ContingentOrderTrigger ToDomain(this Api.ContingentOrderTrigger trigger)
        {
            return new Domain.ContingentOrderTrigger
            {
                Type = trigger.Type.ToDomain(),
                TriggerTime = trigger.TriggerTime?.ToUniversalTime().ToTimestamp(),
                OrderIdTriggeredBy = trigger.OrderIdTriggeredBy,
            };
        }

        public static Domain.ContingentOrderTrigger.Types.TriggerType ToDomain(this Api.ContingentOrderTrigger.TriggerType type)
        {
            switch (type)
            {
                case Api.ContingentOrderTrigger.TriggerType.None:
                    return Domain.ContingentOrderTrigger.Types.TriggerType.None;

                case Api.ContingentOrderTrigger.TriggerType.OnPendingOrderExpired:
                    return Domain.ContingentOrderTrigger.Types.TriggerType.OnPendingOrderExpired;

                case Api.ContingentOrderTrigger.TriggerType.OnPendingOrderPartiallyFilled:
                    return Domain.ContingentOrderTrigger.Types.TriggerType.OnPendingOrderPartiallyFilled;

                case Api.ContingentOrderTrigger.TriggerType.OnTime:
                    return Domain.ContingentOrderTrigger.Types.TriggerType.OnTime;

                default:
                    throw new ArgumentException($"Unsupported trigger type: {type}");
            }
        }
    }
}
