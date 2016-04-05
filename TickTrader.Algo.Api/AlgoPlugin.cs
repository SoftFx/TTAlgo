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

        private IPluginDataProvider context;
        private SymbolProvider symbolProvider;
        private MarketDataProvider marketDataProvider;
        private AccountDataProvider accountDataProvider;

        internal AlgoPlugin()
        {
            if (activator == null)
                throw new InvalidOperationException("Algo context is not initialized!");

            this.context = activator.Activate(this);
        }

        protected AccountDataProvider Account
        {
            get
            {
                if (accountDataProvider == null)
                    accountDataProvider = context.GetAccountDataProvider();
                return accountDataProvider;
            }
        }

        protected MarketDataProvider MarketSeries
        {
            get
            {
                if (marketDataProvider == null)
                    marketDataProvider = context.GetMarketDataProvider();
                return marketDataProvider;
            }
        }

        protected SymbolProvider Symbols
        {
            get
            {
                if (symbolProvider == null)
                    symbolProvider = context.GetSymbolProvider();
                return symbolProvider;
            }
        }

        protected virtual void Init() { }

        internal void InvokeInit()
        {
            Init();
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
