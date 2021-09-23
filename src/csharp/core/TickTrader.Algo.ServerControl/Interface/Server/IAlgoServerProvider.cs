using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
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

        Task<AuthResult> ValidateCreds(string login, string password);

        Task<AuthResult> Validate2FA(string login, string oneTimePassword);

        #endregion Credentials


        #region Initialization

        Task<List<PackageInfo>> GetPackageList();

        Task<List<AccountModelInfo>> GetAccountList();

        Task<List<PluginModelInfo>> GetBotList();

        #endregion Initialization


        #region Updates

        Task AttachSessionChannel(Channel<IMessage> channel);

        #endregion Updates


        #region Requests

        Task<ApiMetadataInfo> GetApiMetadata();

        Task<MappingCollectionInfo> GetMappingsInfo();

        Task<SetupContextInfo> GetSetupContext();

        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        Task StartBot(StartPluginRequest request);

        Task StopBot(StopPluginRequest request);

        Task AddBot(AddPluginRequest request);

        Task RemoveBot(RemovePluginRequest request);

        Task ChangeBotConfig(ChangePluginConfigRequest request);

        Task<string> AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        Task RemovePackage(RemovePackageRequest request);

        Task<string> UploadPackage(UploadPackageRequest request, string pkgFilePath);

        Task<byte[]> GetPackageBinary(DownloadPackageRequest request);

        Task<PluginStatusResponse> GetBotStatusAsync(PluginStatusRequest request);

        Task<AlertRecordInfo[]> GetAlertsAsync(PluginAlertsRequest request);

        Task<PluginLogsResponse> GetBotLogsAsync(PluginLogsRequest request);

        Task<PluginFolderInfo> GetBotFolderInfo(PluginFolderInfoRequest request);

        Task ClearBotFolder(ClearPluginFolderRequest request);

        Task DeleteBotFile(DeletePluginFileRequest request);

        Task<string> GetBotFileReadPath(DownloadPluginFileRequest request);

        Task<string> GetBotFileWritePath(UploadPluginFileRequest request);

        #endregion Requests
    }
}
