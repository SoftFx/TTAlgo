using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class CancelOrderRequest : OrderRequest
    {
         public string OrderId { get; set; }
         public OrderSide Side { get; set; }
    }
}
