using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public static class InfoExtensions
    {
        public static SymbolConfig ToInfo(this ISymbolInfo symbol)
        {
            return new SymbolConfig(symbol.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
