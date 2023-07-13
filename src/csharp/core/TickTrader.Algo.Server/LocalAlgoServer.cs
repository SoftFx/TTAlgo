using Google.Protobuf;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    public interface IAlgoServerLocal : IAlgoServerApi
    {
        ServerBusModel EventBus { get; }


        Task Start();
        Task Stop();
        Task<bool> NeedLegacyState();
        Task LoadLegacyState(ServerSavedState savedState);

        Task<bool> PackageWithNameExists(string pkgName);

        Task<PluginModelInfo> GetPluginInfo(string pluginId);
        Task<bool> PluginExists(string pluginId);
        Task<string> GeneratePluginId(string pluginDisplayName);
    }

    public class LocalAlgoServer : IAlgoServerLocal
    {
        private static readonly SetupContextInfo _setupContext = new SetupContextInfo(Feed.Types.Timeframe.M1,
            new SymbolConfig("none", SymbolConfig.Types.SymbolOrigin.Online), MappingDefaults.DefaultBarToBarMapping.Key);

        private IActorRef _server, _algoHost;
        private bool _ownsAlgoHost;


        public ServerBusModel EventBus { get; } = new ServerBusModel();

        public SetupContextInfo DefaultSetupContext => _setupContext;


        public LocalAlgoServer() { }


        public async Task Init(AlgoServerSettings settings, IActorRef algoHost = null)
        {
            _algoHost = algoHost;
            if (algoHost == null)
            {
                _ownsAlgoHost = true;
                settings.HostSettings.RuntimeSettings.WorkingDirectory ??= settings.DataFolder;
                _algoHost = AlgoHostActor.Create(settings.HostSettings);
            }

            _server = AlgoServerActor.Create(_algoHost, settings);
            await AlgoHostModel.AddConsumer(_algoHost, _server);
            EventBus.Init(await _server.Ask<IActorRef>(AlgoServerActor.EventBusRequest.Instance));
        }


        public async Task Start()
        {
            if (_ownsAlgoHost)
                await AlgoHostModel.Start(_algoHost);
            await _server.Ask(AlgoServerActor.StartCmd.Instance);
        }

        public async Task Stop()
        {
            await _server.Ask(AlgoServerActor.StopCmd.Instance);
            if (_ownsAlgoHost)
                await AlgoHostModel.Stop(_algoHost);
        }

        public Task<bool> NeedLegacyState() => _server.Ask<bool>(AlgoServerActor.NeedLegacyStateRequest.Instance);
        public Task LoadLegacyState(ServerSavedState savedState) => _server.Ask(new AlgoServerActor.LoadLegacyStateCmd(savedState));

        public Task<PackageListSnapshot> GetPackageSnapshot() => EventBus.GetPackageSnapshot();
        public Task<bool> PackageWithNameExists(string pkgName) => _server.Ask<bool>(new AlgoHostModel.PkgFileExistsRequest(pkgName));
        public Task<string> UploadPackage(UploadPackageRequest request, string pkgFilePath) => AlgoHostModel.UploadPackage(_server, request, pkgFilePath);
        public Task<byte[]> GetPackageBinary(string pkgId) => _server.Ask<byte[]>(new AlgoHostModel.PkgBinaryRequest(pkgId));
        public Task RemovePackage(RemovePackageRequest request) => AlgoHostModel.RemovePackage(_server, request);
        public Task<MappingCollectionInfo> GetMappingsInfo(MappingsInfoRequest request) => _server.Ask<MappingCollectionInfo>(request);

        public Task<AccountListSnapshot> GetAccounts() => EventBus.GetAccountSnapshot();
        public Task<string> AddAccount(AddAccountRequest request) => _server.Ask<string>(request);
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
        public Task<PluginLogsResponse> GetPluginLogs(PluginLogsRequest request) => _server.Ask<PluginLogsResponse>(request);
        public Task<PluginStatusResponse> GetPluginStatus(PluginStatusRequest request) => _server.Ask<PluginStatusResponse>(request);

        public Task<PluginFolderInfo> GetPluginFolderInfo(PluginFolderInfoRequest request) => _server.Ask<PluginFolderInfo>(request);
        public Task ClearPluginFolder(ClearPluginFolderRequest request) => _server.Ask(request);
        public Task DeletePluginFile(DeletePluginFileRequest request) => _server.Ask(request);
        public Task<string> GetPluginFileReadPath(DownloadPluginFileRequest request) => _server.Ask<string>(request);
        public Task<string> GetPluginFileWritePath(UploadPluginFileRequest request) => _server.Ask<string>(request);

        public Task<AlertRecordInfo[]> GetAlerts(PluginAlertsRequest request) => _server.Ask<AlertRecordInfo[]>(request);
        public Task SubscribeToAlerts(ChannelWriter<AlertRecordInfo> channel) => _server.Ask(new SubscribeToAlertsCmd(channel));

        public Task SubscribeToUpdates(ChannelWriter<IMessage> channel) => EventBus.SubscribeToUpdates(channel, true);

        public PluginListenerProxy GetPluginListenerProxy(string pluginId) => new PluginListenerProxy(_server, pluginId);

        public Task<ServerVersionInfo> GetServerVersion() => _server.Ask<ServerVersionInfo>(ServerVersionRequest.Instance);
        public Task<ServerUpdateList> GetServerUpdates(ServerUpdateListRequest request) => _server.Ask<ServerUpdateList>(request);
        public Task<StartServerUpdateResponse> StartUpdate(StartServerUpdateRequest request) => _server.Ask<StartServerUpdateResponse>(request);


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

        internal class SubscribeToAlertsCmd
        {
            public ChannelWriter<AlertRecordInfo> AlertSink { get; }

            public SubscribeToAlertsCmd(ChannelWriter<AlertRecordInfo> alertSink)
            {
                AlertSink = alertSink;
            }
        }
    }
}
