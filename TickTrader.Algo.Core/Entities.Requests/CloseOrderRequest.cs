using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class CloseOrderCoreRequest : OrderCoreRequest
    {
        public string OrderId { get; set; }
        public double? Volume { get; set; }
        public double? VolumeLots { get; set; }
        public double? Slippage { get; set; }
        public string ByOrderId { get; set; }
    }
}
