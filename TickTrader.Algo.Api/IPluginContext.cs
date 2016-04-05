using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IPluginDataProvider
    {
        MarketDataProvider GetMarketDataProvider();
        AccountDataProvider GetAccountDataProvider();
        SymbolProvider GetSymbolProvider();
    }

    internal interface IPluginActivator
    {
        IPluginDataProvider Activate(AlgoPlugin instance);
    }
}
