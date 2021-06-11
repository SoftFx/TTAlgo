using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public class SymbolMarketNode
    {
        public SymbolMarketNode(SymbolInfo smb)
        {
            SymbolInfo = smb;
        }

        public SymbolInfo SymbolInfo { get; private set; }
        public bool IsShadowCopy { get; private set; }

        public object ActivationIndex { get; set; }
        public IFeedSubscription UserSubscriptionInfo { get; set; }
        public SubscriptionGroup SubGroup { get; set; }


        public void Update(SymbolInfo smb)
        {
            if (smb == null)
            {
                IsShadowCopy = true;
                return;
            }
            IsShadowCopy = false;
            SymbolInfo = smb;
        }
    }
}
