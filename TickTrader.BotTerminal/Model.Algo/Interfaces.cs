using Machinarium.Qnil;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Protocol;
using TickTrader.Algo.Core.Repository;
using System.Collections.Generic;
using SciChart.Charting.Visuals.Axes;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.BotTerminal
{
    internal interface IAlgoAgent
    {
        string Name { get; }

        bool IsRemote { get; }

        IVarSet<PackageKey, PackageInfo> Packages { get; }

        IVarSet<PluginKey, PluginInfo> Plugins { get; }

        IVarSet<AccountKey, AccountModelInfo> Accounts { get; }

        IVarSet<string, ITradeBot> Bots { get; }

        PluginCatalog Catalog { get; }

        IPluginIdProvider IdProvider { get; }

        bool SupportsAccountManagement { get; }

        AccessManager AccessManager { get; }


        event Action<PackageInfo> PackageStateChanged;
        event Action<AccountModelInfo> AccountStateChanged;
        event Action<ITradeBot> BotStateChanged;
        event Action<ITradeBot> BotUpdated;
        event Action AccessLevelChanged;


        Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext);

        Task StartBot(string botId);

        Task StopBot(string botId);

        Task AddBot(AccountKey account, PluginConfig config);

        Task RemoveBot(string botId, bool cleanLog, bool cleanAlgoData);

        Task ChangeBotConfig(string botId, PluginConfig newConfig);

        Task AddAccount(AccountKey account, string password);

        Task RemoveAccount(AccountKey account);

        Task ChangeAccount(AccountKey account, string password);

        Task<ConnectionErrorInfo> TestAccount(AccountKey account);

        Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password);

        Task UploadPackage(string fileName, string srcFilePath, IFileProgressListener progressListener);

        Task RemovePackage(PackageKey package);

        Task DownloadPackage(PackageKey package, string dstFilePath, IFileProgressListener progressListener);

        Task<BotFolderInfo> GetBotFolderInfo(string botId, BotFolderId folderId);

        Task ClearBotFolder(string botId, BotFolderId folderId);

        Task DeleteBotFile(string botId, BotFolderId folderId, string fileName);

        Task DownloadBotFile(string botId, BotFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener);

        Task UploadBotFile(string botId, BotFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener);
    }

    internal interface IExecStateObservable
    {
        bool IsStarted { get; }

        event Action StartEvent;
        event AsyncEventHandler StopEvent;
    }

    internal interface IPluginDataChartModel : IExecStateObservable
    {
        ITimeVectorRef TimeSyncRef { get; }

        AxisBase CreateXAxis();
    }

    internal interface IAlgoPluginHost : IPluginDataChartModel
    {
        void Lock();
        void Unlock();

        void InitializePlugin(PluginExecutor plugin);
        void UpdatePlugin(PluginExecutor plugin);

        ITradeExecutor GetTradeApi();
        ITradeHistoryProvider GetTradeHistoryApi();
        string GetConnectionInfo();

        event Action ParamsChanged;
        event Action Connected;
        event Action Disconnected;

        //event Action<PluginCatalogItem> PluginBeingReplaced; // fired on background thread!
        //event Action<PluginCatalogItem> PluginBeingRemoved; // fired on background thread!
    }

    internal interface IPluginModel
    {
        string InstanceId { get; }

        IDictionary<string, IOutputCollector> Outputs { get; }

        event Action OutputsChanged;
    }
}
