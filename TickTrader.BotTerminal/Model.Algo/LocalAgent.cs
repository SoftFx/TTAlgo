using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class LocalAgent : IAlgoAgent, IAlgoSetupMetadata
    {
        private AlgoEnvironment _algoEnv;
        private TraderClientModel _clientModel;
        private BotManager _botManager;
        private VarList<AccountKey> _accounts;
        private SetupMetadataInfo _setupMetadataInfo;


        public string Name => "Bot Terminal";

        public IVarList<AccountKey> Accounts => _accounts;

        public IAlgoLibrary Library => _algoEnv.Library;

        public PluginCatalog Catalog => _algoEnv.Repo;

        public IPluginIdProvider IdProvider => _algoEnv.IdProvider;

        public MappingCollection Mappings => _algoEnv.Mappings;

        public IReadOnlyList<ISymbolInfo> Symbols => _clientModel.ObservableSymbolList;


        public LocalAgent(AlgoEnvironment algoEnv, TraderClientModel clientModel, BotManager botManager)
        {
            _algoEnv = algoEnv;
            _clientModel = clientModel;
            _botManager = botManager;

            _accounts = new VarList<AccountKey>();
            _setupMetadataInfo = new SetupMetadataInfo(ApiMetadataInfo.CreateCurrentMetadata(), _algoEnv.Mappings.ToInfo());

            _clientModel.Connected += ClientModelOnConnected;
        }


        public Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = new AccountMetadataInfo(new AccountKey(_clientModel.Connection.CurrentServer,
                _clientModel.Connection.CurrentLogin), _clientModel.ObservableSymbolList.Select(s => new SymbolInfo(s.Name)).ToList());
            var res = new SetupMetadata(_setupMetadataInfo, accountMetadata, setupContext ?? _botManager.GetSetupContextInfo());
            return Task.FromResult(res);
        }

        public Task<bool> AddOrUpdatePlugin(PluginConfig config, bool start)
        {
            throw new NotImplementedException();
        }


        private void ClientModelOnConnected()
        {
            _accounts.Clear();
            _accounts.Add(new AccountKey(_clientModel.Connection.CurrentServer, _clientModel.Connection.CurrentLogin));
        }
    }
}
