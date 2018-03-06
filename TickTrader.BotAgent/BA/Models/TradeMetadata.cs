using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotAgent.BA.Models
{
    public class TradeMetadata : IAlgoSetupMetadata
    {
        public ExtCollection Extentions { get; }

        public IReadOnlyList<ISymbolInfo> Symbols { get; }

        public SymbolMappingsCollection SymbolMappings { get; }

        public IPluginIdProvider IdProvider { get; }

        public TradeMetadata(IEnumerable<ISymbolInfo> symbols)
        {
            //Extentions = new ExtCollection();
            Symbols = symbols.ToList();
        }
    }
}
