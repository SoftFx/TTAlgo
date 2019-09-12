using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Calc.Conversion;
using TickTrader.Algo.Core.Infrastructure;

namespace TickTrader.Algo.Core
{
    /// <summary>
    /// Aggregates different entities by symbol to minimize dicionary lookups.
    /// </summary>
    internal class AlgoMarketNode : SymbolMarketNode
    {
        public AlgoMarketNode(SymbolAccessor smb) : base(smb)
        {
        }

        public ActivationRegistry ActivationIndex { get; set; }
        public IFeedSubscription UserSubscriptionInfo { get; set; }
        public SubscriptionGroup SubGroup { get; set; }
    }
}
