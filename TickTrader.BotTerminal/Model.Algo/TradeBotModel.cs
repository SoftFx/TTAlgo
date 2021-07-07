using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using System.IO;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal interface ITradeBot
    {
        bool IsRemote { get; }

        string InstanceId { get; }

        PluginConfig Config { get; }

        PluginModelInfo.Types.PluginState State { get; }

        string FaultMessage { get; }

        PluginDescriptor Descriptor { get; }

        string Status { get; }

        BotJournal Journal { get; }

        string AccountId { get; }


        event Action<ITradeBot> Updated;
        event Action<ITradeBot> StateChanged;
        event Action<ITradeBot> StatusChanged;


        void SubscribeToStatus();

        void UnsubscribeFromStatus();

        void SubscribeToLogs();

        void UnsubscribeFromLogs();
    }

    internal class TradeBotModel : PluginModel, IBotWriter, ITradeBot
    {
        private BotListenerProxy _botListener;

        public bool IsRemote => false;
        public string Status { get; private set; }
        public BotJournal Journal { get; }
        public string AccountId { get; }

        public event Action<ITradeBot> StatusChanged = delegate { };
        public event Action<ITradeBot> StateChanged = delegate { };
        public event Action<ITradeBot> Updated = delegate { };

        public TradeBotModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext, string accountId)
            : base(config, agent, host, setupContext)
        {
            AccountId = accountId;
            Journal = new BotJournal(InstanceId, true);
            host.Connected += Host_Connected;
            host.Disconnected += Host_Disconnected;
        }

        internal void Abort()
        {
            if (State == PluginModelInfo.Types.PluginState.Stopping)
                AbortExecutor();
        }

        public async Task Start()
        {
            if (!State.CanStart())
                return;

            if (await StartExcecutor())
            {
                _botListener?.Start();
                ChangeState(PluginModelInfo.Types.PluginState.Running);
            }
        }

        public async Task Stop()
        {
            if (!State.CanStop())
                return;

            if (await StopExecutor())
            {
                _botListener?.Stop();
                _botListener?.Dispose();
                ChangeState(PluginModelInfo.Types.PluginState.Stopped);
            }
        }

        protected override void FillExectorConfig(ExecutorConfig config)
        {
            config.WorkingDirectory = Agent.AlgoServer.Env.GetPluginWorkingFolder(InstanceId);
            EnvService.Instance.EnsureFolder(config.WorkingDirectory);
        }

        protected override async Task<ExecutorModel> CreateExecutor()
        {
            var executor = await base.CreateExecutor();

            _botListener = new BotListenerProxy(executor, OnBotExited, this);

            return executor;
        }

        internal override void Configurate(PluginConfig config)
        {
            if (State == PluginModelInfo.Types.PluginState.Broken)
                return;

            if (State.IsStopped())
            {
                base.Configurate(config);

                Updated?.Invoke(this);
            }
            else
                throw new InvalidOperationException("Make sure that the bot is stopped before setting a new configuration");
        }

        protected override void ChangeState(PluginModelInfo.Types.PluginState state, string faultMessage = null)
        {
            base.ChangeState(state, faultMessage);
            StateChanged(this);
        }

        protected override void OnPluginUpdated()
        {
            if (State.IsStopped())
            {
                UpdateRefs();
            }
        }

        protected override void LockResources()
        {
            base.LockResources();
            Host.Lock();
        }

        protected override void UnlockResources()
        {
            base.UnlockResources();
            Host.Unlock();
        }

        protected override void OnRefsUpdated()
        {
            Updated?.Invoke(this);
        }

        protected override IOutputCollector CreateOutputCollector<T>(ExecutorModel executor, IOutputConfig config, OutputDescriptor descriptor)
        {
            return new CachingOutputCollector<T>(executor, config, descriptor);
        }

        private void Host_Connected()
        {
            if (State == PluginModelInfo.Types.PluginState.Reconnecting)
            {
                HandleReconnect();
                ChangeState(PluginModelInfo.Types.PluginState.Running);
            }
        }

        private void Host_Disconnected()
        {
            if (State == PluginModelInfo.Types.PluginState.Running)
            {
                HandleDisconnect();
                ChangeState(PluginModelInfo.Types.PluginState.Reconnecting);
            }
        }

        private BotMessage Convert(PluginLogRecord record)
        {
            return BotMessage.Create(record, InstanceId);
        }

        private void OnBotExited()
        {
            Execute.OnUIThread(() =>
            {
                if (State == PluginModelInfo.Types.PluginState.Running)
                {
                    ChangeState(PluginModelInfo.Types.PluginState.Stopped);
                    UnlockResources();
                }
            });
        }

        #region ITradeBot stubs

        public void SubscribeToStatus() { }

        public void UnsubscribeFromStatus() { }

        public void SubscribeToLogs() { }

        public void UnsubscribeFromLogs() { }

        public void SubscribeToAlerts() { }

        public void UnsubscribeFromAlerts() { }

        #endregion

        #region IBotWriter implementation

        void IBotWriter.LogMesssage(PluginLogRecord rec)
        {
            Journal.Add(Convert(rec));

            if (rec.Severity == PluginLogRecord.Types.LogSeverity.Alert)
                AlertModel.AddAlert(InstanceId, rec);
        }

        void IBotWriter.UpdateStatus(string status)
        {
            Status = status;
            StatusChanged?.Invoke(this);
        }

        void IBotWriter.Trace(string status)
        {
            Journal.LogStatus(status);
        }

        #endregion IBotWriter implementation
    }
}
