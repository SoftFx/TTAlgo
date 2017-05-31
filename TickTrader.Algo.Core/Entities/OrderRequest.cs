using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OrderRequest
    {
        public string Symbol { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public double? Sl { get; set; }
        public string Comment { get; set; }
        public OrderExecOptions ExecOptions { get; set; }
        public string Tag { get; set; }
    }
}
