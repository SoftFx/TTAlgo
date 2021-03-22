using Machinarium.Qnil;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.ServerControl;
using System.Collections.Generic;
using SciChart.Charting.Visuals.Axes;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal interface IAlgoAgent
    {
        string Name { get; }

        bool IsRemote { get; }

        IVarSet<string, PackageInfo> Packages { get; }

        IVarSet<PluginKey, PluginInfo> Plugins { get; }

        IVarSet<AccountKey, AccountModelInfo> Accounts { get; }

        IVarSet<string, ITradeBot> Bots { get; }

        PluginCatalog Catalog { get; }

        IPluginIdProvider IdProvider { get; }

        bool SupportsAccountManagement { get; }

        AccessManager AccessManager { get; }

        IAlertModel AlertModel { get; }


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

        Task RemovePackage(string packageId);

        Task DownloadPackage(string packageId, string dstFilePath, IFileProgressListener progressListener);

        Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName);

        Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener);

        Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener);
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

        void InitializePlugin(ExecutorModel runtime);
        void UpdatePlugin(ExecutorModel runtime);
        void EnqueueStartAction(Action action);

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
