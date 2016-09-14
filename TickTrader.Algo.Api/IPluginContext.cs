using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IPluginContext
    {
        FeedProvider GetFeed();
        AccountDataProvider GetAccountDataProvider();
        SymbolProvider GetSymbolProvider();
        IPluginMonitor GetPluginLogger();
        TradeCommands GetTradeApi();
        StatusApi GetStatusApi();
        void OnExit();
    }

    internal interface IPluginMonitor
    {
        void Print(string entry, object[] parameters);
        void PrintError(string entry, object[] parameters);
    }

    internal interface IPluginActivator
    {
        IPluginContext Activate(AlgoPlugin instance);
    }
}
