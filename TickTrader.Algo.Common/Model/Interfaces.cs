using Machinarium.Qnil;
using System;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Lib;
using Machinarium.Var;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public interface ISymbolManager : IVarSet<string, ISymbolInfo>
    {
        IFeedSubscription SubscribeAll();
    }

    public interface IActionObserver
    {
        void StartIndeterminateProgress();
        void StartProgress(double min, double max);
        void SetProgress(double val);
        void SetMessage(string message);
    }

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
