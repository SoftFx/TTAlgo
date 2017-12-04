using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.BotAgent.BA.Models;

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

        public AccountType AccountType { get; }

        public IReadOnlyList<SymbolInfo> Symbols { get; }
        public IReadOnlyList<CurrencyInfo> Currencies { get; }
    }
}
