using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Domain;

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

        Task<List<PackageInfo>> GetPackageList();

        Task<List<AccountModelInfo>> GetAccountList();

        Task<List<BotModelInfo>> GetBotList();

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

        Task<ApiMetadataInfo> GetApiMetadata();

        Task<MappingCollectionInfo> GetMappingsInfo();

        Task<SetupContextInfo> GetSetupContext();

        Task<AccountMetadataInfo> GetAccountMetadata(AccountKey account);

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

        Task RemovePackage(PackageKey package);

        Task<string> GetPackageReadPath(PackageKey package);

        Task<string> GetPackageWritePath(PackageKey package);

        Task<string> GetBotStatusAsync(string botId);

        Task<AlertRecordInfo[]> GetAlertsAsync(Timestamp lastLogTimeUtc, int maxCount);

        Task<LogRecordInfo[]> GetBotLogsAsync(string botId, Timestamp lastLogTimeUtc, int maxCount);

        Task<BotFolderInfo> GetBotFolderInfo(string botId, BotFolderId folderId);

        Task ClearBotFolder(string botId, BotFolderId folderId);

        Task DeleteBotFile(string botId, BotFolderId folderId, string fileName);

        Task<string> GetBotFileReadPath(string botId, BotFolderId folderId, string fileName);

        Task<string> GetBotFileWritePath(string botId, BotFolderId folderId, string fileName);

        #endregion Requests
    }
}
