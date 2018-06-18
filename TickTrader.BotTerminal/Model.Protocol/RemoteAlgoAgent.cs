using Machinarium.Qnil;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoAgent : IAlgoAgent
    {
        private ProtocolClient _protocolClient;
        private BotAgentModel _botAgent;
        private PluginIdProvider _idProvider;


        public string Name => _protocolClient.SessionSettings.ServerAddress;

        public IVarSet<PackageKey, PackageInfo> Packages => _botAgent.Packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _botAgent.Plugins;

        public IVarSet<AccountKey, AccountModelInfo> Accounts => _botAgent.Accounts;

        public IVarSet<string, BotModelInfo> Bots => _botAgent.Bots;

        public PluginCatalog Catalog { get; }

        public IPluginIdProvider IdProvider => _idProvider;


        public event Action<PackageInfo> PackageStateChanged;

        public event Action<AccountModelInfo> AccountStateChanged;

        public event Action<BotModelInfo> BotStateChanged;


        public RemoteAlgoAgent(ProtocolClient protocolClient, BotAgentModel botAgent)
        {
            _protocolClient = protocolClient;
            _botAgent = botAgent;

            Catalog = new PluginCatalog(this);
            _idProvider = new PluginIdProvider();

            _botAgent.PackageStateChanged += OnPackageStateChanged;
            _botAgent.AccountStateChanged += OnAccountStateChanged;
            _botAgent.BotStateChanged += OnBotStateChanged;
        }


        public async Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(account);
            return new SetupMetadata(_botAgent.ApiMetadata, _botAgent.Mappings, accountMetadata, _botAgent.SetupContext);
        }

        public Task StartBot(string instanceId)
        {
            return _protocolClient.StartBot(instanceId);
        }

        public Task StopBot(string instanceId)
        {
            return _protocolClient.StopBot(instanceId);
        }

        public Task AddBot(AccountKey account, PluginConfig config)
        {
            return _protocolClient.AddBot(account, config);
        }

        public Task RemoveBot(string instanceId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            return _protocolClient.RemoveBot(instanceId, cleanLog, cleanAlgoData);
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            return _protocolClient.ChangeBotConfig(instanceId, newConfig);
        }

        public Task AddAccount(AccountKey account, string password, bool useNewProtocol)
        {
            return _protocolClient.AddAccount(account, password, useNewProtocol);
        }

        public Task RemoveAccount(AccountKey account)
        {
            return _protocolClient.RemoveAccount(account);
        }

        public Task ChangeAccount(AccountKey account, string password, bool useNewProtocol)
        {
            return _protocolClient.ChangeAccount(account, password, useNewProtocol);
        }

        public Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            return _protocolClient.TestAccount(account);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password, bool useNewProtocol)
        {
            return _protocolClient.TestAccountCreds(account, password, useNewProtocol);
        }

        public Task UploadPackage(string fileName, byte[] packageBinary)
        {
            return _protocolClient.UploadPackage(fileName, packageBinary);
        }

        public Task RemovePackage(PackageKey package)
        {
            return _protocolClient.RemovePackage(package);
        }

        public Task<byte[]> DownloadPackage(PackageKey package)
        {
            return _protocolClient.DownloadPackage(package);
        }


        private void OnPackageStateChanged(PackageInfo package)
        {
            PackageStateChanged?.Invoke(package);
        }

        private void OnAccountStateChanged(AccountModelInfo account)
        {
            AccountStateChanged?.Invoke(account);
        }

        private void OnBotStateChanged(BotModelInfo bot)
        {
            BotStateChanged?.Invoke(bot);
        }
    }
}
