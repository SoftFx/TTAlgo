using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public interface IAlgoServerProvider
    {
        #region Credentials

        event Action AdminCredsChanged;

        event Action DealerCredsChanged;

        event Action ViewerCredsChanged;

        ClientClaims.Types.AccessLevel ValidateCreds(string login, string password);

        #endregion Credentials


        #region Initialization

        Task<List<PackageInfo>> GetPackageList();

        Task<List<AccountModelInfo>> GetAccountList();

        Task<List<PluginModelInfo>> GetBotList();

        #endregion Initialization


        #region Updates

        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<AccountModelInfo>> AccountUpdated;

        event Action<UpdateInfo<PluginModelInfo>> BotUpdated;

        event Action<PackageStateUpdate> PackageStateUpdated;

        event Action<PluginStateUpdate> BotStateUpdated;

        event Action<AccountStateUpdate> AccountStateUpdated;

        #endregion Updates


        #region Requests

        Task<ApiMetadataInfo> GetApiMetadata();

        Task<MappingCollectionInfo> GetMappingsInfo();

        Task<SetupContextInfo> GetSetupContext();

        Task<AccountMetadataInfo> GetAccountMetadata(string accountId);

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

        Task RemovePackage(string packageId);

        Task<string> GetPackageReadPath(string packageId);

        Task<string> GetPackageWritePath(string packageId);

        Task<string> GetBotStatusAsync(string botId);

        Task<AlertRecordInfo[]> GetAlertsAsync(Timestamp lastLogTimeUtc, int maxCount);

        Task<LogRecordInfo[]> GetBotLogsAsync(string botId, Timestamp lastLogTimeUtc, int maxCount);

        Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId);

        Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName);

        Task<string> GetBotFileReadPath(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName);

        Task<string> GetBotFileWritePath(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName);

        #endregion Requests
    }
}
