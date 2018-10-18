using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class NetPositionCloseInfo
    {
        public decimal CloseAmount { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal BalanceMovement { get; set; }
        public TradeChargesInfo Charges { get; set; }

        public SymbolAccessor SymbolInfo { get; set; }
    }
}
