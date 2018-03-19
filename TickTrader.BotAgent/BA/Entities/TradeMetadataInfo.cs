using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.BotAgent.BA.Models;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotAgent.BA.Entities
{
    public class TradeMetadataInfo
    {
        public TradeMetadataInfo(AccountTypes accType, List<SymbolEntity> symbolsCopy, List<CurrencyEntity> currenciesCopy)
        {
            AccountType = accType;
            Symbols = symbolsCopy;
            Currencies = currenciesCopy;
        }

        public AccountTypes AccountType { get; }

        public IReadOnlyList<SymbolEntity> Symbols { get; }
        public IReadOnlyList<CurrencyEntity> Currencies { get; }
    }
}
