using Google.Protobuf;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Account.Settings;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    internal class AlgoServerPrivate : IPluginHost
    {
        private readonly IActorRef _server, _eventBus;


        public EnvService Env { get; }

        public ServerStateModel SavedState { get; }

        public AlertManagerModel Alerts { get; }

        public ConnectionOptions AccountOptions { get; set; }

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


        #region IPluginHost implementation

        Task IPluginHost.UpdateRunningState(string pluginId, bool isRunning) => SavedState.SetPluginRunning(pluginId, isRunning);

        Task IPluginHost.UpdateSavedState(PluginSavedState savedState) => SavedState.UpdatePlugin(savedState);

        void IPluginHost.OnPluginUpdated(PluginModelUpdate update) => SendUpdate(update);

        void IPluginHost.OnPluginStateUpdated(PluginStateUpdate update) => SendUpdate(update);

        void IPluginHost.OnPluginAlert(string pluginId, PluginLogRecord record) => Alerts.SendPluginAlert(pluginId, record);

        void IPluginHost.OnGlobalAlert(string msg) => Alerts.SendServerAlert(msg);

        Task<string> IPluginHost.GetPkgRuntimeId(string pkgId) => RuntimeServerModel.GetPkgRuntimeId(_server, pkgId);

        Task<IActorRef> IPluginHost.GetRuntime(string runtimeId) => RuntimeServerModel.GetRuntime(_server, runtimeId);

        ExecutorConfig IPluginHost.CreateExecutorConfig(string pluginId, string accId, PluginConfig pluginConfig)
        {
            var config = new ExecutorConfig { Id = pluginId, AccountId = accId, IsLoggingEnabled = true };
            config.SetPluginConfig(pluginConfig);
            config.WorkingDirectory = Env.GetPluginWorkingFolder(pluginId);
            config.LogDirectory = Env.GetPluginLogsFolder(pluginId);
            config.InitPriorityInvokeStrategy();
            config.InitSlidingBuffering(512);
            config.InitBarStrategy(Feed.Types.MarketSide.Bid);

            return config;
        }

        #endregion IPluginHost implementation


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
