using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
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


    public class StubSymbolInfo : ISymbolInfo
    {
        public string Name { get; set; }


        public StubSymbolInfo(SymbolEntity symbolEntity)
        {
            Name = symbolEntity.Name;
        }
    }


    // stub
    public class SetupMetadata : IAlgoSetupMetadata
    {
        public IReadOnlyList<ISymbolInfo> Symbols { get; }

        public SymbolMappingsCollection SymbolMappings { get; }

        public IPluginIdProvider IdProvider { get; }

        public SetupMetadata(IEnumerable<SymbolEntity> symbols)
        {
            //Extentions = new ExtCollection();
            Symbols = symbols.Select(s => new StubSymbolInfo(s)).ToList();
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
