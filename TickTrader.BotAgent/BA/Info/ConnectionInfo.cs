using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.BotAgent.BA.Models;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotAgent.BA.Info
{
    public class ConnectionInfo
    {
        public ConnectionInfo(ClientModel client)
        {
            AccountType = client.Account.Type.Value;
            Symbols = client.Symbols.Snapshot.Values.Select(s => s.Descriptor).ToList();
            Currencies = client.Currencies.Values.ToList();
        }

        public AccountTypes AccountType { get; }

        public IReadOnlyList<SymbolEntity> Symbols { get; }
        public IReadOnlyList<CurrencyEntity> Currencies { get; }
    }
}
