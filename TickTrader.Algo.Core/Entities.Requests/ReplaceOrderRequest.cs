using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class ReplaceOrderRequest : OrderRequest
    {
        public string OrderId { get; set; }
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double CurrentVolume { get; set; }
        public double? NewVolume { get; set; }
        public double? Price { get; set; }
        public double? StopPrice { get; set; }
        public double? MaxVisibleVolume { get; set; }
        public double? StopLoss { get; set; }
        public double? TrakeProfit { get; set; }
        public string Comment { get; set; }
        public string Tag { get; set; }
        public DateTime? Expiration { get; set; }
        public OrderExecOptions? Options { get; set; }
        public bool? OverrideIoC => Options.HasValue ? Options.Value.HasFlag(OrderExecOptions.ImmediateOrCancel) : (bool?)null;
    }
}
