using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Library;
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

        MappingCollection SymbolMappings { get; }

        IPluginIdProvider IdProvider { get; }
    }


    public interface IAlgoSetupContext
    {
        TimeFrames DefaultTimeFrame { get; }

        string DefaultSymbolCode { get; }

        string DefaultMapping { get; }
    }


    public interface IPluginIdProvider
    {
        string GeneratePluginId(PluginDescriptor descriptor);

        bool IsValidPluginId(PluginDescriptor descriptor, string pluginId);
    }
}
