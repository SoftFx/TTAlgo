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
        EnvironmentInfo GetEnvironment();
        IHelperApi GetHelper();
        void OnExit();
    }

    internal interface IHelperApi
    {
        Quote CreateQuote(string symbol, DateTime time, IEnumerable<BookEntry> bids, IEnumerable<BookEntry> asks);
        BookEntry CreateBookEntry(double price, double volume);
        IEnumerable<BookEntry> CreateBook(IEnumerable<double> prices, IEnumerable<double> volumes);
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
