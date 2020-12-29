using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class BotAgentServerAdapter : IBotAgentServer
    {
        private static IAlgoCoreLogger _logger = CoreLoggerFactory.GetLogger<BotAgentServerAdapter>();
        private static readonly SetupContext _agentContext = new SetupContext();


        private readonly IBotAgent _botAgent;
        private readonly IAuthManager _authManager;


        public event Action AdminCredsChanged = delegate { };
        public event Action DealerCredsChanged = delegate { };
        public event Action ViewerCredsChanged = delegate { };

        public event Action<UpdateInfo<PackageInfo>> PackageUpdated = delegate { };
        public event Action<UpdateInfo<AccountModelInfo>> AccountUpdated = delegate { };
        public event Action<UpdateInfo<BotModelInfo>> BotUpdated = delegate { };
        public event Action<PackageInfo> PackageStateUpdated = delegate { };
        public event Action<BotModelInfo> BotStateUpdated = delegate { };
        public event Action<AccountModelInfo> AccountStateUpdated = delegate { };


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


        public AccessLevels ValidateCreds(string login, string password)
        {
            if (_authManager.ValidViewerCreds(login, password))
                return AccessLevels.Viewer;
            if (_authManager.ValidDealerCreds(login, password))
                return AccessLevels.Dealer;
            if (_authManager.ValidAdminCreds(login, password))
                return AccessLevels.Admin;

            return AccessLevels.Anonymous;
        }

        public Task<List<AccountModelInfo>> GetAccountList()
        {
            return _botAgent.GetAccounts();
        }

        public Task<List<BotModelInfo>> GetBotList()
        {
            return _botAgent.GetBots();
        }

        public Task<List<PackageInfo>> GetPackageList()
        {
            return _botAgent.GetPackages();
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
            return Task.FromResult(new SetupContextInfo(_agentContext.DefaultTimeFrame, _agentContext.DefaultSymbol.ToInfo(), _agentContext.DefaultMapping));
        }

        public async Task<AccountMetadataInfo> GetAccountMetadata(AccountKey account)
        {
            var (error, accMetadata) = await _botAgent.GetAccountMetadata(account);
            if (error.Code != ConnectionErrorCodes.None)
                throw new Exception($"Account {account.Login} at {account.Server} failed to connect");
            return accMetadata;
        }

        public Task StartBot(string botId)
        {
            return _botAgent.StartBot(botId);
        }

        public Task StopBot(string botId)
        {
            return _botAgent.StopBotAsync(botId);
        }

        public Task AddBot(AccountKey account, PluginConfig config)
        {
            return _botAgent.AddBot(account, config);
        }

        public Task RemoveBot(string botId, bool cleanLog, bool cleanAlgoData)
        {
            return _botAgent.RemoveBot(botId, cleanLog, cleanAlgoData);
        }

        public Task ChangeBotConfig(string botId, PluginConfig newConfig)
        {
            return _botAgent.ChangeBotConfig(botId, newConfig);
        }

        public Task AddAccount(AccountKey account, string password)
        {
            return _botAgent.AddAccount(account, password);
        }

        public Task RemoveAccount(AccountKey account)
        {
            return _botAgent.RemoveAccount(account);
        }

        public Task ChangeAccount(AccountKey account, string password)
        {
            return _botAgent.ChangeAccount(account, password);
        }

        public Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            return _botAgent.TestAccount(account);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password)
        {
            return _botAgent.TestCreds(account, password);
        }

        public Task RemovePackage(PackageKey package)
        {
            return _botAgent.RemovePackage(package);
        }

        public Task<string> GetPackageReadPath(PackageKey package)
        {
            return _botAgent.GetPackageReadPath(package);
        }

        public Task<string> GetPackageWritePath(PackageKey package)
        {
            return _botAgent.GetPackageWritePath(package);
        }

        public async Task<string> GetBotStatusAsync(string botId)
        {
            var log = await _botAgent.GetBotLog(botId);
            return await log.GetStatusAsync();
        }

        public async Task<LogRecordInfo[]> GetBotLogsAsync(string botId, Timestamp lastLogTimeUtc, int maxCount)
        {
            var log = await _botAgent.GetBotLog(botId);
            var msgs = await log.QueryMessagesAsync(lastLogTimeUtc, maxCount);

            return msgs.Select(e => new LogRecordInfo
            {
                TimeUtc = e.TimeUtc,
                Severity = e.Severity,
                Message = e.Message,
            }).ToArray();
        }

        public async Task<AlertRecordInfo[]> GetAlertsAsync(Timestamp lastLogTimeUtc, int maxCount)
        {
            var storage = await _botAgent.GetAlertStorage();
            var alerts = await storage.QueryAlertsAsync(lastLogTimeUtc, maxCount);

            return alerts.Select(e => new AlertRecordInfo
            {
                TimeUtc = e.TimeUtc,
                Message = e.Message,
                BotId = e.BotId,
            }).ToArray();
        }

        public async Task<BotFolderInfo> GetBotFolderInfo(string botId, BotFolderId folderId)
        {
            var botFolder = await GetBotFolder(botId, folderId);

            return new BotFolderInfo
            {
                BotId = botId,
                FolderId = folderId,
                Path = botFolder.Folder,
                Files = botFolder.Files.Select(f => new BotFileInfo { Name = f.Name, Size = f.Size }).ToList(),
            };
        }

        public async Task ClearBotFolder(string botId, BotFolderId folderId)
        {
            var botFolder = await GetBotFolder(botId, folderId);

            botFolder.Clear();
        }

        public async Task DeleteBotFile(string botId, BotFolderId folderId, string fileName)
        {
            var botFolder = await GetBotFolder(botId, folderId);

            botFolder.DeleteFile(fileName);
        }

        public async Task<string> GetBotFileReadPath(string botId, BotFolderId folderId, string fileName)
        {
            var botFolder = await GetBotFolder(botId, folderId);

            return botFolder.GetFileReadPath(fileName);
        }

        public async Task<string> GetBotFileWritePath(string botId, BotFolderId folderId, string fileName)
        {
            var botFolder = await GetBotFolder(botId, folderId);

            return botFolder.GetFileWritePath(fileName);
        }


        private UpdateType Convert(ChangeAction action)
        {
            switch (action)
            {
                case ChangeAction.Added:
                    return UpdateType.Added;
                case ChangeAction.Modified:
                    return UpdateType.Replaced;
                case ChangeAction.Removed:
                    return UpdateType.Removed;
                default:
                    throw new ArgumentException();
            }
        }

        private async Task<IBotFolder> GetBotFolder(string botId, BotFolderId folderId)
        {
            switch (folderId)
            {
                case BotFolderId.AlgoData:
                    return await _botAgent.GetAlgoData(botId);
                case BotFolderId.BotLogs:
                    return await _botAgent.GetBotLog(botId);
                default:
                    throw new ArgumentException();
            }
        }

        #region Event handlers

        private void OnAccountChanged(AccountModelInfo account, ChangeAction action)
        {
            try
            {
                AccountUpdated(new UpdateInfo<AccountModelInfo>
                {
                    Type = Convert(action),
                    Value = account,
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnBotChanged(BotModelInfo bot, ChangeAction action)
        {
            try
            {
                BotUpdated(new UpdateInfo<BotModelInfo>
                {
                    Type = Convert(action),
                    Value = bot,
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnPackageChanged(PackageInfo package, ChangeAction action)
        {
            try
            {
                PackageUpdated(new UpdateInfo<PackageInfo>
                {
                    Type = Convert(action),
                    Value = package,
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnBotStateChanged(BotModelInfo bot)
        {
            try
            {
                BotStateUpdated(bot);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnAccountStateChanged(AccountModelInfo account)
        {
            try
            {
                AccountStateUpdated(account);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
            }
        }

        private void OnPackageStateChanged(PackageInfo package)
        {
            try
            {
                PackageStateUpdated(package);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send update: {ex.Message}", ex);
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
