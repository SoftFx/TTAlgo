using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Calc.Conversion;

namespace TickTrader.Algo.Core
{
    internal class AlgoMarketNode : SymbolMarketNode
    {
        public AlgoMarketNode(SymbolAccessor smb) : base(smb)
        {
        }

        public ActivationRegistry ActivationIndex { get; set; }
        public SubscriptionManager.Collection UserSubscriptions { get; set; }
    }
}
