using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

using AlgoServerPublicApi = TickTrader.Algo.Server.PublicAPI;


namespace TickTrader.BotTerminal
{
    internal interface ITradeBot
    {
        bool IsRemote { get; }

        string InstanceId { get; }

        PluginConfig Config { get; }

        PluginModelInfo.Types.PluginState State { get; }

        string FaultMessage { get; }

        PluginDescriptor Descriptor { get; }

        string Status { get; }

        BotJournal Journal { get; }

        string AccountId { get; }


        event Action<ITradeBot> Updated;
        event Action<ITradeBot> StateChanged;
        event Action<ITradeBot> StatusChanged;


        void SubscribeToStatus();

        void UnsubscribeFromStatus();

        void SubscribeToLogs();

        void UnsubscribeFromLogs();
    }


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

        AlgoServerPublicApi.IVersionSpec VersionSpec { get; }

        AlertManagerModel AlertModel { get; }

        ServerVersionInfo CurrentVersion { get; }

        UpdateServiceInfo UpdateSvcInfo { get; }


        event Action<PackageInfo> PackageStateChanged;
        event Action<AccountModelInfo> AccountStateChanged;
        event Action<ITradeBot> BotStateChanged;
        event Action<ITradeBot> BotUpdated;
        event Action AccessLevelChanged;
        event Action UpdateServiceStateChanged;


        Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext);

        Task StartBot(string botId);

        Task StopBot(string botId);

        Task AddBot(string accountId, PluginConfig config);

        Task RemoveBot(string botId, bool cleanLog, bool cleanAlgoData);

        Task ChangeBotConfig(string botId, PluginConfig newConfig);

        Task AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<AlgoServerPublicApi.ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<AlgoServerPublicApi.ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        Task UploadPackage(string fileName, string srcFilePath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task RemovePackage(string packageId);

        Task DownloadPackage(string packageId, string dstFilePath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName);

        Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, AlgoServerPublicApi.IFileProgressListener progressListener);

        Task<ServerUpdateList> GetServerUpdateList(bool forced);

        Task<StartServerUpdateResponse> StartServerUpdate(string releaseId);

        Task<StartServerUpdateResponse> StartServerUpdateFromFile(string version, string srcPath);

        Task DiscardServerUpdateResult();
    }
}
