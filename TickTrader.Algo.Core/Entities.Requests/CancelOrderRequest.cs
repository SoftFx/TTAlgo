using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class CancelOrderRequest : OrderCoreRequest
    {
         public string OrderId { get; set; }
    }
}
