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

        private IPluginContext context;
        private SymbolProvider symbolProvider;
        private FeedProvider feed;
        private AccountDataProvider accountDataProvider;
        private IPluginMonitor monitor;
        private TradeCommands tradeCmdApi;
        private StatusApi status;

        internal AlgoPlugin()
        {
            if (activator != null)
                this.context = activator.Activate(this);
        }

        internal IPluginContext Context { get { return context; } }

        protected AccountDataProvider Account
        {
            get
            {
                if (accountDataProvider == null)
                    accountDataProvider = context.GetAccountDataProvider();
                return accountDataProvider;
            }
        }

        protected CustomFeedProvider Feed { get { return GetFeedProvider().CustomCommds; } }
        protected SymbolList Symbols { get { return GetSymbolProvider().List; } }
        protected Symbol Symbol { get { return GetSymbolProvider().MainSymbol; } }
        protected BarSeries Bars { get { return GetFeedProvider().Bars; } }
        protected double Bid { get { return Symbol.Bid; } }
        protected double Ask { get { return Symbol.Ask; } }

        protected virtual void Init() { }

        internal void InvokeInit()
        {
            Init();
        }

        internal IPluginMonitor GetLogger()
        {
            if (monitor == null)
                monitor = context.GetPluginLogger();
            return monitor;
        }

        internal TradeCommands GetTradeApi()
        {
            if (tradeCmdApi == null)
                tradeCmdApi = context.GetTradeApi();
            return tradeCmdApi;
        }

        internal SymbolProvider GetSymbolProvider()
        {
            if (symbolProvider == null)
                symbolProvider = context.GetSymbolProvider();
            return symbolProvider;
        }

        internal StatusApi GetStatusApi()
        {
            if (status == null)
                status = context.GetStatusApi();
            return status;
        }

        internal FeedProvider GetFeedProvider()
        {
            if (feed == null)
                feed = context.GetFeed();
            return feed;
        }

        //internal virtual void DoInit()
        //{
        //    currentInstance = this;

        //    try
        //    {
        //        Init();

        //        foreach (Indicator i in nestedIndicators)
        //            i.DoInit();
        //    }
        //    finally
        //    {
        //        currentInstance = null;
        //    }
        //}
    }
}
