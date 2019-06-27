using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    public static class AccountMetadataInfoExtensions
    {
        public static IReadOnlyList<SymbolKey> GetAvaliableSymbols(this AccountMetadataInfo metadata, SymbolKey defaultSymbol)
        {
            var symbols = metadata?.Symbols;
            if ((symbols?.Count ?? 0) == 0)
                symbols = new List<SymbolKey> { defaultSymbol };
            return symbols;
        }

        public static SymbolKey GetSymbolOrDefault(this IReadOnlyList<SymbolKey> availableSymbols, SymbolConfig config)
        {
            if (config != null)
                return availableSymbols.FirstOrDefault(s => s.Origin == config.Origin && s.Name == config.Name);
            return null;
        }

        public static SymbolKey GetSymbolOrAny(this IReadOnlyList<SymbolKey> availableSymbols, SymbolKey info)
        {
            return availableSymbols.FirstOrDefault(s => s.Origin == info.Origin && s.Name == info.Name)
                ?? availableSymbols.First();
        }
    }


    public static class SymbolInfoExtensions
    {
        public static SymbolConfig ToConfig(this SymbolKey info)
        {
            return new SymbolConfig { Name = info.Name, Origin = info.Origin };
        }
    }


    public static class MappingCollectionInfoExtensions
    {
        public static MappingInfo GetBarToBarMappingOrDefault(this MappingCollectionInfo mappingCollection, MappingKey mappingKey)
        {
            var mapping = mappingCollection.BarToBarMappings.FirstOrDefault(m => m.Key.Equals(mappingKey));
            if (mapping == null)
            {
                var defaultMappingKey = new MappingKey(mappingCollection.DefaultFullBarToBarReduction);
                mapping = mappingCollection.BarToBarMappings.First(m => m.Key.Equals(defaultMappingKey));
            }
            return mapping;
        }

        public static MappingInfo GetBarToDoubleMappingOrDefault(this MappingCollectionInfo mappingCollection, MappingKey mappingKey)
        {
            var mapping = mappingCollection.BarToDoubleMappings.FirstOrDefault(m => m.Key.Equals(mappingKey));
            if (mapping == null)
            {
                var defaultMappingKey = new MappingKey(mappingCollection.DefaultFullBarToBarReduction, mappingCollection.DefaultBarToDoubleReduction);
                mapping = mappingCollection.BarToDoubleMappings.First(m => m.Key.Equals(defaultMappingKey));
            }
            return mapping;
        }

        public static MappingInfo GetQuoteToBarMappingOrDefault(this MappingCollectionInfo mappingCollection, MappingKey mappingKey)
        {
            var mapping = mappingCollection.QuoteToBarMappings.FirstOrDefault(m => m.Key.Equals(mappingKey));
            if (mapping == null)
            {
                var defaultMappingKey = new MappingKey(mappingCollection.DefaultQuoteToBarReduction);
                mapping = mappingCollection.QuoteToBarMappings.First(m => m.Key.Equals(defaultMappingKey));
            }
            return mapping;
        }

        public static MappingInfo GetQuoteToDoubleMappingOrDefault(this MappingCollectionInfo mappingCollection, MappingKey mappingKey)
        {
            var mapping = mappingCollection.QuoteToDoubleMappings.FirstOrDefault(m => m.Key.Equals(mappingKey));
            if (mapping == null)
            {
                var defaultMappingKey = new MappingKey(mappingCollection.DefaultQuoteToBarReduction, mappingCollection.DefaultBarToDoubleReduction);
                mapping = mappingCollection.QuoteToDoubleMappings.First(m => m.Key.Equals(defaultMappingKey));
            }
            return mapping;
        }
    }


    public static class SetupContextExtensions
    {
        public static SetupContextInfo GetSetupContextInfo(this IAlgoSetupContext setupContext)
        {
            return new SetupContextInfo(setupContext.DefaultTimeFrame, setupContext.DefaultSymbol.ToInfo(), setupContext.DefaultMapping);
        }
    }
}
