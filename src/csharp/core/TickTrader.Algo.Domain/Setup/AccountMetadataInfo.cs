using System.Collections.Generic;

namespace TickTrader.Algo.Domain
{
    public partial class AccountMetadataInfo
    {
        public AccountMetadataInfo(AccountKey account, List<SymbolConfig> symbols, SymbolConfig defaultSymbol)
        {
            Key = account;
            Symbols.AddRange(symbols);
            DefaultSymbol = defaultSymbol;
        }
    }
}
