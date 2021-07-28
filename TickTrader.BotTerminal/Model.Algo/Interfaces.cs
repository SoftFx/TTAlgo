using Machinarium.Qnil;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

using AlgoServerPublicApi = TickTrader.Algo.Server.PublicAPI;


namespace TickTrader.BotTerminal
{
    internal interface IAlgoAgent
    {
        string Name { get; }

        bool IsRemote { get; }

        IVarSet<string, PackageInfo> Packages { get; }

        IVarSet<PluginKey, PluginInfo> Plugins { get; }

        IVarSet<string, AccountModelInfo> Accounts { get; }

        IVarSet<string, ITradeBot> Bots { get; }

        PluginCatalog Catalog { get; }

        IPluginIdProvider IdProvider { get; }

        bool SupportsAccountManagement { get; }

        AlgoServerPublicApi.IAccessManager AccessManager { get; }

        IAlertModel AlertModel { get; }


        event Action<PackageInfo> PackageStateChanged;
        event Action<AccountModelInfo> AccountStateChanged;
        event Action<ITradeBot> BotStateChanged;
        event Action<ITradeBot> BotUpdated;
        event Action AccessLevelChanged;


        Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext);

        Task StartBot(string botId);

        Task StopBot(string botId);

        Task AddBot(string accountId, PluginConfig config);

        Task RemoveBot(string botId, bool cleanLog, bool cleanAlgoData);

        Task ChangeBotConfig(string botId, PluginConfig newConfig);

        Task AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        Task UploadPackage(string fileName, string srcFilePath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task RemovePackage(string packageId);

        Task DownloadPackage(string packageId, string dstFilePath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName);

        Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, AlgoServerPublicApi.IFileProgressListener progressListener);
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

        void InitializePlugin(ExecutorConfig config);
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
