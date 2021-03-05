using System.Linq;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public static class InfoExtensions
    {
        public static MappingInfo ToInfo(this Mapping mapping)
        {
            return new MappingInfo
            {
                Key = mapping.Key,
                DisplayName = mapping.DisplayName,
            };
        }

        public static MappingCollectionInfo ToInfo(this MappingCollection mappings)
        {
            return new MappingCollectionInfo
            {
                BarToBarMappings = mappings.BarToBarMappings.Values.Select(ToInfo).ToList(),
                BarToDoubleMappings = mappings.BarToDoubleMappings.Values.Select(ToInfo).ToList(),
                QuoteToBarMappings = mappings.QuoteToBarMappings.Values.Select(ToInfo).ToList(),
                QuoteToDoubleMappings = mappings.QuoteToDoubleMappings.Values.Select(ToInfo).ToList(),
                DefaultFullBarToBarReduction = MappingCollection.DefaultFullBarToBarReduction,
                DefaultBarToDoubleReduction = MappingCollection.DefaultBarToDoubleReduction,
                DefaultFullBarToDoubleReduction = MappingCollection.DefaultFullBarToDoubleReduction,
                DefaultQuoteToBarReduction = MappingCollection.DefaultQuoteToBarReduction,
                DefaultQuoteToDoubleReduction = MappingCollection.DefaultQuoteToDoubleReduction,
            };
        }

        public static SymbolKey ToInfo(this ISymbolInfo symbol)
        {
            return new SymbolKey(symbol.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
