using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public abstract class OrderRequest
    {
        public string OperationId { get; set; }
    }
}
