using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OpenOrderRequest : OrderRequest
    {
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double? Price { get; set; }
        public double? StopPrice { get; set; }
        public double Volume { get; set; }
        public double? MaxVisibleVolume { get; set; }
        public double? StopLoss { get; set; }
        public double? TakeProfit { get; set; }
        public string Comment { get; set; }
        public OrderExecOptions Options { get; set; }
        public string Tag { get; set; }
    }
}
