using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    public class ContingentOrderTriggerAccessor : IContingentOrderTrigger
    {
        private readonly Domain.ContingentOrderTrigger _trigger;


        public ContingentOrderTrigger.TriggerType Type => _trigger.Type.ToApi();

        public DateTime? TriggerTime => _trigger.TriggerTime?.ToDateTime();

        public string OrderIdTriggeredBy => _trigger.OrderIdTriggeredBy;


        public ContingentOrderTriggerAccessor(Domain.ContingentOrderTrigger trigger)
        {
            _trigger = trigger;
        }
    }
}
