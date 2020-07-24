using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public interface ISetupSymbolInfo
    {
        string Name { get; }

        SymbolOrigin Origin { get; }

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
        TimeFrames DefaultTimeFrame { get; }

        ISetupSymbolInfo DefaultSymbol { get; }

        MappingKey DefaultMapping { get; }
    }


    public interface IPluginIdProvider
    {
        string GeneratePluginId(PluginDescriptor descriptor);

        bool IsValidPluginId(AlgoTypes pluginType, string pluginId);

        void RegisterPluginId(string pluginId);
    }
}
