using System.Linq;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Account.Fdk2
{
    public static class SfxConvert
    {
        internal static FDK2.SymbolEntry[] GetSymbolEntries(string[] symbolIds, int marketDepth)
        {
            return symbolIds.Select(id => new FDK2.SymbolEntry { Id = id, MarketDepth = (ushort)marketDepth }).ToArray();
        }
    }
}
