using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public class AccountMetadataInfo
    {
        public AccountKey Key { get; set; }

        public List<SymbolKey> Symbols { get; set; }

        public SymbolKey DefaultSymbol { get; set; }


        public AccountMetadataInfo()
        {
            Symbols = new List<SymbolKey>();
        }

        public AccountMetadataInfo(AccountKey key, List<SymbolKey> symbols, SymbolKey defaultSymbol)
        {
            Key = key;
            Symbols = symbols;
            DefaultSymbol = defaultSymbol;
        }
    }
}
