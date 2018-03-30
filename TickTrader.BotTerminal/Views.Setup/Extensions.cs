using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    public static class AccountMetadataInfoExtensions
    {
        public static IReadOnlyList<SymbolInfo> GetAvaliableSymbols(this AccountMetadataInfo metadata, string defaultSymbolCode)
        {
            var symbols = metadata?.Symbols;
            if ((symbols?.Count ?? 0) == 0)
                symbols = new List<SymbolInfo> { new SymbolInfo(defaultSymbolCode) };
            return symbols;
        }

        public static SymbolInfo GetSymbolOrAny(this IReadOnlyList<SymbolInfo> availableSymbols, string symbolCode)
        {
            return availableSymbols.FirstOrDefault(s => s.Name == symbolCode) ?? availableSymbols.First();
        }
    }
}
