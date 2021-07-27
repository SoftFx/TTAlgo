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

        Task<MappingCollectionInfo> GetMappingsInfo(MappingsInfoRequest request);

        Task<SetupContextInfo> GetSetupContext();

        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        Task StartBot(StartPluginRequest request);

        Task StopBot(StopPluginRequest request);

        Task AddBot(AddPluginRequest request);

        Task RemoveBot(RemovePluginRequest request);

        Task ChangeBotConfig(ChangePluginConfigRequest request);

        Task AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        Task RemovePackage(RemovePackageRequest request);

        Task UploadPackage(UploadPackageRequest request, string pkgFilePath);

        Task<byte[]> GetPackageBinary(DownloadPackageRequest request);

        Task<string> GetBotStatusAsync(PluginStatusRequest request);

        Task<AlertRecordInfo[]> GetAlertsAsync(PluginAlertsRequest request);

        Task<LogRecordInfo[]> GetBotLogsAsync(PluginLogsRequest request);

        Task<PluginFolderInfo> GetBotFolderInfo(PluginFolderInfoRequest request);

        Task ClearBotFolder(ClearPluginFolderRequest request);

        Task DeleteBotFile(DeletePluginFileRequest request);

        Task<string> GetBotFileReadPath(DownloadPluginFileRequest request);

        Task<string> GetBotFileWritePath(UploadPluginFileRequest request);

        #endregion Requests
    }
}
