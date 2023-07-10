using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using Google.Protobuf;
using TickTrader.Algo.Async;
using ServerApi = TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    public class AlgoServerAdapter
    {
        private readonly ServerApi.IAlgoServerApi _algoServer;
        private readonly IAuthManager _authManager;


        public IEventSource<CredsChangedEvent> CredsChanged => _authManager.CredsChanged;


        public AlgoServerAdapter(ServerApi.IAlgoServerApi algoServer, IAuthManager authManager)
        {
            _algoServer = algoServer;
            _authManager = authManager;
        }


        public Task<AuthResult> ValidateCreds(string login, string password) => _authManager.Auth(login, password);

        public Task<AuthResult> Validate2FA(string login, string oneTimePassword) => _authManager.Auth2FA(login, oneTimePassword);

        public async Task AttachSessionChannel(Channel<IMessage> channel)
        {
            await _algoServer.SubscribeToUpdates(channel.Writer);
        }

        public async Task<List<Domain.AccountModelInfo>> GetAccountList()
        {
            return (await _algoServer.GetAccounts()).Accounts.ToList();
        }

        public async Task<List<Domain.PluginModelInfo>> GetBotList()
        {
            return (await _algoServer.GetPlugins()).Plugins.ToList();
        }

        public async Task<List<Domain.PackageInfo>> GetPackageList()
        {
            return (await _algoServer.GetPackageSnapshot()).Packages.ToList();
        }

        public Task<Domain.ApiMetadataInfo> GetApiMetadata()
        {
            return Task.FromResult(Domain.ApiMetadataInfo.Current);
        }

        public Task<Domain.MappingCollectionInfo> GetMappingsInfo()
        {
            return _algoServer.GetMappingsInfo(new ServerApi.MappingsInfoRequest());
        }

        public Task<Domain.SetupContextInfo> GetSetupContext()
        {
            return Task.FromResult(_algoServer.DefaultSetupContext);
        }

        public Task<Domain.AccountMetadataInfo> GetAccountMetadata(ServerApi.AccountMetadataRequest request)
        {
            return _algoServer.GetAccountMetadata(request);
        }

        public Task StartBot(ServerApi.StartPluginRequest request)
        {
            return _algoServer.StartPlugin(request);
        }

        public Task StopBot(ServerApi.StopPluginRequest request)
        {
            return _algoServer.StopPlugin(request);
        }

        public Task AddBot(ServerApi.AddPluginRequest request)
        {
            return _algoServer.AddPlugin(request);
        }

        public Task RemoveBot(ServerApi.RemovePluginRequest request)
        {
            return _algoServer.RemovePlugin(request);
        }

        public Task ChangeBotConfig(ServerApi.ChangePluginConfigRequest request)
        {
            return _algoServer.UpdatePluginConfig(request);
        }

        public Task<string> AddAccount(ServerApi.AddAccountRequest request)
        {
            return _algoServer.AddAccount(request);
        }

        public Task RemoveAccount(ServerApi.RemoveAccountRequest request)
        {
            return _algoServer.RemoveAccount(request);
        }

        public Task ChangeAccount(ServerApi.ChangeAccountRequest request)
        {
            return _algoServer.ChangeAccount(request);
        }

        public Task<Domain.ConnectionErrorInfo> TestAccount(ServerApi.TestAccountRequest request)
        {
            return _algoServer.TestAccount(request);
        }

        public Task<Domain.ConnectionErrorInfo> TestAccountCreds(ServerApi.TestAccountCredsRequest request)
        {
            return _algoServer.TestCreds(request);
        }

        public Task RemovePackage(ServerApi.RemovePackageRequest request)
        {
            return _algoServer.RemovePackage(request);
        }

        public Task<string> UploadPackage(ServerApi.UploadPackageRequest request, string pkgFilePath)
        {
            return _algoServer.UploadPackage(request, pkgFilePath);
        }

        public Task<byte[]> GetPackageBinary(ServerApi.DownloadPackageRequest request)
        {
            return _algoServer.GetPackageBinary(request.PackageId);
        }

        public Task<ServerApi.PluginStatusResponse> GetBotStatusAsync(ServerApi.PluginStatusRequest request)
        {
            return _algoServer.GetPluginStatus(request);
        }

        public Task<ServerApi.PluginLogsResponse> GetBotLogsAsync(ServerApi.PluginLogsRequest request)
        {
            return _algoServer.GetPluginLogs(request);
        }

        public Task<Domain.AlertRecordInfo[]> GetAlertsAsync(ServerApi.PluginAlertsRequest request)
        {
            return _algoServer.GetAlerts(request);
        }

        public Task<Domain.PluginFolderInfo> GetBotFolderInfo(ServerApi.PluginFolderInfoRequest request)
        {
            return _algoServer.GetPluginFolderInfo(request);
        }

        public Task ClearBotFolder(ServerApi.ClearPluginFolderRequest request)
        {
            return _algoServer.ClearPluginFolder(request);
        }

        public Task DeleteBotFile(ServerApi.DeletePluginFileRequest request)
        {
            return _algoServer.DeletePluginFile(request);
        }

        public Task<string> GetBotFileReadPath(ServerApi.DownloadPluginFileRequest request)
        {
            return _algoServer.GetPluginFileReadPath(request);
        }

        public Task<string> GetBotFileWritePath(ServerApi.UploadPluginFileRequest request)
        {
            return _algoServer.GetPluginFileWritePath(request);
        }

        public Task<string> GetServerVersion()
        {
            return _algoServer.GetServerVersion();
        }

        public Task<ServerApi.ServerUpdateListResponse> GetServerUpdates(ServerApi.ServerUpdateListRequest request)
        {
            return _algoServer.GetServerUpdates(request);
        }

        public Task<ServerApi.StartServerUpdateResponse> StartServerUpdate(ServerApi.StartServerUpdateRequest request)
        {
            return _algoServer.StartUpdate(request);
        }
    }
}
