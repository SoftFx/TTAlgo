using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public class AccountMetadataInfo
    {
        public AccountKey Id { get; set; }

        public List<SymbolInfo> Symbols { get; set; }
    }
}
