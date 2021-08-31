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

        public event Action<UpdateInfo<PackageInfo>> PackageUpdated = delegate { };
        public event Action<UpdateInfo<AccountModelInfo>> AccountUpdated = delegate { };
        public event Action<UpdateInfo<PluginModelInfo>> BotUpdated = delegate { };
        public event Action<PackageStateUpdate> PackageStateUpdated = delegate { };
        public event Action<PluginStateUpdate> BotStateUpdated = delegate { };
        public event Action<AccountStateUpdate> AccountStateUpdated = delegate { };


        public BotAgentServerAdapter(IAlgoServerLocal algoServer, IAuthManager authManager)
        {
            _algoServer = algoServer;
            _authManager = authManager;

            var eventBus = algoServer.EventBus;
            eventBus.PackageUpdated.Subscribe(OnPackageUpdated);
            eventBus.AccountUpdated.Subscribe(OnAccountUpdated);
            eventBus.PluginUpdated.Subscribe(OnPluginUpdated);
            eventBus.PackageStateUpdated.Subscribe(OnPackageStateUpdated);
            eventBus.AccountStateUpdated.Subscribe(OnAccountStateUpdated);
            eventBus.PluginStateUpdated.Subscribe(OnPluginStateUpdated);

            _authManager.AdminCredsChanged += OnAdminCredsChanged;
            _authManager.DealerCredsChanged += OnDealerCredsChanged;
            _authManager.ViewerCredsChanged += OnViewerCredsChanged;
        }


        public ClientClaims.Types.AccessLevel ValidateCreds(string login, string password)
        {
            if (_authManager.ValidViewerCreds(login, password))
                return ClientClaims.Types.AccessLevel.Viewer;
            if (_authManager.ValidDealerCreds(login, password))
                return ClientClaims.Types.AccessLevel.Dealer;
            if (_authManager.ValidAdminCreds(login, password))
                return ClientClaims.Types.AccessLevel.Admin;

            return ClientClaims.Types.AccessLevel.Anonymous;
        }

        public async Task AttachSessionChannel(Channel<IMessage> channel)
        {
            channel.Writer.TryWrite(ApiMetadataInfo.Current);
            channel.Writer.TryWrite(_agentContext);
            channel.Writer.TryWrite(await _algoServer.GetMappingsInfo(new MappingsInfoRequest()));
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

        public Task<MappingCollectionInfo> GetMappingsInfo(MappingsInfoRequest request)
        {
            return _algoServer.GetMappingsInfo(request);
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

        public Task<string> GetBotStatusAsync(PluginStatusRequest request)
        {
            return _algoServer.GetPluginStatus(request);
        }

        public async Task<LogRecordInfo[]> GetBotLogsAsync(PluginLogsRequest request)
        {
            var msgs = await _algoServer.GetPluginLogs(request);

            return msgs.Select(e => new LogRecordInfo
            {
                TimeUtc = e.TimeUtc,
                Severity = e.Severity,
                Message = e.Message,
            }).ToArray();
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


        private UpdateInfo.Types.UpdateType Convert(Update.Types.Action action)
        {
            switch (action)
            {
                case Update.Types.Action.Added:
                    return UpdateInfo.Types.UpdateType.Added;
                case Update.Types.Action.Updated:
                    return UpdateInfo.Types.UpdateType.Replaced;
                case Update.Types.Action.Removed:
                    return UpdateInfo.Types.UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        #region Event handlers

        private void OnAccountUpdated(AccountModelUpdate update)
        {
            try
            {
                var acc = update.Account;
                if (update.Action == Update.Types.Action.Removed)
                    acc = new AccountModelInfo { AccountId = update.Id }; // backwards compatibility

                AccountUpdated(new UpdateInfo<AccountModelInfo>(Convert(update.Action), acc));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {ex.Message}");
            }
        }

        private void OnPluginUpdated(PluginModelUpdate update)
        {
            try
            {
                var plugin = update.Plugin;
                if (update.Action == Update.Types.Action.Removed)
                    plugin = new PluginModelInfo { InstanceId = update.Id }; // backwards compatibility

                BotUpdated(new UpdateInfo<PluginModelInfo>(Convert(update.Action), plugin));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {ex.Message}");
            }
        }

        private void OnPackageUpdated(PackageUpdate update)
        {
            try
            {
                var pkg = update.Package;
                if (update.Action == Update.Types.Action.Removed)
                    pkg = new PackageInfo { PackageId = update.Id }; // backwards compatibility

                PackageUpdated(new UpdateInfo<PackageInfo>(Convert(update.Action), pkg));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {ex.Message}");
            }
        }

        private void OnPluginStateUpdated(PluginStateUpdate bot)
        {
            try
            {
                BotStateUpdated(bot);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {ex.Message}");
            }
        }

        private void OnAccountStateUpdated(AccountStateUpdate account)
        {
            try
            {
                AccountStateUpdated(account);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {ex.Message}");
            }
        }

        private void OnPackageStateUpdated(PackageStateUpdate package)
        {
            try
            {
                PackageStateUpdated(package);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {ex.Message}");
            }
        }

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
