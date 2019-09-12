using Machinarium.Qnil;
using System;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Lib;
using Machinarium.Var;
using TickTrader.Algo.Core.Infrastructure;

namespace TickTrader.Algo.Common.Model
{
    public interface ISyncContext
    {
        void Invoke(Action syncAction);
        void Invoke<T>(Action<T> syncAction, T args);
        T Invoke<T>(Func<T> syncFunc);
        TOut Invoke<TIn, TOut>(Func<TIn, TOut> syncFunc, TIn args);
        void Send(Action asyncAction);
    }

    public interface ISyncChannel<T>
    {
        void Send(object data);
    }

    public interface ISymbolManager : IVarSet<string, SymbolModel>
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
        IVarSet<string, SymbolModel> Symbols { get; }
        IVarSet<string, CurrencyEntity> Currencies { get; }
        QuoteDistributor Distributor { get; }
    }
}
