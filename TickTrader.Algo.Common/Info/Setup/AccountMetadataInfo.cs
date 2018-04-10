using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public class AccountMetadataInfo
    {
        public AccountKey Key { get; set; }

        public List<SymbolInfo> Symbols { get; set; }


        public AccountMetadataInfo() { }

        public AccountMetadataInfo(AccountKey key, List<SymbolInfo> symbols)
        {
            Key = key;
            Symbols = symbols;
        }
    }
}
