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

        public static Api.IContingentOrderTrigger ToApi(this Domain.ContingentOrderTrigger trigger)
        {
            return new ContingentOrderTriggerAccessor(trigger);
        }

        public static Api.ContingentOrderTrigger.TriggerType ToApi(this Domain.ContingentOrderTrigger.Types.TriggerType type)
        {
            switch (type)
            {
                case Domain.ContingentOrderTrigger.Types.TriggerType.OnPendingOrderExpired:
                    return Api.ContingentOrderTrigger.TriggerType.OnPendingOrderExpired;

                case Domain.ContingentOrderTrigger.Types.TriggerType.OnPendingOrderPartiallyFilled:
                    return Api.ContingentOrderTrigger.TriggerType.OnPendingOrderPartiallyFilled;

                case Domain.ContingentOrderTrigger.Types.TriggerType.OnTime:
                    return Api.ContingentOrderTrigger.TriggerType.OnTime;

                default:
                    throw new ArgumentException($"Unsupported trigger type: {type}");
            }
        }

        public static Api.TriggerResultState ToApi(this Domain.TriggerReportInfo.Types.TriggerResultState state)
        {
            switch (state)
            {
                case Domain.TriggerReportInfo.Types.TriggerResultState.Failed:
                    return Api.TriggerResultState.Failed;
                case Domain.TriggerReportInfo.Types.TriggerResultState.Successful:
                    return Api.TriggerResultState.Successful;

                default:
                    throw new ArgumentException($"Unsupported trigger state: {state}");
            }
        }
    }
}