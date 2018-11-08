using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public interface ISymbolInfo
    {
        string Name { get; }

        SymbolOrigin Origin { get; }

        string Id { get; }
    }


    public interface IAlgoSetupMetadata
    {
        IReadOnlyList<ISymbolInfo> Symbols { get; }

        MappingCollection Mappings { get; }

        IPluginIdProvider IdProvider { get; }
    }


    public interface IAlgoSetupContext
    {
        TimeFrames DefaultTimeFrame { get; }

        ISymbolInfo DefaultSymbol { get; }

        MappingKey DefaultMapping { get; }
    }


    public interface IPluginIdProvider
    {
        string GeneratePluginId(PluginDescriptor descriptor);

        bool IsValidPluginId(PluginDescriptor descriptor, string pluginId);
    }
}
