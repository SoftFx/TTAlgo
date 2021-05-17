using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public static class InfoExtensions
    {
        public static MappingCollectionInfo ToInfo(this MappingCollection mappings)
        {
            var res = new MappingCollectionInfo
            {
                DefaultBarToBarMapping = MappingCollection.DefaultBarToBarMapping.Key,
                DefaultBarToDoubleMapping = MappingCollection.DefaultBarToDoubleMapping.Key,
                DefaultQuoteToBarMapping = MappingCollection.DefaultQuoteToBarMapping.Key,
                DefaultQuoteToDoubleMapping = MappingCollection.DefaultQuoteToDoubleMapping.Key,
            };
            res.BarToBarMappings.AddRange(mappings.BarToBarMappings);
            res.BarToDoubleMappings.AddRange(mappings.BarToDoubleMappings);
            res.QuoteToBarMappings.AddRange(mappings.QuoteToBarMappings);
            res.QuoteToDoubleMappings.AddRange(mappings.QuoteToDoubleMappings);
            return res;
        }

        public static SymbolConfig ToInfo(this ISymbolInfo symbol)
        {
            return new SymbolConfig(symbol.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
