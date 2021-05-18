using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class SymbolInfoExtensions
    {
        public static SymbolConfig ToConfig(this ISetupSymbolInfo symbol)
        {
            return new SymbolConfig { Name = symbol.Name, Origin = symbol.Origin };
        }

        public static SymbolKey ToInfo(this ISetupSymbolInfo symbol)
        {
            return new SymbolKey(symbol.Name, symbol.Origin);
        }

        public static SymbolKey ToKey(this SymbolInfo info)
        {
            return new SymbolKey(info.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }

        public static SymbolKey ToKey(this SymbolConfig info)
        {
            return new SymbolKey(info.Name, SymbolConfig.Types.SymbolOrigin.Online);
        }
    }
}
