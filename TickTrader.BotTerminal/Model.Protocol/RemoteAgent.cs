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


        public void StartBot(string instanceId)
        {
            _protocolClient.StartBot(instanceId);
        }

        public void StopBot(string instanceId)
        {
            _protocolClient.StopBot(instanceId);
        }

        public void AddBot(AccountKey account, PluginConfig config)
        {
            _protocolClient.AddBot(account, config);
        }

        public void RemoveBot(string instanceId)
        {
            _protocolClient.RemoveBot(instanceId);
        }

        public void ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            _protocolClient.ChangeBotConfig(instanceId, newConfig);
        }


        #region IAlgoAgent implementation

        public string Name => _protocolClient.SessionSettings.ServerAddress;

        public IVarList<AccountKey> Accounts { get; }

        public IAlgoLibrary Library => BotAgent.Library;

        public PluginCatalog Catalog => _catalog;

        public IPluginIdProvider IdProvider => _idProvider;


        public Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddOrUpdatePlugin(PluginConfig config, bool start)
        {
            throw new NotImplementedException();
        }

        #endregion IAlgoAgent implementation
    }
}
