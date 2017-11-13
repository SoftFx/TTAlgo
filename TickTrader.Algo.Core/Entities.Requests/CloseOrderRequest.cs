using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class CloseOrderRequest : OrderRequest
    {
        public string OrderId { get; set; }
        public double? Volume { get; set; }
        public string ByOrderId { get; set; }
    }
}
