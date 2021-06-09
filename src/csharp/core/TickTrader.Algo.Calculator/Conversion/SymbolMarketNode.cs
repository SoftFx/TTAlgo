using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public class SymbolMarketNode
    {
        public SymbolMarketNode(SymbolInfo smb)
        {
            SymbolInfo = smb;
            //Rate = new QuoteInfo(smb.Name);
        }


        public SymbolInfo SymbolInfo { get; private set; }
        //public IRateInfo Rate { get; private set; }
        public bool IsShadowCopy { get; private set; }

        public double Ask => SymbolInfo.Ask;
        public double Bid => SymbolInfo.Bid;

        public bool HasBid => SymbolInfo.LastQuote.HasBid;
        public bool HasAsk => SymbolInfo.LastQuote.HasAsk;

        //public void Update(IRateInfo rate)
        //{
        //    Rate = rate;
        //    Changed?.Invoke();
        //}

        //public event Action Changed;

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
