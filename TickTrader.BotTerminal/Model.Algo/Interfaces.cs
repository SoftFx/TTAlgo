using Machinarium.Qnil;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal interface IAlgoAgent
    {
        string Name { get; }

        IVarList<AccountKey> Accounts { get; }

        IAlgoLibrary Library { get; }

        PluginCatalog Catalog { get; }

        IPluginIdProvider IdProvider { get; }


        event Action<BotModelInfo> BotStateChanged;


        Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext);

        Task<bool> AddOrUpdatePlugin(PluginConfig config, bool start);
    }


    internal interface IAlgoPluginHost
    {
        void Lock();
        void Unlock();

        bool IsStarted { get; }
        void InitializePlugin(PluginExecutor plugin);
        void UpdatePlugin(PluginExecutor plugin);

        ITradeExecutor GetTradeApi();
        ITradeHistoryProvider GetTradeHistoryApi();
        BotJournal Journal { get; }

        event Action ParamsChanged;
        event Action Connected;
        event Action StartEvent;
        event AsyncEventHandler StopEvent;

        //event Action<PluginCatalogItem> PluginBeingReplaced; // fired on background thread!
        //event Action<PluginCatalogItem> PluginBeingRemoved; // fired on background thread!
    }
}
