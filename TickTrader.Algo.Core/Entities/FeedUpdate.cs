using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public struct FeedUpdate
    {
        public FeedUpdate(string smb, QuoteEntity quote)
            : this()
        {
            this.SymbolCode = smb;
            this.Quote = quote;
        }

        public string SymbolCode { get; private set; }
        public QuoteEntity Quote { get; private set; }
    }
}
