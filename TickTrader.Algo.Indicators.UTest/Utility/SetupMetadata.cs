using System.Collections.Generic;
using TickTrader.Algo.Core.Setup;

namespace TickTrader.Algo.Indicators.UTest.Utility
{
    public class SetupMetadata : ISetupMetadata
    {
        protected HashSet<string> Symbols = new HashSet<string>();

        public SetupMetadata(string mainSymbol)
        {
            MainSymbol = mainSymbol;
            Symbols.Add(MainSymbol);
        }

        public SetupMetadata(string mainSymbol, IEnumerable<string> symbols)
        {
            MainSymbol = mainSymbol;
            foreach (var symbol in symbols)
            {
                AddSymbol(symbol);
            }
        }

        public string MainSymbol { get; protected set; }

        public bool AddSymbol(string symbolCode)
        {
            if (SymbolExist(symbolCode)) return false;
            Symbols.Add(symbolCode);
            return true;
        }

        public bool SymbolExist(string symbolCode)
        {
            return Symbols.Contains(symbolCode);
        }
    }
}
