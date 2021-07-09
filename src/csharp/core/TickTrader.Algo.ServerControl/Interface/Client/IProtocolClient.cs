using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public interface IProtocolClient
    {
        #region Connection Management

        ClientStates State { get; }

        string LastError { get; }

        VersionSpec VersionSpec { get; }

        AccessManager AccessManager { get; }


        Task Connect(ClientSessionSettings settings);

        Task Disconnect();

        #endregion Connection Management


        #region Other

        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        // to be removed
        Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request);

        // replacement
        //Task SubscribeToAlertList(AlertListSubscribeRequest request); // request params: Timestamp from

        #endregion Other


        #region Package Management

        Task UploadPackage(UploadPackageRequest request, string srcPath, IFileProgressListener progressListener);

        Task RemovePackage(RemovePackageRequest request);

        Task DownloadPackage(DownloadPackageRequest request, string dstPath, IFileProgressListener progressListener);

        #endregion Package Management


        #region Account Management

        Task AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        #endregion Account Management


        #region Plugin Management

        Task AddPlugin(AddPluginRequest request);

        Task RemovePlugin(RemovePluginRequest request);

        Task StartPlugin(StartPluginRequest request);

        Task StopPlugin(StopPluginRequest request);

        Task ChangePluginConfig(ChangePluginConfigRequest request);

        // to be removed
        Task<string> GetPluginStatus(PluginStatusRequest request);

        // replacement
        //Task SubscribeToPluginStatus(PluginStatusSubscribeRequest request); // request params: string pluginId

        // to be removed
        Task<LogRecordInfo[]> GetPluginLogs(PluginLogsRequest request);

        // replacement
        //Task SubscribeToPluginLogs(PluginLogsSubscribeRequest request); // request params: string pluginId

        #endregion Plugin Management


        #region Plugin Files Management

        Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);

        Task ClearPluginFolder(ClearPluginFolderRequest request);

        Task DeletePluginFile(DeletePluginFileRequest request);

        Task DownloadPluginFile(DownloadPluginFileRequest request, string dstPath, IFileProgressListener progressListener);

        Task UploadPluginFile(UploadPluginFileRequest request, string srcPath, IFileProgressListener progressListener);

        #endregion Plugin Files Management
    }
}
