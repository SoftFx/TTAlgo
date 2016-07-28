using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IPluginContext
    {
        MarketDataProvider GetMarketDataProvider();
        AccountDataProvider GetAccountDataProvider();
        SymbolProvider GetSymbolProvider();
        IPluginMonitor GetPluginLogger();
        ITradeCommands GetTradeApi();
        void OnExit();
    }

    internal interface IPluginMonitor
    {
        void UpdateStatus(string status);
        void WriteLog(string entry, object[] parameters);
    }

    internal interface IPluginActivator
    {
        IPluginContext Activate(AlgoPlugin instance);
    }
}
