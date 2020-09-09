using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class AlgoSetupContext : IAlgoSetupContext
    {
        private static MappingKey _defaultMapping = new MappingKey(MappingCollection.BidBarReduction, MappingCollection.DefaultBarToDoubleReduction);


        public Feed.Types.Timeframe DefaultTimeFrame { get; }

        public ISetupSymbolInfo DefaultSymbol { get; }

        public MappingKey DefaultMapping => _defaultMapping;


        public AlgoSetupContext(Feed.Types.Timeframe defaultTimeFrame, SymbolConfig defaultSymbol)
        {
            DefaultTimeFrame = defaultTimeFrame;
            DefaultSymbol = new SymbolToken(defaultSymbol.Name, defaultSymbol.Origin);
        }
    }

    public class AlgoSetupMetadata : IAlgoSetupMetadata
    {
        public IReadOnlyList<ISetupSymbolInfo> Symbols { get; }

        public MappingCollection Mappings { get; }

        public IPluginIdProvider IdProvider { get; }


        public AlgoSetupMetadata(IEnumerable<SymbolInfo> symbols, MappingCollection mappings)
        {
            Symbols = symbols.Select(s => new SymbolToken(s.Name)).ToList();
            Mappings = mappings;
            IdProvider = new IdProviderStub();
        }


        private class IdProviderStub : IPluginIdProvider
        {
            public string GeneratePluginId(PluginDescriptor descriptor)
            {
                return descriptor.Id;
            }

            public bool IsValidPluginId(AlgoTypes pluginType, string pluginId)
            {
                return true;
            }

            public void RegisterPluginId(string pluginId)
            {
            }
        }
    }
}
