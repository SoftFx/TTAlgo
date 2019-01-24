using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public abstract class AlgoPlugin
    {
        [ThreadStatic]
        internal static IPluginActivator activator;
        [ThreadStatic]
        internal static IPluginContext staticContext;

        internal IPluginContext context;

        internal AlgoPlugin()
        {
            if (activator == null)
                throw new Exception("Context is missing!");

            context = activator.Activate(this);
        }

        public AccountDataProvider Account { get { return context.AccountData; } }
        public DiagnosticInfo Diagnostics { get { return context.Diagnostics; } }
        public CustomFeedProvider Feed { get { return context.Feed.CustomCommds; } }
        public EnvironmentInfo Enviroment { get { return context.Environment; } }
        public SymbolList Symbols { get { return context.Symbols.List; } }
        public CurrencyList Currencies { get { return context.Currencies; } }
        public string Id { get { return context.InstanceId; } }
        public Symbol Symbol { get { return context.Symbols.MainSymbol; } }
        public BarSeries Bars { get { return context.Feed.Bars; } }
        public double Bid { get { return Symbol.Bid; } }
        public double Ask { get { return Symbol.Ask; } }
        public TimeFrames TimeFrame { get { return context.TimeFrame; } }
        public IndicatorProvider Indicators { get { return context.Indicators; } }

        /// <summary>
        /// Occurs when connection to server is lost.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Occurs when connection to server is restored.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;

        protected virtual void Init() { }

        internal void InvokeInit()
        {
            Init();
        }

        internal void InvokeConnected(ConnectedEventArgs args)
        {
            Connected?.Invoke(this, args);
        }

        internal void InvokeDisconnected(DisconnectedEventArgs args)
        {
            Disconnected?.Invoke(this, args);
        }

        public void OnMainThread(Action action)
        {
            context.OnPluginThread(action);
        }

        public void BeginOnMainThread(Action action)
        {
            context.OnPluginThread(action);
        }

        public Task OnMainThreadAsync(Action action)
        {
            return context.OnPluginThreadAsync(action);
        }
    }

    public class ConnectedEventArgs : EventArgs
    {
    }

    public class DisconnectedEventArgs : EventArgs
    {
    }
}
