using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.WebAdmin.Server.Models;
using TickTrader.Algo.Package;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class BotAgentServerAdapter : IAlgoServerProvider
    {
        private static IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<BotAgentServerAdapter>();
        private static readonly SetupContextInfo _agentContext = new SetupContextInfo(Feed.Types.Timeframe.M1,
            new SymbolConfig("none", SymbolConfig.Types.SymbolOrigin.Online), MappingDefaults.DefaultBarToBarMapping.Key);


        private readonly IBotAgent _botAgent;
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


        public BotAgentServerAdapter(IBotAgent botAgent, IAuthManager authManager)
        {
            _botAgent = botAgent;
            _authManager = authManager;

            _botAgent.AccountChanged += OnAccountChanged;
            _botAgent.BotChanged += OnBotChanged;
            _botAgent.PackageChanged += OnPackageChanged;
            _botAgent.BotStateChanged += OnBotStateChanged;
            _botAgent.AccountStateChanged += OnAccountStateChanged;
            _botAgent.PackageStateChanged += OnPackageStateChanged;

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

        public async Task<List<AccountModelInfo>> GetAccountList()
        {
            return (await _botAgent.GetAccounts()).Accounts.ToList();
        }

        public async Task<List<PluginModelInfo>> GetBotList()
        {
            return (await _botAgent.GetBots()).Plugins.ToList();
        }

        public async Task<List<PackageInfo>> GetPackageList()
        {
            return (await _botAgent.GetPackageSnapshot()).Packages.ToList();
        }

        public Task<ApiMetadataInfo> GetApiMetadata()
        {
            return Task.FromResult(ApiMetadataInfo.CreateCurrentMetadata());
        }

        public Task<MappingCollectionInfo> GetMappingsInfo()
        {
            return _botAgent.GetMappingsInfo();
        }

        public Task<SetupContextInfo> GetSetupContext()
        {
            return Task.FromResult(_agentContext);
        }

        public Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request)
        {
            return _botAgent.GetAccountMetadata(request);
        }

        public Task StartBot(StartPluginRequest request)
        {
            return _botAgent.StartBot(request);
        }

        public Task StopBot(StopPluginRequest request)
        {
            return _botAgent.StopBotAsync(request);
        }

        public Task AddBot(AddPluginRequest request)
        {
            return _botAgent.AddBot(request);
        }

        public Task RemoveBot(RemovePluginRequest request)
        {
            return _botAgent.RemoveBot(request);
        }

        public Task ChangeBotConfig(ChangePluginConfigRequest request)
        {
            return _botAgent.ChangeBotConfig(request);
        }

        public Task AddAccount(AddAccountRequest request)
        {
            return _botAgent.AddAccount(request);
        }

        public Task RemoveAccount(RemoveAccountRequest request)
        {
            return _botAgent.RemoveAccount(request);
        }

        public Task ChangeAccount(ChangeAccountRequest request)
        {
            return _botAgent.ChangeAccount(request);
        }

        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            return _botAgent.TestAccount(request);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            return _botAgent.TestCreds(request);
        }

        public Task RemovePackage(RemovePackageRequest request)
        {
            return _botAgent.RemovePackage(request);
        }

        public Task UploadPackage(UploadPackageRequest request, string pkgFilePath)
        {
            return _botAgent.UploadPackage(request, pkgFilePath);
        }

        public Task<byte[]> GetPackageBinary(DownloadPackageRequest request)
        {
            return _botAgent.DownloadPackage(request.PackageId);
        }

        public Task<string> GetBotStatusAsync(PluginStatusRequest request)
        {
            return _botAgent.GetBotStatus(request);
        }

        public async Task<LogRecordInfo[]> GetBotLogsAsync(PluginLogsRequest request)
        {
            var msgs = await _botAgent.GetBotLogs(request);

            return msgs.Select(e => new LogRecordInfo
            {
                TimeUtc = e.TimeUtc,
                Severity = e.Severity,
                Message = e.Message,
            }).ToArray();
        }

        public Task<AlertRecordInfo[]> GetAlertsAsync(PluginAlertsRequest request)
        {
            return _botAgent.GetAlerts(request);
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(PluginFolderInfoRequest request)
        {
            return _botAgent.GetPluginFolderInfo(request);
        }

        public Task ClearBotFolder(ClearPluginFolderRequest request)
        {
            return _botAgent.ClearPluginFolder(request);
        }

        public Task DeleteBotFile(DeletePluginFileRequest request)
        {
            return _botAgent.DeletePluginFile(request);
        }

        public Task<string> GetBotFileReadPath(DownloadPluginFileRequest request)
        {
            return _botAgent.GetPluginFileReadPath(request);
        }

        public Task<string> GetBotFileWritePath(UploadPluginFileRequest request)
        {
            return _botAgent.GetPluginFileWritePath(request);
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

        private void OnAccountChanged(AccountModelUpdate update)
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

        private void OnBotChanged(PluginModelUpdate update)
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

        private void OnPackageChanged(PackageUpdate update)
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

        private void OnBotStateChanged(PluginStateUpdate bot)
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

        private void OnAccountStateChanged(AccountStateUpdate account)
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

        private void OnPackageStateChanged(PackageStateUpdate package)
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
