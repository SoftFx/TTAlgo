using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Calc.Conversion
{
    public class SymbolMarketNode
    {
        public SymbolMarketNode(SymbolAccessor smb)
        {
            SymbolInfo = smb;
            Rate = new QuoteEntity(smb.Name, DateTime.MinValue, (double?)null, (double?)null); // empty rate
        }

        public SymbolAccessor SymbolInfo { get; private set; }
        public RateUpdate Rate { get; private set; }
        public bool IsShadowCopy { get; private set; }

        public double Ask => Rate.Ask;
        public double Bid => Rate.Bid;

        public bool HasBid => Rate.HasBid;
        public bool HasAsk => Rate.HasAsk;

        public void Update(RateUpdate rate)
        {
            Rate = rate;
            Changed?.Invoke();
        }

        public event Action Changed;

        public void Update(SymbolAccessor smb)
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
