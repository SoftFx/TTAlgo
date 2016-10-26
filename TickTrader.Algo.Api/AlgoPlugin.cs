using System;
using System.Collections.Generic;

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
        private EnvironmentInfo env;
        private IHelperApi helper;

        internal AlgoPlugin()
        {
            if (activator != null)
                this.context = activator.Activate(this);
        }

        internal IPluginContext Context { get { return context; } }

        public AccountDataProvider Account
        {
            get
            {
                if (accountDataProvider == null)
                    accountDataProvider = context.GetAccountDataProvider();
                return accountDataProvider;
            }
        }

        public CustomFeedProvider Feed { get { return GetFeedProvider().CustomCommds; } }
        public EnvironmentInfo Enviroment { get { return GetEnvInfoProvider(); } }
        public SymbolList Symbols { get { return GetSymbolProvider().List; } }
        public Symbol Symbol { get { return GetSymbolProvider().MainSymbol; } }
        public BarSeries Bars { get { return GetFeedProvider().Bars; } }
        public double Bid { get { return Symbol.Bid; } }
        public double Ask { get { return Symbol.Ask; } }

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

        internal EnvironmentInfo GetEnvInfoProvider()
        {
            if (env == null)
                env = context.GetEnvironment();
            return env;
        }

        internal IHelperApi GetHelper()
        {
            if (helper == null)
                helper = context.GetHelper();
            return helper;
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

        public Quote CreateQuote(string symbol, DateTime time, IEnumerable<BookEntry> bids, IEnumerable<BookEntry> asks)
        {
            return GetHelper().CreateQuote(symbol, time, bids, asks);
        }

        public BookEntry CreateBookEntry(double price, double volume)
        {
            return GetHelper().CreateBookEntry(price, volume);
        }

        public IEnumerable<BookEntry> CreateBook(IEnumerable<double> prices, IEnumerable<double> volumes)
        {
            return GetHelper().CreateBook(prices, volumes);
        }
    }
}
