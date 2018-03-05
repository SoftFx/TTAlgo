using System.Collections.Generic;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public interface ISymbolInfo
    {
        string Name { get; }
    }


    public interface IAlgoSetupMetadata
    {
        IReadOnlyList<ISymbolInfo> Symbols { get; }

        SymbolMappingsCollection SymbolMappings { get; }

        IPluginIdProvider IdProvider { get; }
    }

    public interface IPluginIdProvider
    {
        string GeneratePluginId(AlgoPluginDescriptor descriptor);

        bool IsValidPluginId(AlgoPluginDescriptor descriptor, string pluginId);
    }
}
