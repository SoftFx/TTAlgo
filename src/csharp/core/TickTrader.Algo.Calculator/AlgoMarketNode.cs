using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    /// <summary>
    /// Aggregates different entities by symbol to minimize dicionary lookups.
    /// </summary>
    public class AlgoMarketNode : SymbolMarketNode
    {
        public AlgoMarketNode(SymbolInfo smb) : base(smb)
        {
        }

        public object ActivationIndex { get; set; }
        public IFeedSubscription UserSubscriptionInfo { get; set; }
        public SubscriptionGroup SubGroup { get; set; }
    }
}
