using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using System.Threading.Channels;
using Google.Protobuf;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class BotAgentServerAdapter : IAlgoServerProvider
    {
        private static IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<BotAgentServerAdapter>();
        private static readonly SetupContextInfo _agentContext = new SetupContextInfo(Feed.Types.Timeframe.M1,
            new SymbolConfig("none", SymbolConfig.Types.SymbolOrigin.Online), MappingDefaults.DefaultBarToBarMapping.Key);


        private readonly IAlgoServerLocal _algoServer;
        private readonly IAuthManager _authManager;


        public event Action AdminCredsChanged = delegate { };
        public event Action DealerCredsChanged = delegate { };
        public event Action ViewerCredsChanged = delegate { };


        public BotAgentServerAdapter(IAlgoServerLocal algoServer, IAuthManager authManager)
        {
            _algoServer = algoServer;
            _authManager = authManager;

            _authManager.AdminCredsChanged += OnAdminCredsChanged;
            _authManager.DealerCredsChanged += OnDealerCredsChanged;
            _authManager.ViewerCredsChanged += OnViewerCredsChanged;
        }


        public Task<AuthResult> ValidateCreds(string login, string password) => _authManager.Auth(login, password);

        public Task<AuthResult> Validate2FA(string login, string oneTimePassword) => _authManager.Auth2FA(login, oneTimePassword);

        public async Task AttachSessionChannel(Channel<IMessage> channel)
        {
            await _algoServer.EventBus.SubscribeToUpdates(channel.Writer, true);
        }

        public async Task<List<AccountModelInfo>> GetAccountList()
        {
            return (await _algoServer.GetAccounts()).Accounts.ToList();
        }

        public async Task<List<PluginModelInfo>> GetBotList()
        {
            return (await _algoServer.GetPlugins()).Plugins.ToList();
        }

        public async Task<List<PackageInfo>> GetPackageList()
        {
            return (await _algoServer.GetPackageSnapshot()).Packages.ToList();
        }

        public Task<ApiMetadataInfo> GetApiMetadata()
        {
            return Task.FromResult(ApiMetadataInfo.Current);
        }

        public Task<MappingCollectionInfo> GetMappingsInfo()
        {
            return _algoServer.GetMappingsInfo(new MappingsInfoRequest());
        }

        public Task<SetupContextInfo> GetSetupContext()
        {
            return Task.FromResult(_agentContext);
        }

        public Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request)
        {
            return _algoServer.GetAccountMetadata(request);
        }

        public Task StartBot(StartPluginRequest request)
        {
            return _algoServer.StartPlugin(request);
        }

        public Task StopBot(StopPluginRequest request)
        {
            return _algoServer.StopPlugin(request);
        }

        public Task AddBot(AddPluginRequest request)
        {
            return _algoServer.AddPlugin(request);
        }

        public Task RemoveBot(RemovePluginRequest request)
        {
            return _algoServer.RemovePlugin(request);
        }

        public Task ChangeBotConfig(ChangePluginConfigRequest request)
        {
            return _algoServer.UpdatePluginConfig(request);
        }

        public Task<string> AddAccount(AddAccountRequest request)
        {
            return _algoServer.AddAccount(request);
        }

        public Task RemoveAccount(RemoveAccountRequest request)
        {
            return _algoServer.RemoveAccount(request);
        }

        public Task ChangeAccount(ChangeAccountRequest request)
        {
            return _algoServer.ChangeAccount(request);
        }

        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            return _algoServer.TestAccount(request);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            return _algoServer.TestCreds(request);
        }

        public Task RemovePackage(RemovePackageRequest request)
        {
            return _algoServer.RemovePackage(request);
        }

        public Task<string> UploadPackage(UploadPackageRequest request, string pkgFilePath)
        {
            return _algoServer.UploadPackage(request, pkgFilePath);
        }

        public Task<byte[]> GetPackageBinary(DownloadPackageRequest request)
        {
            return _algoServer.GetPackageBinary(request.PackageId);
        }

        public Task<PluginStatusResponse> GetBotStatusAsync(PluginStatusRequest request)
        {
            return _algoServer.GetPluginStatus(request);
        }

        public Task<PluginLogsResponse> GetBotLogsAsync(PluginLogsRequest request)
        {
            return _algoServer.GetPluginLogs(request);
        }

        public Task<AlertRecordInfo[]> GetAlertsAsync(PluginAlertsRequest request)
        {
            return _algoServer.GetAlerts(request);
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(PluginFolderInfoRequest request)
        {
            return _algoServer.GetPluginFolderInfo(request);
        }

        public Task ClearBotFolder(ClearPluginFolderRequest request)
        {
            return _algoServer.ClearPluginFolder(request);
        }

        public Task DeleteBotFile(DeletePluginFileRequest request)
        {
            return _algoServer.DeletePluginFile(request);
        }

        public Task<string> GetBotFileReadPath(DownloadPluginFileRequest request)
        {
            return _algoServer.GetPluginFileReadPath(request);
        }

        public Task<string> GetBotFileWritePath(UploadPluginFileRequest request)
        {
            return _algoServer.GetPluginFileWritePath(request);
        }


        #region Event handlers

        private void OnAdminCredsChanged()
        {
            AdminCredsChanged?.Invoke();
        }

        private void OnDealerCredsChanged()
        {
            DealerCredsChanged?.Invoke();
        }

        private void OnViewerCredsChanged()
        {
            ViewerCredsChanged?.Invoke();
        }

        #endregion Event handlers
    }
}
