using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.ServerControl
{
    public interface IProtocolClient
    {
        void StartClient();

        void StopClient();

        void SendLogin();

        void SendLogout();

        void SendDisconnect();

        void Init();

        Task<ApiMetadataInfo> GetApiMetadata();

        Task<MappingCollectionInfo> GetMappingsInfo();

        Task<SetupContextInfo> GetSetupContext();

        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        Task<List<PluginModelInfo>> GetPluginList();

        Task AddPlugin(AddPluginRequest request);

        Task RemovePlugin(RemovePluginRequest request);

        Task StartPlugin(StartPluginRequest request);

        Task StopPlugin(StopPluginRequest request);

        Task ChangePluginConfig(ChangePluginConfigRequest request);

        Task<List<AccountModelInfo>> GetAccountList();

        Task AddAccount(AddAccountRequest request);

        Task RemoveAccount(RemoveAccountRequest request);

        Task ChangeAccount(ChangeAccountRequest request);

        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);

        Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request);

        Task<List<PackageInfo>> GetPackageList();

        Task UploadPackage(UploadPackageRequest request, string srcPath, IFileProgressListener progressListener);

        Task RemovePackage(RemovePackageRequest request);

        Task DownloadPackage(DownloadPackageRequest request, string dstPath, IFileProgressListener progressListener);

        Task<string> GetPluginStatus(PluginStatusRequest request);

        Task<LogRecordInfo[]> GetPluginLogs(PluginLogsRequest request);

        Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request);

        Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);

        Task ClearPluginFolder(ClearPluginFolderRequest request);

        Task DeletePluginFile(DeletePluginFileRequest request);

        Task DownloadPluginFile(DownloadPluginFileRequest request, string dstPath, IFileProgressListener progressListener);

        Task UploadPluginFile(UploadPluginFileRequest request, string srcPath, IFileProgressListener progressListener);
    }
}
