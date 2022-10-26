using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Runtime;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AlgoServerPrivate : IRpcHost, IRuntimeOwner, IPluginHost
    {
        private static readonly int _serverProcId = Process.GetCurrentProcess().Id;

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

        internal void OnRuntimeInvalid(string pkgId, string runtimeId) => _server.Tell(new PkgRuntimeInvalidMsg(pkgId, runtimeId));

        internal Task<IActorRef> GetRuntime(string id) => _server.Ask<IActorRef>(new RuntimeRequest(id));

        internal Task<string> GetPkgRuntimeId(string pkgId) => _server.Ask<string>(new PkgRuntimeIdRequest(pkgId));

        internal Task<IActorRef> GetAccountControl(string accId) => _server.Ask<IActorRef>(new AccountControlRequest(accId));


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


        #region IRuntimeOwner implementation

        string IRuntimeOwner.RuntimeExePath => Env.RuntimeExePath;

        string IRuntimeOwner.WorkingDirectory => Env.AppFolder;

        bool IRuntimeOwner.EnableDevMode => RuntimeSettings.EnableDevMode;

        RpcProxyParams IRuntimeOwner.GetRpcParams() => new() { Address = Address, Port = BoundPort, ParentProcId = _serverProcId };

        void IRuntimeOwner.OnRuntimeStopped(string runtimeId) => _server.Tell(new RuntimeStoppedMsg(runtimeId));

        void IRuntimeOwner.OnRuntimeInvalid(string pkgId, string runtimeId) => _server.Tell(new PkgRuntimeInvalidMsg(pkgId, runtimeId));

        #endregion IRuntimeOwner implementation


        #region IPluginHost implementation

        Task IPluginHost.UpdateRunningState(string pluginId, bool isRunning) => SavedState.SetPluginRunning(pluginId, isRunning);

        Task IPluginHost.UpdateSavedState(PluginSavedState savedState) => SavedState.UpdatePlugin(savedState);

        void IPluginHost.OnPluginUpdated(PluginModelUpdate update) => SendUpdate(update);

        void IPluginHost.OnPluginStateUpdated(PluginStateUpdate update) => SendUpdate(update);

        void IPluginHost.OnPluginAlert(string pluginId, PluginLogRecord record) => Alerts.SendPluginAlert(pluginId, record);

        void IPluginHost.OnGlobalAlert(string msg) => Alerts.SendServerAlert(msg);

        Task<string> IPluginHost.GetPkgRuntimeId(string pkgId) => GetPkgRuntimeId(pkgId);

        Task<IActorRef> IPluginHost.GetRuntime(string runtimeId) => GetRuntime(runtimeId);

        ExecutorConfig IPluginHost.CreateExecutorConfig(string pluginId, string accId, PluginConfig pluginConfig)
        {
            var config = new ExecutorConfig { Id = pluginId, AccountId = accId, IsLoggingEnabled = true, PluginConfig = Any.Pack(pluginConfig) };
            config.WorkingDirectory = Env.GetPluginWorkingFolder(pluginId);
            config.LogDirectory = Env.GetPluginLogsFolder(pluginId);
            config.InitPriorityInvokeStrategy();
            config.InitSlidingBuffering(4000);
            config.InitBarStrategy(Feed.Types.MarketSide.Bid);

            return config;
        }

        #endregion IPluginHost implementation


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

        internal class PkgRuntimeInvalidMsg
        {
            public string PkgId { get; }

            public string RuntimeId { get; }

            public PkgRuntimeInvalidMsg(string pkgId, string runtimeId)
            {
                PkgId = pkgId;
                RuntimeId = runtimeId;
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
            new(loggerId)
            {
                ConnectionSettings = new ConnectionSettings
                {
                    AccountFactory = KnownAccountFactories.Fdk2,
                    Options = AccountOptions,
                },

                Monitoring = new AccountMonitoringSettings
                {
                    NotificationMethod = Alerts.SendMonitoringAlert,

                    EnableQuoteMonitoring = MonitoringSettings.QuoteMonitoring.EnableMonitoring,
                    AccetableQuoteDelay = MonitoringSettings.QuoteMonitoring.AccetableQuoteDelay,
                    AlertsDelay = MonitoringSettings.QuoteMonitoring.AlertsDelay,
                },
            };
    }
}
