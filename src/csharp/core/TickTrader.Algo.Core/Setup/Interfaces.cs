using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Setup
{
    public interface ISetupSymbolInfo
    {
        string Name { get; }

        SymbolConfig.Types.SymbolOrigin Origin { get; }

        string Id { get; }
    }


    public interface IAlgoSetupMetadata
    {
        IReadOnlyList<ISetupSymbolInfo> Symbols { get; }

        MappingCollectionInfo Mappings { get; }

        IPluginIdProvider IdProvider { get; }
    }


    public interface IAlgoSetupContext
    {
        Feed.Types.Timeframe DefaultTimeFrame { get; }

        ISetupSymbolInfo DefaultSymbol { get; }

        MappingKey DefaultMapping { get; }
    }


    public interface IPluginIdProvider
    {
        string GeneratePluginId(PluginDescriptor descriptor);

        bool IsValidPluginId(Metadata.Types.PluginType pluginType, string pluginId);

        void RegisterPluginId(string pluginId);
    }
}
