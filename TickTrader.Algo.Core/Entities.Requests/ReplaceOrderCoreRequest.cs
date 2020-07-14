using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class ReplaceOrderCoreRequest : OrderCoreRequest
    {
        public string OrderId { get; set; }
        public string Symbol { get; set; }
        public Domain.OrderInfo.Types.Type Type { get; set; }
        public Domain.OrderInfo.Types.Side Side { get; set; }
        public double CurrentVolume { get; set; }
        public double CurrentVolumeLots { get; set; }
        public double? NewVolume { get; set; }
        public double? NewVolumeLots { get; set; }
        public double VolumeChange { get; set; }
        public double? Price { get; set; }
        public double? StopPrice { get; set; }
        public double? MaxVisibleVolume { get; set; }
        public double? MaxVisibleVolumeLots { get; set; }
        public double? StopLoss { get; set; }
        public double? TakeProfit { get; set; }
        public double? Slippage { get; set; }
        public string Comment { get; set; }
        public string Tag { get; set; }
        public DateTime? Expiration { get; set; }
        public OrderExecOptions? Options { get; set; }
        public bool? OverrideIoC => Options.HasValue ? Options.Value.HasFlag(OrderExecOptions.ImmediateOrCancel) : (bool?)null;
    }
}
