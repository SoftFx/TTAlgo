using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotAgent.BA.Models
{
    public class IdProviderStub : IPluginIdProvider
    {
        public string GeneratePluginId(AlgoPluginDescriptor descriptor)
        {
            return descriptor.UserDisplayName;
        }

        public bool IsValidPluginId(AlgoPluginDescriptor descriptor, string pluginId)
        {
            return true;
        }
    }


    // stub
    public class SetupMetadata : IAlgoSetupMetadata
    {
        public IReadOnlyList<ISymbolInfo> Symbols { get; }

        public SymbolMappingsCollection SymbolMappings { get; }

        public IPluginIdProvider IdProvider { get; }

        public SetupMetadata(IEnumerable<ISymbolInfo> symbols)
        {
            //Extentions = new ExtCollection();
            Symbols = symbols.ToList();
            SymbolMappings = new SymbolMappingsCollection(null);
            IdProvider = new IdProviderStub();
        }
    }


    // stub
    public class SetupContext : IAlgoSetupContext
    {
        public TimeFrames DefaultTimeFrame => TimeFrames.M1;

        public string DefaultSymbolCode => "EURUSD";

        public string DefaultMapping => "Bid";
    }
}
