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
    internal class RemoteAgent : IAlgoAgent
    {
        private ProtocolClient _protocolClient;
        private PluginCatalog _catalog;
        private PluginIdProvider _idProvider;


        public BotAgentModel BotAgent { get; }


        public RemoteAgent(ProtocolClient protocolClient, BotAgentModel botAgent)
        {
            _protocolClient = protocolClient;
            BotAgent = botAgent;

            _catalog = new PluginCatalog(BotAgent.Library);
            _idProvider = new PluginIdProvider();

            Accounts = BotAgent.Accounts.TransformToList((k, v) => k);
        }


        public Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            return _protocolClient.TestAccount(account);
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

        public Task RemoveBot(string instanceId)
        {
            return _protocolClient.RemoveBot(instanceId);
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            return _protocolClient.ChangeBotConfig(instanceId, newConfig);
        }


        #region IAlgoAgent implementation

        public string Name => _protocolClient.SessionSettings.ServerAddress;

        public IVarList<AccountKey> Accounts { get; }

        public IAlgoLibrary Library => BotAgent.Library;

        public PluginCatalog Catalog => _catalog;

        public IPluginIdProvider IdProvider => _idProvider;


        public async Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(account);
            return new SetupMetadata(BotAgent.ApiMetadata, BotAgent.Mappings, accountMetadata, BotAgent.SetupContext);
        }

        public Task<bool> AddOrUpdatePlugin(PluginConfig config, bool start)
        {
            throw new NotImplementedException();
        }

        #endregion IAlgoAgent implementation
    }
}
