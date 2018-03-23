using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public enum AccountTypeInfo { Unknown, Gross, Net, Cash };


    public class AccountMetadataInfo
    {
        public AccountKey Id { get; set; }

        public AccountTypeInfo Type { get; set; }

        public List<SymbolInfo> Symbols { get; set; }

        public List<CurrencyInfo> Currencies { get; set; }
    }
}
