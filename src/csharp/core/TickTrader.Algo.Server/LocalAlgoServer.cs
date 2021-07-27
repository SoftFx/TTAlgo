using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    public interface IAlgoServerLocal
    {
        ServerBusModel EventBus { get; }


        Task Start();
        Task Stop();
        Task<bool> NeedLegacyState();
        Task LoadLegacyState(ServerSavedState savedState);

        Task<PackageListSnapshot> GetPackageSnapshot();
        Task<bool> PackageWithNameExists(string pkgName);
        Task UploadPackage(UploadPackageRequest request, string pkgFilePath);
        Task<byte[]> GetPackageBinary(string pkgId);
        Task RemovePackage(RemovePackageRequest request);
        Task<MappingCollectionInfo> GetMappingsInfo(MappingsInfoRequest request);

        Task<AccountListSnapshot> GetAccounts();
        Task AddAccount(AddAccountRequest request);
        Task ChangeAccount(ChangeAccountRequest request);
        Task RemoveAccount(RemoveAccountRequest request);
        Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request);
        Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request);
        Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request);

        Task<PluginListSnapshot> GetPlugins();
        Task<PluginModelInfo> GetPluginInfo(string pluginId);
        Task<bool> PluginExists(string pluginId);
        Task<string> GeneratePluginId(string pluginDisplayName);
        Task AddPlugin(AddPluginRequest request);
        Task UpdatePluginConfig(ChangePluginConfigRequest request);
        Task RemovePlugin(RemovePluginRequest request);
        Task StartPlugin(StartPluginRequest request);
        Task StopPlugin(StopPluginRequest request);
        Task<PluginLogRecord[]> GetPluginLogs(PluginLogsRequest request);
        Task<string> GetPluginStatus(PluginStatusRequest request);

        Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request);
        Task ClearPluginFolder(ClearPluginFolderRequest request);
        Task DeletePluginFile(DeletePluginFileRequest request);
        Task<string> GetPluginFileReadPath(DownloadPluginFileRequest request);
        Task<string> GetPluginFileWritePath(UploadPluginFileRequest request);

        Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request);
    }

    public class LocalAlgoServer : IAlgoServerLocal
    {
        private IActorRef _server;


        public ServerBusModel EventBus { get; } = new ServerBusModel();


        public LocalAlgoServer() { }


        public async Task Init(AlgoServerSettings settings)
        {
            _server = AlgoServerActor.Create(settings);
            EventBus.Init(await _server.Ask<IActorRef>(AlgoServerActor.EventBusRequest.Instance));
        }


        public Task Start() => _server.Ask(AlgoServerActor.StartCmd.Instance);
        public Task Stop() => _server.Ask(AlgoServerActor.StopCmd.Instance);
        public Task<bool> NeedLegacyState() => _server.Ask<bool>(AlgoServerActor.NeedLegacyStateRequest.Instance);
        public Task LoadLegacyState(ServerSavedState savedState) => _server.Ask(new AlgoServerActor.LoadLegacyStateCmd(savedState));

        public Task<PackageListSnapshot> GetPackageSnapshot() => EventBus.GetPackageSnapshot();
        public Task<bool> PackageWithNameExists(string pkgName) => _server.Ask<bool>(new PkgFileExistsRequest(pkgName));
        public Task UploadPackage(UploadPackageRequest request, string pkgFilePath) => _server.Ask(new UploadPackageCmd(request, pkgFilePath));
        public Task<byte[]> GetPackageBinary(string pkgId) => _server.Ask<byte[]>(new PkgBinaryRequest(pkgId));
        public Task RemovePackage(RemovePackageRequest request) => _server.Ask<RemovePackageRequest>(request);
        public Task<MappingCollectionInfo> GetMappingsInfo(MappingsInfoRequest request) => _server.Ask<MappingCollectionInfo>(request);

        public Task<AccountListSnapshot> GetAccounts() => EventBus.GetAccountSnapshot();
        public Task AddAccount(AddAccountRequest request) => _server.Ask(request);
        public Task ChangeAccount(ChangeAccountRequest request) => _server.Ask(request);
        public Task RemoveAccount(RemoveAccountRequest request) => _server.Ask(request);
        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request) => _server.Ask<ConnectionErrorInfo>(request);
        public Task<ConnectionErrorInfo> TestCreds(TestAccountCredsRequest request) => _server.Ask<ConnectionErrorInfo>(request);
        public Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request) => _server.Ask<AccountMetadataInfo>(request);

        public Task<PluginListSnapshot> GetPlugins() => EventBus.GetPluginSnapshot();
        public Task<PluginModelInfo> GetPluginInfo(string pluginId) => EventBus.GetPluginInfo(pluginId);
        public Task<bool> PluginExists(string pluginId) => _server.Ask<bool>(new PluginExistsRequest(pluginId));
        public Task<string> GeneratePluginId(string pluginDisplayName) => _server.Ask<string>(new GeneratePluginIdRequest(pluginDisplayName));
        public Task AddPlugin(AddPluginRequest request) => _server.Ask(request);
        public Task UpdatePluginConfig(ChangePluginConfigRequest request) => _server.Ask(request);
        public Task RemovePlugin(RemovePluginRequest request) => _server.Ask(request);
        public Task StartPlugin(StartPluginRequest request) => _server.Ask(request);
        public Task StopPlugin(StopPluginRequest request) => _server.Ask(request);
        public Task<PluginLogRecord[]> GetPluginLogs(PluginLogsRequest request) => _server.Ask<PluginLogRecord[]>(request);
        public Task<string> GetPluginStatus(PluginStatusRequest request) => _server.Ask<string>(request);

        public Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request) => _server.Ask<PluginFolderInfo>(request);
        public Task ClearPluginFolder(ClearPluginFolderRequest request) => _server.Ask(request);
        public Task DeletePluginFile(DeletePluginFileRequest request) => _server.Ask(request);
        public Task<string> GetPluginFileReadPath(DownloadPluginFileRequest request) => _server.Ask<string>(request);
        public Task<string> GetPluginFileWritePath(UploadPluginFileRequest request) => _server.Ask<string>(request);

        public Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request) => _server.Ask<AlertRecordInfo[]>(request);


        internal class PkgFileExistsRequest
        {
            public string PkgName { get; }

            public PkgFileExistsRequest(string pkgName)
            {
                PkgName = pkgName;
            }
        }

        internal class UploadPackageCmd
        {
            public UploadPackageRequest Request { get; }

            public string FilePath { get; }

            public UploadPackageCmd(UploadPackageRequest request, string filePath)
            {
                Request = request;
                FilePath = filePath;
            }
        }

        internal class PkgBinaryRequest
        {
            public string Id { get; }

            public PkgBinaryRequest(string id)
            {
                Id = id;
            }
        }

        internal class PluginExistsRequest
        {
            public string Id { get; }

            public PluginExistsRequest(string id)
            {
                Id = id;
            }
        }

        internal class GeneratePluginIdRequest
        {
            public string PluginDisplayName { get; }

            public GeneratePluginIdRequest(string pluginDisplayName)
            {
                PluginDisplayName = pluginDisplayName;
            }
        }
    }
}
