using System;

namespace TickTrader.Algo.Api
{
    public class ContingentOrderTrigger
    {
        public enum TriggerType
        {
            None,
            OnPendingOrderExpired,
            OnPendingOrderPartiallyFilled,
            OnTime,
        }

        public TriggerType Type { get; private set; }

        public DateTime? TriggerTime { get; private set; }

        public string OrderIdTriggeredBy { get; private set; }


        private ContingentOrderTrigger(TriggerType type, DateTime? triggerTime = null, string orderId = null)
        {
            Type = type;
            TriggerTime = triggerTime;
            OrderIdTriggeredBy = orderId;
        }


        public static ContingentOrderTrigger Create(TriggerType type) => new ContingentOrderTrigger(type);

        public static ContingentOrderTrigger Create(DateTime triggerTime) => new ContingentOrderTrigger(TriggerType.OnTime, triggerTime);

        public static ContingentOrderTrigger Create(TriggerType type, string orderIdTriggeredBy) => new ContingentOrderTrigger(type, orderId: orderIdTriggeredBy);

        public static ContingentOrderTrigger Create(TriggerType type, DateTime? triggerTime, string orderIdTriggeredBy) => new ContingentOrderTrigger(type, triggerTime, orderIdTriggeredBy);


        public ContingentOrderTrigger WithTriggerTime(DateTime? triggerTime)
        {
            TriggerTime = triggerTime;
            return this;
        }

        public ContingentOrderTrigger WithOrderIdTriggeredBy(string orderId)
        {
            OrderIdTriggeredBy = orderId;
            return this;
        }
    }
}
