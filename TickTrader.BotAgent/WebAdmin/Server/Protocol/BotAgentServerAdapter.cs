using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
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

        public async Task<AccountMetadataInfo> GetAccountMetadata(AccountMetadataRequest request)
        {
            var accountId = request.AccountId;

            var (error, accMetadata) = await _botAgent.GetAccountMetadata(accountId);
            if (!error.IsOk)
                throw new Exception($"Account '{accountId}' failed to connect");
            return accMetadata;
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

        public async Task<PluginFolderInfo> GetBotFolderInfo(PluginFolderInfoRequest request)
        {
            var botId = request.PluginId;
            var folderId = request.FolderId;

            var botFolder = await GetBotFolder(botId, folderId);

            var res = new PluginFolderInfo
            {
                PluginId = botId,
                FolderId = folderId,
                Path = await botFolder.GetFolder(),
            };
            res.Files.AddRange((await botFolder.GetFiles()).Select(f => new PluginFileInfo { Name = f.Name, Size = f.Size }));
            return res;
        }

        public async Task ClearBotFolder(ClearPluginFolderRequest request)
        {
            var botFolder = await GetBotFolder(request.PluginId, request.FolderId);

            await botFolder.Clear();
        }

        public async Task DeleteBotFile(DeletePluginFileRequest request)
        {
            var botFolder = await GetBotFolder(request.PluginId, request.FolderId);

            await botFolder.DeleteFile(request.FileName);
        }

        public async Task<string> GetBotFileReadPath(DownloadPluginFileRequest request)
        {
            var botFolder = await GetBotFolder(request.PluginId, request.FolderId);

            return await botFolder.GetFileReadPath(request.FileName);
        }

        public async Task<string> GetBotFileWritePath(UploadPluginFileRequest request)
        {
            var botFolder = await GetBotFolder(request.PluginId, request.FolderId);

            return await botFolder.GetFileWritePath(request.FileName);
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

        private async Task<IBotFolder> GetBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            switch (folderId)
            {
                case PluginFolderInfo.Types.PluginFolderId.AlgoData:
                    return await _botAgent.GetAlgoData(botId);
                case PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return await _botAgent.GetBotLog(botId);
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
