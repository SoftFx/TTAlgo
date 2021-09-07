using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public static class InfoExtensions
    {
        public static SymbolConfig ToInfo(this ISymbolInfo symbol)
        {
            return new SymbolConfig(symbol.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
