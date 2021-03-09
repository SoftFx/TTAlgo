using Caliburn.Micro;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Common.Model.Setup;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using System.Collections.Generic;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal interface ITradeBot
    {
        bool IsRemote { get; }

        string InstanceId { get; }

        PluginConfig Config { get; }

        PluginStates State { get; }

        string FaultMessage { get; }

        PluginDescriptor Descriptor { get; }

        string Status { get; }

        BotJournal Journal { get; }

        AccountKey Account { get; }


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
        public AccountKey Account { get; }

        public event Action<ITradeBot> StatusChanged = delegate { };
        public event Action<ITradeBot> StateChanged = delegate { };
        public event Action<ITradeBot> Updated = delegate { };

        public TradeBotModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext, AccountKey account)
            : base(config, agent, host, setupContext)
        {
            Account = account;
            Journal = new BotJournal(InstanceId, true);
            host.Connected += Host_Connected;
            host.Disconnected += Host_Disconnected;
        }

        internal void Abort()
        {
            if (State == PluginStates.Stopping)
                AbortExecutor();
        }

        public async Task Start()
        {
            if (!PluginStateHelper.CanStart(State))
                return;

            if (await StartExcecutor())
            {
                _botListener?.Start();
                ChangeState(PluginStates.Running);
            }
        }

        public async Task Stop()
        {
            if (!PluginStateHelper.CanStop(State))
                return;

            if (await StopExecutor())
            {
                _botListener?.Stop();
                ChangeState(PluginStates.Stopped);
            }
        }

        protected override async Task<ExecutorModel> CreateExecutor()
        {
            var executor = await base.CreateExecutor();
            executor.Config.WorkingDirectory = Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(InstanceId));
            EnvService.Instance.EnsureFolder(executor.Config.WorkingDirectory);

            executor.Config.IsLoggingEnabled = true;
            _botListener = new BotListenerProxy(executor, OnBotExited, this);
            return executor;
        }

        internal override void Configurate(PluginConfig config)
        {
            if (State == PluginStates.Broken)
                return;

            if (PluginStateHelper.IsStopped(State))
            {
                base.Configurate(config);

                Updated?.Invoke(this);
            }
            else
                throw new InvalidOperationException("Make sure that the bot is stopped before setting a new configuration");
        }

        protected override void ChangeState(PluginStates state, string faultMessage = null)
        {
            base.ChangeState(state, faultMessage);
            StateChanged(this);
        }

        protected override void OnPluginUpdated()
        {
            if (PluginStateHelper.IsStopped(State))
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
            if (State == PluginStates.Reconnecting)
            {
                HandleReconnect();
                ChangeState(PluginStates.Running);
            }
        }

        private void Host_Disconnected()
        {
            if (State == PluginStates.Running)
            {
                HandleDisconnect();
                ChangeState(PluginStates.Reconnecting);
            }
        }

        private BotMessage Convert(UnitLogRecord record)
        {
            return BotMessage.Create(record, InstanceId);
        }

        private void OnBotExited()
        {
            Execute.OnUIThread(() =>
            {
                if (State == PluginStates.Running)
                {
                    ChangeState(PluginStates.Stopped);
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

        void IBotWriter.LogMesssage(UnitLogRecord rec)
        {
            Journal.Add(Convert(rec));

            if (rec.Severity == UnitLogRecord.Types.LogSeverity.Alert)
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
