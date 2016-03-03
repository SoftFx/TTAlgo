using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Symbol
    {
        string Id { get; }
        int Digits { get; }
    }

    public interface SymbolList : IReadOnlyList<Symbol>
    {

    }
}
