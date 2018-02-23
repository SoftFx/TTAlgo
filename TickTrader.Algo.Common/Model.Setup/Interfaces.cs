using System.Collections.Generic;

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
    }
}
