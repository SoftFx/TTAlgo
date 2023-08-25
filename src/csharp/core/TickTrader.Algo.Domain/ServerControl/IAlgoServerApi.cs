using Google.Protobuf;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Domain.ServerControl
{
    public interface IAlgoServerApi
    {
        SetupContextInfo DefaultSetupContext { get; }


        Task<PackageListSnapshot> GetPackageSnapshot();
        Task<string> UploadPackage(UploadPackageRequest request, string pkgFilePath);
        Task<byte[]> GetPackageBinary(string pkgId);
        Task RemovePackage(RemovePackageRequest request);
        Task<MappingCollectionInfo> GetMappingsInfo(MappingsInfoRequest request);

        Task<AccountListSnapshot> GetAccounts();
        Task<string> AddAccount(AddAccountRequest request);
        Task ChangeAccount(ChangeAccountRequest request);
        Task RemoveAccount(RemoveAccountRequest request);
        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);
        Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request);
        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        Task<PluginListSnapshot> GetPlugins();
        Task AddPlugin(AddPluginRequest request);
        Task UpdatePluginConfig(ChangePluginConfigRequest request);
        Task RemovePlugin(RemovePluginRequest request);
        Task StartPlugin(StartPluginRequest request);
        Task StopPlugin(StopPluginRequest request);
        Task<PluginLogsResponse> GetPluginLogs(PluginLogsRequest request);
        Task<PluginStatusResponse> GetPluginStatus(PluginStatusRequest request);

        Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);
        Task ClearPluginFolder(ClearPluginFolderRequest request);
        Task DeletePluginFile(DeletePluginFileRequest request);
        Task<string> GetPluginFileReadPath(DownloadPluginFileRequest request);
        Task<string> GetPluginFileWritePath(UploadPluginFileRequest request);

        Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request);
        Task SubscribeToAlerts(ChannelWriter<AlertRecordInfo> channel);

        Task<ServerVersionInfo> GetServerVersion();
        Task<ServerUpdateList> GetServerUpdates(ServerUpdateListRequest request);
        Task<StartServerUpdateResponse> StartUpdate(StartServerUpdateRequest request);
        Task DiscardUpdateResult(DiscardServerUpdateResultRequest request);


        Task SubscribeToUpdates(ChannelWriter<IMessage> channel);
    }
}
