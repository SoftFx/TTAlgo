using System.Linq;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model.Interop
{
    public static class SfxConvert
    {
        internal static FDK2.SymbolEntry[] GetSymbolEntries(string[] symbolIds, int marketDepth)
        {
            return symbolIds.Select(id => new FDK2.SymbolEntry { Id = id, MarketDepth = (ushort)marketDepth }).ToArray();
        }
    }
}
