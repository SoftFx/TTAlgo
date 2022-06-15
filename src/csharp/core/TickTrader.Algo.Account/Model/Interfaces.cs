using Machinarium.Qnil;
using Machinarium.Var;
using System;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public interface IConnectionStatusInfo
    {
        bool IsConnecting { get; }
        BoolVar IsConnected { get; }

        event AsyncEventHandler Initializing;
        event Action IsConnectingChanged;
        event Action Connected;
        event AsyncEventHandler Deinitializing;
        event Action Disconnected;
    }

    public interface IMarketDataProvider
    {
        IVarSet<string, SymbolInfo> Symbols { get; }
        IVarSet<string, CurrencyInfo> Currencies { get; }
        QuoteDistributor2 Distributor { get; }
    }
}
