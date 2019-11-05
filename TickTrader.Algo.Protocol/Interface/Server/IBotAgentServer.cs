using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Protocol
{
    public interface IBotAgentServer
    {
        #region Credentials

        event Action AdminCredsChanged;

        event Action DealerCredsChanged;

        event Action ViewerCredsChanged;

        AccessLevels ValidateCreds(string login, string password);

        #endregion Credentials


        #region Initialization

        List<PackageInfo> GetPackageList();

        List<AccountModelInfo> GetAccountList();

        List<BotModelInfo> GetBotList();

        #endregion Initialization


        #region Updates

        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<AccountModelInfo>> AccountUpdated;

        event Action<UpdateInfo<BotModelInfo>> BotUpdated;

        event Action<PackageInfo> PackageStateUpdated;

        event Action<BotModelInfo> BotStateUpdated;

        event Action<AccountModelInfo> AccountStateUpdated;

        #endregion Updates


        #region Requests

        ApiMetadataInfo GetApiMetadata();

        MappingCollectionInfo GetMappingsInfo();

        SetupContextInfo GetSetupContext();

        AccountMetadataInfo GetAccountMetadata(AccountKey account);

        void StartBot(string botId);

        void StopBot(string botId);

        void AddBot(AccountKey account, PluginConfig config);

        void RemoveBot(string botId, bool cleanLog, bool cleanAlgoData);

        void ChangeBotConfig(string botId, PluginConfig newConfig);

        void AddAccount(AccountKey account, string password);

        void RemoveAccount(AccountKey account);

        void ChangeAccount(AccountKey account, string password);

        ConnectionErrorInfo TestAccount(AccountKey account);

        ConnectionErrorInfo TestAccountCreds(AccountKey account, string password);

        void RemovePackage(PackageKey package);

        string GetPackageReadPath(PackageKey package);

        string GetPackageWritePath(PackageKey package);

        string GetBotStatus(string botId);

        Task<string> GetBotStatusAsync(string botId);

        LogRecordInfo[] GetBotLogs(string botId, DateTime lastLogTimeUtc, int maxCount);

        Task<LogRecordInfo[]> GetBotLogsAsync(string botId, DateTime lastLogTimeUtc, int maxCount, bool alert);

        BotFolderInfo GetBotFolderInfo(string botId, BotFolderId folderId);

        void ClearBotFolder(string botId, BotFolderId folderId);

        void DeleteBotFile(string botId, BotFolderId folderId, string fileName);

        string GetBotFileReadPath(string botId, BotFolderId folderId, string fileName);

        string GetBotFileWritePath(string botId, BotFolderId folderId, string fileName);

        #endregion Requests
    }
}
