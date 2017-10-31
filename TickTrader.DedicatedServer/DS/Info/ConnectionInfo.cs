using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.DedicatedServer.DS.Models;

namespace TickTrader.DedicatedServer.DS.Info
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
