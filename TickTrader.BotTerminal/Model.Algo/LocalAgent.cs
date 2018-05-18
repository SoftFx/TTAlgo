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
        private VarList<AccountKey> _accounts;
        private SetupMetadataInfo _setupMetadataInfo;


        public AlgoEnvironment AlgoEnv { get; }

        public TraderClientModel ClientModel { get; }

        public BotManager BotManager { get; }


        public LocalAgent(AlgoEnvironment algoEnv, TraderClientModel clientModel, BotManager botManager)
        {
            AlgoEnv = algoEnv;
            ClientModel = clientModel;
            BotManager = botManager;

            _accounts = new VarList<AccountKey>();
            _setupMetadataInfo = new SetupMetadataInfo(ApiMetadataInfo.CreateCurrentMetadata(), AlgoEnv.Mappings.ToInfo());

            ClientModel.Connected += ClientModelOnConnected;
        }



        #region IAlgoAgent implementation

        public string Name => "Bot Terminal";

        public IVarList<AccountKey> Accounts => _accounts;

        public IAlgoLibrary Library => AlgoEnv.Library;

        public PluginCatalog Catalog => AlgoEnv.Repo;

        public IPluginIdProvider IdProvider => AlgoEnv.IdProvider;


        public Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = new AccountMetadataInfo(new AccountKey(ClientModel.Connection.CurrentServer,
                ClientModel.Connection.CurrentLogin), ClientModel.ObservableSymbolList.Select(s => new SymbolInfo(s.Name)).ToList());
            var res = new SetupMetadata(_setupMetadataInfo, accountMetadata, setupContext ?? BotManager.GetSetupContextInfo());
            return Task.FromResult(res);
        }

        public Task<bool> AddOrUpdatePlugin(PluginConfig config, bool start)
        {
            throw new NotImplementedException();
        }


        private void ClientModelOnConnected()
        {
            _accounts.Clear();
            _accounts.Add(new AccountKey(ClientModel.Connection.CurrentServer, ClientModel.Connection.CurrentLogin));
        }

        #endregion IAlgoAgent implementation


        #region IAlgoSetupMetadata implementation

        public MappingCollection Mappings => AlgoEnv.Mappings;

        public IReadOnlyList<ISymbolInfo> Symbols => ClientModel.ObservableSymbolList;

        #endregion IAlgoSetupMetadata implementation
    }
}
