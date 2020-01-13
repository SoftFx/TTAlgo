using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class CancelOrderRequest : OrderRequest
    {
         public string OrderId { get; set; }
    }
}
