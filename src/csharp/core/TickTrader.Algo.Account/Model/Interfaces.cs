using Machinarium.Qnil;
using System;
using Machinarium.Var;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Core.Lib;

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
        QuoteDistributor Distributor { get; }
    }
}
