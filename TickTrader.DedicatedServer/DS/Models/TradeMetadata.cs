using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class TradeMetadata : IAlgoGuiMetadata
    {
        public TradeMetadata(IEnumerable<ISymbolInfo> symbols)
        {
            Extentions = new ExtCollection();
            Symbols = symbols.ToList();
        }

        public ExtCollection Extentions { get; private set; }
        public IReadOnlyList<ISymbolInfo> Symbols { get; private set; }
    }
}
