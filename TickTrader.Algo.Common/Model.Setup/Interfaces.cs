using System.Collections.Generic;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
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

        MappingCollection Mappings { get; }

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

        bool IsValidPluginId(Domain.Metadata.Types.PluginType pluginType, string pluginId);

        void RegisterPluginId(string pluginId);
    }
}
