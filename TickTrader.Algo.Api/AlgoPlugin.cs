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
        private static IPluginContext context;

        internal AlgoPlugin()
        {
            if (activator == null)
                throw new Exception("Context is missing!");

            activator.Activate(this);
        }

        internal static IPluginContext Context { get { return context; } set { context = value; } }

        public AccountDataProvider Account { get { return context.AccountData; } }
        public DiagnosticInfo Diagnostics { get { return context.Diagnostics; } }
        public CustomFeedProvider Feed { get { return context.Feed.CustomCommds; } }
        public EnvironmentInfo Enviroment { get { return context.Environment; } }
        public SymbolList Symbols { get { return context.Symbols.List; } }
        public Symbol Symbol { get { return context.Symbols.MainSymbol; } }
        public BarSeries Bars { get { return context.Feed.Bars; } }
        public double Bid { get { return Symbol.Bid; } }
        public double Ask { get { return Symbol.Ask; } }

        protected virtual void Init() { }

        internal void InvokeInit()
        {
            Init();
        }
    }
}
