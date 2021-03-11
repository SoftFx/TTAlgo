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
            var res = new MappingCollectionInfo
            {
                DefaultFullBarToBarReduction = MappingCollection.DefaultFullBarToBarReduction,
                DefaultBarToDoubleReduction = MappingCollection.DefaultBarToDoubleReduction,
                DefaultFullBarToDoubleReduction = MappingCollection.DefaultFullBarToDoubleReduction,
                DefaultQuoteToBarReduction = MappingCollection.DefaultQuoteToBarReduction,
                DefaultQuoteToDoubleReduction = MappingCollection.DefaultQuoteToDoubleReduction,
            };
            res.BarToBarMappings.AddRange(mappings.BarToBarMappings.Values.Select(ToInfo));
            res.BarToDoubleMappings.AddRange(mappings.BarToDoubleMappings.Values.Select(ToInfo));
            res.QuoteToBarMappings.AddRange(mappings.QuoteToBarMappings.Values.Select(ToInfo));
            res.QuoteToDoubleMappings.AddRange(mappings.QuoteToDoubleMappings.Values.Select(ToInfo));
            return res;
        }

        public static SymbolConfig ToInfo(this ISymbolInfo symbol)
        {
            return new SymbolConfig(symbol.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
