using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common
{
    public static class SymbolInfoExtencions
    {
        public static SymbolKey ToKey(this SymbolInfo info) => new SymbolKey(info.Name, SymbolOrigin.Online);
    }
}
