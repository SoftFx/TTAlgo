using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Calc.Conversion
{
    public class SymbolMarketNode
    {
        public SymbolMarketNode(SymbolInfo smb)
        {
            SymbolInfo = smb;
            Rate = new QuoteInfo(smb.Name);
        }


        public SymbolInfo SymbolInfo { get; private set; }
        public IRateInfo Rate { get; private set; }
        public bool IsShadowCopy { get; private set; }

        public double Ask => Rate.Ask;
        public double Bid => Rate.Bid;

        public bool HasBid => Rate.HasBid;
        public bool HasAsk => Rate.HasAsk;

        public void Update(IRateInfo rate)
        {
            Rate = rate;
            Changed?.Invoke();
        }

        public event Action Changed;

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
