using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Common.Model.Setup
{
    internal class DummySymbolInfo : ISymbolInfo
    {
        public string Name { get; }


        public DummySymbolInfo(string symbol)
        {
            Name = symbol;
        }
    }


    public static class AlgoSetupMetadataExtensions
    {
        public static IReadOnlyList<ISymbolInfo> GetAvaliableSymbols(this IAlgoSetupMetadata metadata, string defaultSymbolCode)
        {
            var symbols = metadata?.Symbols;
            if ((symbols?.Count ?? 0) == 0)
                symbols = new List<ISymbolInfo> { new DummySymbolInfo(defaultSymbolCode) };
            return symbols;
        }

        public static ISymbolInfo GetSymbolOrAny(this IReadOnlyList<ISymbolInfo> availableSymbols, string symbolCode)
        {
            return availableSymbols.FirstOrDefault(s => s.Name == symbolCode) ?? availableSymbols.First();
        }
    }
}
