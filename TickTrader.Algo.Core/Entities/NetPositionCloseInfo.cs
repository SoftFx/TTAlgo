using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal struct NetPositionCloseInfo
    {
        public decimal CloseAmount { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal BalanceMovement { get; set; }
    }
}
