using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Calc.Conversion
{
    public class SymbolMarketNode
    {
        public SymbolMarketNode(ISymbolInfo smb)
        {
            SymbolInfo = smb;
            Rate = new QuoteInfo(smb.Name);
        }

        public ISymbolInfo SymbolInfo { get; }
        public IRateInfo Rate { get; private set; }

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
    }
}
