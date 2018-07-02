using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Protocol;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Protocol
{
    public class BotAgentServer : IBotAgentServer
    {
        private static IAlgoCoreLogger _logger = CoreLoggerFactory.GetLogger<BotAgentServer>();
        private static readonly SetupContext _agentContext = new SetupContext();


        private IBotAgent _botAgent;
        private ServerCredentials _serverCreds;


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated = delegate { };
        public event Action<UpdateInfo<AccountModelInfo>> AccountUpdated = delegate { };
        public event Action<UpdateInfo<BotModelInfo>> BotUpdated = delegate { };
        public event Action<PackageInfo> PackageStateUpdated = delegate { };
        public event Action<BotModelInfo> BotStateUpdated = delegate { };
        public event Action<AccountModelInfo> AccountStateUpdated = delegate { };


        public BotAgentServer(IServiceProvider services, IConfiguration serverConfig)
        {
            _botAgent = services.GetRequiredService<IBotAgent>();
            _serverCreds = serverConfig.GetCredentials();

            if (_serverCreds == null)
                throw new Exception("Server credentials not found");

            _botAgent.AccountChanged += OnAccountChanged;
            _botAgent.BotChanged += OnBotChanged;
            _botAgent.PackageChanged += OnPackageChanged;
            _botAgent.BotStateChanged += OnBotStateChanged;
            _botAgent.AccountStateChanged += OnAccountStateChanged;
        }


        public bool ValidateCreds(string login, string password)
        {
            return _serverCreds.Login == login && _serverCreds.Password == password;
        }

        public List<AccountModelInfo> GetAccountList()
        {
            return _botAgent.GetAccounts();
        }

        public List<BotModelInfo> GetBotList()
        {
            return _botAgent.GetTradeBots();
        }

        public List<PackageInfo> GetPackageList()
        {
            return _botAgent.GetPackages();
        }

        public ApiMetadataInfo GetApiMetadata()
        {
            return ApiMetadataInfo.CreateCurrentMetadata();
        }

        public MappingCollectionInfo GetMappingsInfo()
        {
            return _botAgent.GetMappingsInfo();
        }

        public SetupContextInfo GetSetupContext()
        {
            return new SetupContextInfo(_agentContext.DefaultTimeFrame, _agentContext.DefaultSymbolCode, _agentContext.DefaultMapping);
        }

        public AccountMetadataInfo GetAccountMetadata(AccountKey account)
        {
            var error = _botAgent.GetAccountMetadata(account, out var accountMetadata);
            if (error.Code != ConnectionErrorCodes.None)
                throw new Exception($"Account {account.Login} at {account.Server} failed to connect");
            return accountMetadata;
        }

        public void StartBot(string botId)
        {
            _botAgent.StartBot(botId);
        }

        public void StopBot(string botId)
        {
            _botAgent.StopBotAsync(botId);
        }

        public void AddBot(AccountKey account, PluginConfig config)
        {
            _botAgent.AddBot(account, (TradeBotConfig)config);
        }

        public void RemoveBot(string botId, bool cleanLog, bool cleanAlgoData)
        {
            _botAgent.RemoveBot(botId, cleanLog, cleanAlgoData);
        }

        public void ChangeBotConfig(string botId, PluginConfig newConfig)
        {
            _botAgent.ChangeBotConfig(botId, (TradeBotConfig)newConfig);
        }

        public void AddAccount(AccountKey account, string password, bool useNewProtocol)
        {
            _botAgent.AddAccount(account, password, useNewProtocol);
        }

        public void RemoveAccount(AccountKey account)
        {
            _botAgent.RemoveAccount(account);
        }

        public void ChangeAccount(AccountKey account, string password, bool useNewProtocol)
        {
            _botAgent.ChangeAccount(account, password, useNewProtocol);
        }

        public ConnectionErrorInfo TestAccount(AccountKey account)
        {
            return _botAgent.TestAccount(account);
        }

        public ConnectionErrorInfo TestAccountCreds(AccountKey account, string password, bool useNewProtocol)
        {
            return _botAgent.TestCreds(account, password, useNewProtocol);
        }

        public void UploadPackage(string fileName, byte[] packageBinary)
        {
            _botAgent.UpdatePackage(packageBinary, fileName);
        }

        public void RemovePackage(PackageKey package)
        {
            _botAgent.RemovePackage(package);
        }

        public byte[] DownloadPackage(PackageKey package)
        {
            return _botAgent.DownloadPackage(package);
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
    }
}
