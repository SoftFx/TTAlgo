using Google.Protobuf;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class AlgoServerPrivate : IRpcHost
    {
        private readonly IActorRef _server, _eventBus;


        public string Address { get; set; }

        public int BoundPort { get; set; }

        public EnvService Env { get; }

        public ServerStateModel SavedState { get; }

        public AlertManagerModel Alerts { get; }

        public ConnectionOptions AccountOptions { get; set; }

        public RuntimeSettings RuntimeSettings { get; set; }

        public MonitoringSettings MonitoringSettings { get; set; }


        public AlgoServerPrivate(IActorRef server, EnvService env, IActorRef eventBus, ServerStateModel savedState, AlertManagerModel alerts)
        {
            _server = server;
            Env = env;
            _eventBus = eventBus;
            SavedState = savedState;
            Alerts = alerts;
        }


        internal void SendUpdate(IMessage update) => _eventBus.Tell(update);

        internal void OnRuntimeStopped(string runtimeId) => _server.Tell(new RuntimeStoppedMsg(runtimeId));

        internal Task<IActorRef> GetRuntime(string id) => _server.Ask<IActorRef>(new RuntimeRequest(id));

        internal Task<string> GetPkgRuntimeId(string pkgId) => _server.Ask<string>(new PkgRuntimeIdRequest(pkgId));

        internal Task<AccountControlModel> GetAccountControl(string accId) => _server.Ask<AccountControlModel>(new AccountControlRequest(accId));


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            switch (protocol.Url)
            {
                case KnownProtocolUrls.RuntimeV1:
                    return new ServerRuntimeV1Handler(this);
            }
            return null;
        }

        #endregion IRpcHost implementation


        internal class RuntimeRequest
        {
            public string Id { get; }

            public RuntimeRequest(string id)
            {
                Id = id;
            }
        }

        internal class PkgRuntimeIdRequest
        {
            public string PkgId { get; }

            public PkgRuntimeIdRequest(string pkgId)
            {
                PkgId = pkgId;
            }
        }

        internal class RuntimeStoppedMsg
        {
            public string Id { get; }

            public RuntimeStoppedMsg(string id)
            {
                Id = id;
            }
        }

        internal class AccountControlRequest
        {
            public string Id { get; }

            public AccountControlRequest(string id)
            {
                Id = id;
            }
        }

        internal AccountModelSettings GetDefaultClientSettings(string loggerId) =>
            new AccountModelSettings(loggerId)
            {
                ConnectionSettings = new ConnectionSettings
                {
                    AccountFactory = KnownAccountFactories.Fdk2,
                    Options = AccountOptions,
                },

                HistoryProviderSettings = new HistoryProviderSettings
                {
                    FolderPath = Env.FeedHistoryCacheFolder,
                    Options = FeedHistoryFolderOptions.ServerClientHierarchy,
                },

                Monitoring = new AccountMonitoringSettings
                {
                    NotificationMethod = Alerts.SendServerAlert,

                    EnableQuoteMonitoring = MonitoringSettings.QuoteMonitoring.EnableMonitoring,
                    AccetableQuoteDelay = MonitoringSettings.QuoteMonitoring.AccetableQuoteDelay,
                    AlertsDelay = MonitoringSettings.QuoteMonitoring.AlertsDelay,
                },
            };
    }
}
