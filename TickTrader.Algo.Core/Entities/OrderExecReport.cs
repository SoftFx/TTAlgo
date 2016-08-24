using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OrderExecReport
    {
        public string OrderId { get; set; }
        public OrderEntity OrderCopy { get; set; }
        public OrderExecAction ExecAction { get; set; }
        public OrderEntityAction Action { get; set; }
    }

    public enum OrderExecAction { Opened, Modified, Canceled, Closed, Filled, Expired }
    public enum OrderEntityAction { Added, Removed, Updated }
}
