using System.Collections.Generic;

namespace TickTrader.Algo.Domain
{
    public partial class AccountMetadataInfo
    {
        public AccountMetadataInfo(string accountId, List<SymbolConfig> symbols, SymbolConfig defaultSymbol)
        {
            AccountId = accountId;
            Symbols.AddRange(symbols);
            DefaultSymbol = defaultSymbol;
        }
    }
}
