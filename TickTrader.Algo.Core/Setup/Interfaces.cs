using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Setup
{
    public interface ISetupMetadata
    {
        string MainSymbol { get; }
        bool SymbolExist(string symbolCode);
    }
}
