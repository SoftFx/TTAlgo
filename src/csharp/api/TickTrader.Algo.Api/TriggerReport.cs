using System;

namespace TickTrader.Algo.Api
{
    public interface TriggerReport
    {
        string ContingentOrderId { get; }

        DateTime TransactionTime { get; }

        ContingentOrderTrigger.TriggerType TriggerType { get; }

        TriggerResultState TriggerState { get; }

        DateTime? TriggerTime { get; }

        string OrderIdTriggeredBy { get; }

        string Symbol { get; }

        OrderType Type { get; }

        OrderSide Side { get; }

        double? Price { get; }

        double? StopPrice { get; }

        double Volume { get; }

        string RelatedOrderId { get; }
    }

    public enum TriggerResultState
    {
        Failed,
        Successful,
    }
}
