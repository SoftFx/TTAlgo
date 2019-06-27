using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class NetPositionOpenInfo
    {
        public TradeChargesInfo Charges { get; set; }
        public PositionAccessor ResultingPosition { get; set; }
        //public SymbolAccessor SymbolInfo { get; set; }
        public NetPositionCloseInfo CloseInfo { get; set; }
    }

    internal class NetPositionCloseInfo
    {
        public double CloseAmount { get; set; }
        public double ClosePrice { get; set; }
        public double BalanceMovement { get; set; }
    }
}
